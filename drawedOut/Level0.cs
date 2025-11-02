using System.Diagnostics;

// TODO: move all const values into global
namespace drawedOut
{
    public partial class Level0 : Form
    {
        private static Player playerBox = new Player(
            origin: new Point(750, 250),
            width: 100,
            height: 260,
            attackPower: 1,
            energy: 100,
            maxHp: 6);

        private static Platform box2 = new(
           origin: new Point(1, 1050),
           width: 5400,
           height: 550,
           isMainPlat: true);

         private static Platform box3 = new(
           origin: new Point(300, 200),
           width: 400,
           height: 175);

         private static Platform box4 = new(
           origin: new Point(1000, 400),
           width: 200,
           height: 300);

        private static Platform box5 = new(
           origin: new Point(1500, 400),
           width: 200,
           height: 300);

        private static Brush playerBrush; // TODO: remove when player sprites is added

        private static HpBarUI hpBar = new HpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    maxHp: 6);


        private static Dictionary<Entity, PointF> zoomOrigins = new Dictionary<Entity, PointF>();
        private static Dictionary<Character, Bitmap?> characterAnimations = new Dictionary<Character, Bitmap?>();

        private static Keys? prevLeftRight;

        private static bool
            gameTickEnabled = true,
            showHitbox = true,

            movingLeft = false,
            movingRight = false,
            jumping = false,

            playerIsHit = false,
            isParrying = false,

            isPaused = false,
            slowedMov = false;


        private static int
            gameTickFreq = 160,
            gameTickInterval;


        private const float 
            ZOOM_FACTOR = 1.2F,
            SLOW_FACTOR = 1.5F,
            SLOW_DURATION_S = 0.35F,

            FREEZE_DURATION_S = 0.15F,

            ANIMATION_FPS = 1000/24F,

            PARRY_DURATION_S = 0.45F,
            PERFECT_PARRY_WINDOW_S = 0.05F,
            PARRY_ENDLAG_S = 0.2F;

        private static float
            parryWindowS,
            endLagTime,

            curZoom = 1,
            // TODO: redo freeze and slow logic
            slowTimeS = 0,
            freezeTimeS = 0;

        // threading 
        // used for closing the thread
        private static CancellationTokenSource cancelTokenSrc = new CancellationTokenSource(); 
        private static Thread gameTickThread;
        private static ParallelOptions threadSettings = new ParallelOptions();
        private static Stopwatch deltaTimeSW = new Stopwatch();

        public Level0()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            Global.CalcNewCenter();

            threadSettings.MaxDegreeOfParallelism = 4;
            threadSettings.CancellationToken = cancelTokenSrc.Token;

            // set height and width of window
            Width = Global.LevelSize.Width;
            Height = Global.LevelSize.Height;

            // sets the refresh interval
            gameTickInterval = (int)(1000.0F / gameTickFreq);

            Stopwatch threadDelaySW = Stopwatch.StartNew();
            Stopwatch animTickSW = Stopwatch.StartNew();
            CancellationToken threadCT = cancelTokenSrc.Token;

            characterAnimations.Add(playerBox, playerBox.NextAnimFrame());

            foreach (Enemy e in Enemy.InactiveEnemyList)
                characterAnimations.Add(e, e.NextAnimFrame());

            gameTickThread = new Thread(() =>
            {
                while (gameTickEnabled)
                {
                    if (threadCT.IsCancellationRequested)
                        return;

                    if (animTickSW.Elapsed.TotalMilliseconds >= ANIMATION_FPS)
                    {
                        TickAnimations();
                        animTickSW.Restart();
                        GC.Collect();
                    }

                    if (threadDelaySW.Elapsed.TotalMilliseconds >= gameTickInterval)
                    {
                        double deltaTime = getDeltaTime();
                        threadDelaySW.Restart();

                        if (isPaused) continue;

                        movementTick(deltaTime);
                        attackHandler(deltaTime); 

                        try { Invoke(renderGraphics); } 
                        catch (ObjectDisposedException) { return; }
                    }
                }
            });

        }


        private void TickAnimations()
        {
            foreach (KeyValuePair<Character, Bitmap?> c in characterAnimations)
                characterAnimations[c.Key] = c.Key.NextAnimFrame();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            hpBar.UpdateMaxHp(playerBox.MaxHp);

            playerBrush = Brushes.Blue;

            deltaTimeSW.Start();

            if (gameTickThread is null) throw new Exception("gameTickThread not initialsed");
            gameTickThread.Start();

            fpsTimer.Start();

            togglePause(false);
        }


        private double getDeltaTime()
        {
            double deltaTime = deltaTimeSW.Elapsed.TotalSeconds;
            deltaTimeSW.Restart();

            isPaused = false;
            if (freezeTimeS > 0)
            {
                freezeTimeS -= (float)deltaTime;
                isPaused = true;
            }
            return deltaTime;
        }


        private void slowTime(double deltaTime)
        {
            if (slowTimeS > 0)
            {
                slowedMov = true;

                if (ZOOM_FACTOR <= 1)
                    throw new ArgumentException("ZOOM_FACTOR must be bigger than 1");

                slowTimeS -= (float)deltaTime;

                deltaTime /= SLOW_FACTOR;
            }
            else 
            {
                slowTimeS = 0;
                if (slowedMov) // keeps the player in motion when in slow 
                {
                    movingLeft = false;
                    movingRight = false;
                    slowedMov = false;
                }
            }
        }


        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static List<Projectile> disposedProjectiles = new List<Projectile>();

        private void attackHandler(double deltaTime)
        {
            playerIsHit = false;
            freezeTimeS = 0;

            Attacks.UpdateHitboxes();

            if (endLagTime > 0)
            {
                endLagTime -= (float)deltaTime;
                isParrying = false;
            }

            try { CheckProjectileCollisions(deltaTime); }
            catch (OperationCanceledException) { return; }

            // ticks down the parry window
            if (isParrying && parryWindowS > 0)
                parryWindowS -= (float)deltaTime;
            if (parryWindowS < 0)
            {
                endLagTime = PARRY_ENDLAG_S * 10;
                parryWindowS = 0;
            }


            if (playerBox.Hp <= 0)
            {
                togglePause(true);
                MessageBox.Show("you are dead");
                Application.Exit();
            }
        }


        private void CheckProjectileCollisions(double dt)
        {
            if (Projectile.ProjectileList.Count == 0) return;
            Parallel.ForEach(Projectile.ProjectileList, threadSettings, bullet =>
            {
                bullet.moveProjectile(dt);
                PointF bLoc = bullet.Center;

                if (disposedProjectiles.Contains(bullet)) return;

                foreach (Platform p in Platform.ActivePlatformList)
                {
                    if (!(p.Hitbox.IntersectsWith(bullet.Hitbox)))
                    continue;

                    disposedProjectiles.Add(bullet);
                    break;
                }


                if ((bLoc.X < 0) || (bLoc.Y < 0) || (bLoc.X > ClientSize.Width) || (bLoc.Y > ClientSize.Height))
                {
                    disposedProjectiles.Add(bullet);
                    return;
                }

                if (!playerBox.Hitbox.IntersectsWith(bullet.Hitbox))
                    return;

                if (!isParrying)
                {
                    freezeTimeS = FREEZE_DURATION_S * 10;
                    disposedProjectiles.Add(bullet);
                    playerBox.DoDamage(1, ref hpBar);
                    return;
                }

                bullet.rebound(playerBox.Center); // required to prevent getting hit anyway when parrying

                // TODO: move into player
                // if the current parry has lasted for at most the perfectParryWindow
                if (parryWindowS >= (PARRY_DURATION_S - PERFECT_PARRY_WINDOW_S) * 10)
                {
                    slowTimeS = SLOW_DURATION_S * 10;
                    zoomScreen(ZOOM_FACTOR);
                    isParrying = false;
                    //playerBox.endLagTime = 0; //TODO: parry endlag
                }
                else disposedProjectiles.Add(bullet);

                if (disposedProjectiles.Count == 0) return;
                foreach (Projectile p in disposedProjectiles)
                    Projectile.ProjectileList.Remove(p);

                disposedProjectiles.Clear();


            });
        }



        // pause game
        private void togglePause(bool pause) => isPaused = !isPaused; 


        private static Stopwatch fpsTimer = new Stopwatch();
        // rendering graphics method
        private void renderGraphics()
        {
            if (playerBrush is null) throw new Exception("playerBrush is not defined");

            if (slowTimeS <= 0 && freezeTimeS <= 0 && curZoom != 1)
            {
                unZoomScreen();
                curZoom = 1;
            }

            // debugging/visual indicator for parry
            if (isParrying)
                playerBrush = Brushes.Gray;
            else if (playerIsHit)
                playerBrush = Brushes.Red; // visual hit indicator
            else
                playerBrush = Brushes.Blue;


            float deltaFPSTime = Convert.ToSingle(1/(fpsTimer.Elapsed.TotalSeconds));
            fpsTimer.Restart();
            label1.Text = deltaFPSTime.ToString("F0");
            label2.Text = playerBox.FacingDirection.ToString();
            //label2.Hide();
            //label3.Text = playerBox.CollisionDebugY().ToString();
            label3.Hide();

            Refresh();
        }



        private void zoomScreen(float scaleF)
        {
            curZoom = scaleF;

            float playerX = playerBox.Center.X;
            float playerY = playerBox.Center.Y;

            float midX = Global.CenterOfScreen.X;
            float midY = Global.CenterOfScreen.Y;
            void zoomObj(Entity obj, float x, float y)
            {
                float _xDiff = obj.Center.X - playerX;
                float _yDiff = obj.Center.Y - playerY;

                float newX = x + _xDiff * scaleF;
                float newY = y + _yDiff * scaleF;

                obj.ScaleHitbox(scaleF);
                obj.Center = new PointF (newX, newY);
                this.Invalidate();
            }


            foreach (Entity e in Entity.EntityList)
            {
                zoomOrigins.Add(e,e.Center);
                if (e == playerBox)
                {
                    playerBox.Center = new PointF (Global.CenterOfScreen.X, Global.CenterOfScreen.Y);
                    playerBox.ScaleHitbox(scaleF);
                    continue;
                }
                zoomObj(e, midX, midY); 
            }
        }



        private void unZoomScreen()
        {
            foreach (KeyValuePair<Entity, PointF> EntityPoints in zoomOrigins)
            { 
                Entity obj = EntityPoints.Key;
                PointF point = EntityPoints.Value;
                obj.ResetScale();
                obj.Center = point;
            }

            playerBox.ResetScale();
            zoomOrigins.Clear();
            curZoom = 1; // screen is no longer scaled
        }


        private void movementTick(double deltaTime)
        {
            slowTime(deltaTime);

            foreach (Entity e in Entity.EntityList)
                e.CheckActive();
            
            if (playerBox.IsOnFloor && jumping) { playerBox.DoJump(); }
            Global.XDirections? playerMovDir = null;
            if (movingLeft) playerMovDir = Global.XDirections.left;
            if (movingRight) playerMovDir = Global.XDirections.right;
            
            bool isScrolling = playerBox.CheckScrolling(box2);

            if (isScrolling) ScrollEntities(velocity: playerBox.XVelocity, deltaTime);

            playerBox.MoveCharacter(deltaTime, playerMovDir, doScroll: isScrolling);

            foreach (Platform plat in Platform.ActivePlatformList)
            { playerBox.CheckPlatformCollision(plat); }

            try
            {
                Parallel.ForEach(Enemy.ActiveEnemyList, threadSettings, enemy => {
                    enemy.DoMove( dt: deltaTime, doScroll: isScrolling);
                    foreach (Platform plat in Platform.ActivePlatformList)
                    { enemy.CheckPlatformCollision(plat); }
                });
            }
            catch (OperationCanceledException) { return; }
        }


        public void ScrollEntities(double velocity, double deltaTime)
        {
            foreach( Entity e in Entity.EntityList)
            {
                if (e == playerBox) { continue; }
                e.UpdateX(-velocity * deltaTime);
            }
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (showHitbox)
            {
                //foreach (Enemy e in Enemy.ActiveEnemyList)
                g.DrawRectangle(Pens.Blue, playerBox.Hitbox);

                foreach (Platform plat in Platform.ActivePlatformList)
                {
                    using (Pen redPen = new Pen(Color.Red, 3))
                    { g.DrawRectangle(redPen, plat.Hitbox); }
                }

                foreach (Projectile bullet in Projectile.ProjectileList)
                    g.FillRectangle(Brushes.Red, bullet.Hitbox);

                for (int i = 0; i < hpBar.IconCount; i++)
                    g.FillRectangle(hpBar.HpRecColours[i], hpBar.HpRectangles[i]);
            }

            foreach (KeyValuePair<Character, Bitmap?> img in characterAnimations)
            {
                if (img.Value is null) continue;
                g.DrawImage(img.Value, img.Key.AnimRect);
            }
        }



        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    if (movingRight && !movingLeft && prevLeftRight is null)
                    {
                        prevLeftRight = Keys.D;
                        movingRight = false;
                    }
                    movingLeft = true;
                    break;

                case Keys.D:
                    if (movingLeft && !movingRight && prevLeftRight is null)
                    {
                        prevLeftRight = Keys.A;
                        movingLeft = false;
                    }
                    movingRight = true;
                    break;

                case Keys.W:
                    if (playerBox.IsOnFloor) { jumping = true; }
                    break;
            }
        }


        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (slowTimeS > 0)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                    jumping = false;
                    playerBox.StopJump();
                    break;

                case Keys.A:
                    if (prevLeftRight == Keys.D) movingRight = true;
                    prevLeftRight = null;
                    movingLeft = false;
                    break;

                case Keys.D:
                    if (prevLeftRight == Keys.A) movingLeft = true;
                    prevLeftRight = null;
                    movingRight = false;
                    break;
            }
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // if Not parrying then resets parrywindow and sets to parrying
                case MouseButtons.Right:
                    if (endLagTime <= 0)
                    {
                        parryWindowS = (PARRY_DURATION_S * 10);
                        isParrying = true;
                    }
                    break;
                case MouseButtons.Left:
                    playerBox.DoBasicAttack();
                    break;
            }
        }



        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // stops parrying when mouseup but doesnt reset timer > only on mouse down 
                case MouseButtons.Right:
                    if (isParrying)
                    {
                        endLagTime = PARRY_ENDLAG_S * 10;
                        isParrying = false;
                    }
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            gameTickEnabled = false;    
            cancelTokenSrc.Cancel();
        }
    }
}
