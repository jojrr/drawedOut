using System.Diagnostics;

// TODO: move all const values into global
namespace drawedOut
{
    public partial class Level0 : Form
    {
        private static Player playerBox = new Player(
            origin: new Point(750, 250),
            width: 50,
            height: 50,
            attackPower: 1,
            energy: 100,
            maxHp: 6,
            accel: 100);

        private static Platform box2 = new(
           origin: new Point(1, 650),
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
            gameTickFreq = 60,
            gameTickInterval;


        private const float 
            ZOOM_FACTOR = 1.2F,
            SLOW_FACTOR = 1.5F,
            SLOW_DURATION_S = 0.35F,

            FREEZE_DURATION_S = 0.15F,

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

            threadSettings.MaxDegreeOfParallelism = 4;
            threadSettings.CancellationToken = cancelTokenSrc.Token;

            // set height and width of window
            Width = Global.LevelSize.Width;
            Height = Global.LevelSize.Height;

            // sets the refresh interval
            gameTickInterval = (int)(1000.0F / gameTickFreq);

            Stopwatch threadDelaySW = Stopwatch.StartNew();
            CancellationToken threadCT = cancelTokenSrc.Token;

            gameTickThread = new Thread(() =>
            {
                while (gameTickEnabled)
                {
                    if (threadCT.IsCancellationRequested)
                        return;

                    if (threadDelaySW.Elapsed.TotalMilliseconds >= gameTickInterval)
                    {
                        double deltaTime = getDeltaTime();
                        threadDelaySW.Restart();

                        if (isPaused) continue;

                        // TODO: review delta time logic
                        movementTick(deltaTime);
                        attackHandler(deltaTime); 

                        try { Invoke(renderGraphics); } 
                        catch (ObjectDisposedException) { return; }
                    }
                }
            });

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

            if (endLagTime > 0)
            {
                endLagTime -= (float)deltaTime;
                isParrying = false;
            }

            try { CheckProjectileCollisions(deltaTime); }
            catch (OperationCanceledException) { return; }

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            disposedProjectiles.Clear();

            //if ((bulletInterval > 0) || (deltaTime == 0))
            //    bulletInterval -= (float)deltaTime;
            //else
            //    createBullet();



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

            });
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // if Not parrying then resets parrywindow and sets to parrying
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                if (endLagTime <= 0)
                {
                    parryWindowS = (PARRY_DURATION_S * 10);
                    isParrying = true;
                }
            }
        }



        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            // stops parrying when mouseup but doesnt reset timer > only on mouse down 
            if (e.Button == MouseButtons.Right)
            {
                if (isParrying)
                {
                    endLagTime = PARRY_ENDLAG_S * 10;
                    isParrying = false;
                }
            }
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


            // calculates new position for each projectile based on distance from playerBox center and adjusts for Scale and the "screen" shifting to the center
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

            if (isScrolling) 
                ScrollEntities(velocity: playerBox.XVelocity, deltaTime);

            foreach (Platform plat in Platform.ActivePlatformList)
            { playerBox.CheckPlatformCollision(plat); }

            playerBox.MoveCharacter(deltaTime, playerMovDir, doScroll: isScrolling);

            try
            {
                Parallel.ForEach(Enemy.ActiveEnemyList, threadSettings, enemy => {

                        foreach (Platform plat in Platform.ActivePlatformList)
                        { enemy.CheckPlatformCollision(plat); }

                        enemy.DoMove( dt: deltaTime, doScroll: isScrolling);
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
            if (showHitbox)
            {
                //foreach (Enemy e in Enemy.ActiveEnemyList)
                e.Graphics.FillRectangle(playerBrush, playerBox.Hitbox);

                foreach (Platform plat in Platform.ActivePlatformList)
                {
                    using (Pen redPen = new Pen(Color.Red, 3))
                    { e.Graphics.DrawRectangle(redPen, plat.Hitbox); }
                }

                foreach (Projectile bullet in Projectile.ProjectileList)
                    e.Graphics.FillRectangle(Brushes.Red, bullet.Hitbox);

                for (int i = 0; i < hpBar.IconCount; i++)
                    e.Graphics.FillRectangle(hpBar.HpRecColours[i], hpBar.HpRectangles[i]);
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
                    prevLeftRight = null;
                    movingLeft = false;
                    break;

                case Keys.D:
                    prevLeftRight = null;
                    movingRight = false;
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
