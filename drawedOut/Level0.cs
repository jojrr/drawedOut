using System.Diagnostics;
using InterpolationMode = System.Drawing.Drawing2D.InterpolationMode;

// TODO: move all const values into global
namespace drawedOut
{
    public partial class Level0 : Form
    {
        private static Player playerBox;
        private static MeleeEnemy meleeOne;
        private static Platform mainPlat;
        private static Platform box3;
        private static Platform box4;
        private static Platform box5;

        private static HpBarUI hpBar;
        private static BarUI energyBar;

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
            gameTickFreq = 60,
            gameTickInterval;

        private const float 
            ZOOM_FACTOR = 1.1F,
            SLOW_FACTOR = 3.5F,
            SLOW_DURATION_S = 0.35F,

            FREEZE_DURATION_S = 0.15F,

            ANIMATION_FPS = 1000/24F;


        private static float
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

        private static void InitUI()
        {
            hpBar = new HpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    maxHp: 6);
            hpBar.UpdateMaxHp(playerBox.MaxHp);

            energyBar = new BarUI(
                    origin: new PointF(70, 120),
                    elementWidth: 200,
                    elementHeight: 20,
                    brush: Brushes.Blue,
                    bgBrush: Brushes.Gray,
                    maxVal: 50,
                    borderScale: 0.4f);
        }

        private static void InitEntities()
        {
            playerBox = new Player(
                origin: new Point(850, 550),
                width: 30,
                height: 160,
                attackPower: 1,
                energy: 100,
                hp: 6);

            meleeOne = new MeleeEnemy( origin: new Point(350, 550) );

            mainPlat = new(
               origin: new Point(1, 750),
               width: 5400,
               height: 550,
               isMainPlat: true);

            box3 = new(
               origin: new Point(300, 250),
               width: 400,
               height: 175);

            box4 = new(
               origin: new Point(1000, 550),
               width: 200,
               height: 250);

            box5 = new(
               origin: new Point(1500, 550),
               width: 200,
               height: 250);
        }


        public Level0()
        {
            InitializeComponent();
            Global.LevelResolution = Global.Resolutions.p1080;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            if (Global.LevelResolution == Global.Resolutions.p4k && gameTickFreq > 60)
                gameTickFreq = 60;

            // set height and width of window
            this.Width = Global.LevelSize.Width;
            this.Height = Global.LevelSize.Height;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitEntities();
            InitUI();

            threadSettings.MaxDegreeOfParallelism = 4;
            threadSettings.CancellationToken = cancelTokenSrc.Token;

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
                int gcCounter = 0;
                while (true)
                {
                    if (threadCT.IsCancellationRequested) return;
                    if (!gameTickEnabled) continue;
                    if (threadDelaySW.Elapsed.TotalMilliseconds <= gameTickInterval) continue;

                    double deltaTime = slowTime(getDeltaTime());
                    double animationInterval = animTickSW.Elapsed.TotalMilliseconds;

                    if (slowTimeS > 0) animationInterval /= SLOW_FACTOR;
                    if (animationInterval >= ANIMATION_FPS)
                    {
                        TickAnimations();
                        animTickSW.Restart();
                        if (++gcCounter == 16) 
                        {
                            GC.Collect();
                            gcCounter = 0;
                        }
                    }

                    threadDelaySW.Restart();
                    if (isPaused) continue; 
                    movementTick(deltaTime);
                    attackHandler(deltaTime); 
                    renderGraphics(deltaTime);
                    TryInvoke(this.Refresh);
                }
            });
        }

        // fixes InvalidAsynchronousStateException upon form close
        private void TryInvoke(Action action)
        {
            if (IsDisposed) return;

            try { BeginInvoke(action); } 
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
        }

        private void TickAnimations()
        {
            foreach (KeyValuePair<Character, Bitmap?> c in characterAnimations)
            {
                if (c.Key.IsActive) characterAnimations[c.Key] = c.Key.NextAnimFrame(); 
                else characterAnimations[c.Key] = null;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            deltaTimeSW.Start();

            if (gameTickThread is null) throw new Exception("gameTickThread not initialsed");
            gameTickThread.Start();

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


        private double slowTime(double deltaTime)
        {
            if (slowTimeS > 0)
            {
                if (ZOOM_FACTOR <= 1)
                    throw new ArgumentException("ZOOM_FACTOR must be bigger than 1");

                slowedMov = true;
                slowTimeS -= (float)deltaTime;
                return (deltaTime /= SLOW_FACTOR);
            }
            else 
            {
                slowTimeS = 0;
                if (slowedMov) // the player in motion when in slow 
                {
                    movingLeft = false;
                    movingRight = false;
                    slowedMov = false;
                }
                return deltaTime;
            }
        }


        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static List<Projectile> disposedProjectiles = new List<Projectile>();

        private void attackHandler(double deltaTime)
        {
            playerIsHit = false;
            freezeTimeS = 0;

            foreach (Attacks a in Attacks.AttacksList)
            {
                foreach (Enemy e in Enemy.ActiveEnemyList)
                {
                    if (a.Parent == e) continue;
                    if (a.AtkHitbox.Hitbox.IntersectsWith(e.Hitbox)) 
                    {
                        e.DoDamage(a.AtkDmg);
                        a.Dispose();
                    }
                }
                if (a.Parent is Player) continue;
                if (a.AtkHitbox.Hitbox.IntersectsWith(playerBox.Hitbox)) 
                {
                    if (playerBox.CheckParrying(a)) 
                    {
                        a.Dispose();
                        continue;
                    }

                    playerBox.DoDamage(a.AtkDmg, ref hpBar);
                    a.Dispose();
                }
            }

            try { CheckProjectileCollisions(deltaTime); }
            catch (OperationCanceledException) { return; }

            Character.TickEndlags(deltaTime);
            Attacks.UpdateHitboxes();

            if (playerBox.Hp <= 0)
            {
                togglePause(true);
                MessageBox.Show(this, "you are dead", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }


        private void CheckProjectileCollisions(double dt)
        {
            if (Projectile.ProjectileList.Count == 0) return;
            Parallel.ForEach(Projectile.ProjectileList, threadSettings, bullet =>
            {
                bullet.MoveProjectile(dt);
                PointF bLoc = bullet.Center;

                if (disposedProjectiles.Contains(bullet)) return;

                foreach (Platform p in Platform.ActivePlatformList)
                {
                    if (!(p.Hitbox.IntersectsWith(bullet.Hitbox))) continue;
                    disposedProjectiles.Add(bullet);
                    break;
                }

                if (!bullet.Hitbox.IntersectsWith(ClientRectangle)) // WARNING: changed from location based to rectangle based
                {
                    disposedProjectiles.Add(bullet);
                    return;
                }

                // Return if bullet not touching player
                if (!playerBox.Hitbox.IntersectsWith(bullet.Hitbox))
                    return;

                if (!Player.IsParrying)
                {
                    freezeTimeS = FREEZE_DURATION_S * 10;
                    disposedProjectiles.Add(bullet);
                    playerBox.DoDamage(1, ref hpBar);
                    return;
                }

                bullet.rebound(playerBox);// required to prevent getting hit anyway when parrying

                if (playerBox.CheckParrying(bullet)) disposedProjectiles.Add(bullet);

                if (disposedProjectiles.Count == 0) return;
                foreach (Projectile p in disposedProjectiles)
                    Projectile.ProjectileList.Remove(p);

                disposedProjectiles.Clear();


            });
        }


        public static void SlowTime() => slowTimeS = SLOW_DURATION_S;


        private void movementTick(double deltaTime)
        {
            double scrollVelocity = 0;
            Global.XDirections? playerMovDir = null;
            bool isScrolling = playerBox.CheckScrolling(mainPlat);

            foreach (Entity e in Entity.EntityList)
                e.CheckActive();

            if (playerBox.IsOnFloor && jumping) { playerBox.DoJump(); }
            if (movingLeft) playerMovDir = Global.XDirections.left;
            if (movingRight) playerMovDir = Global.XDirections.right;
            if (isScrolling) scrollVelocity -= playerBox.XVelocity;

            playerBox.MoveCharacter(deltaTime, playerMovDir, scrollVelocity);

            foreach (Platform plat in Platform.ActivePlatformList)
                playerBox.CheckPlatformCollision(plat); 

            try
            {
                Parallel.ForEach(Enemy.ActiveEnemyList, threadSettings, enemy => {
                    enemy.DoMovement( deltaTime, scrollVelocity, playerBox.Center );
                    foreach (Platform plat in Platform.ActivePlatformList)
                    { enemy.CheckPlatformCollision(plat); }
                });
            }
            catch (OperationCanceledException) { return; }

            ScrollEntities(scrollVelocity, deltaTime);
        }


        public void ScrollEntities(double velocity, double deltaTime)
        {
            foreach( Entity e in Entity.EntityList)
            {
                if (e is Player) { continue; }
                e.UpdateX(velocity * deltaTime);
            }
        }



        private ushort prevFrameFPS = 0;
        private float prevFrameTime = 0;
        private static Pen playerPen = Pens.Blue;

        /// <summary> rendering graphics method </summary>
        /// <param name="dt"> deltaTime for fps calculations </param>
        private void renderGraphics(double dt)
        {
            if (slowTimeS <= 0 && freezeTimeS <= 0 && curZoom != 1)
            {
                unZoomScreen();
                curZoom = 1;
            }


            if (Math.Abs(prevFrameFPS - (ushort)(1/dt)) > 4) 
            {
                prevFrameFPS = (ushort)(1/dt);
                prevFrameTime = (float)dt; 
            }

            if (Player.IsParrying) playerPen = Pens.Gray;
            else if (playerIsHit) playerPen = Pens.Red; // visual hit indicator
            else playerPen = Pens.Blue;
        }

        // pause game
        private void togglePause(bool pause) => isPaused = !isPaused; 


        // TODO: remove this logic and use graphics scaling instead.
        // (Center Player on screen method)
        public static void ZoomScreen() 
        {
            curZoom = ZOOM_FACTOR;

            float playerX = playerBox.Center.X;
            float playerY = playerBox.Center.Y;

            float midX = Global.CenterOfScreen.X;
            float midY = Global.CenterOfScreen.Y;
            void zoomObj(Entity obj, float x, float y)
            {
                float _xDiff = obj.Center.X - playerX;
                float _yDiff = obj.Center.Y - playerY;

                float newX = x + _xDiff * curZoom;
                float newY = y + _yDiff * curZoom;

                obj.ScaleHitbox(curZoom);
                obj.Center = new PointF (newX, newY);
                //Invalidate(); // WARNING: unsure if needed
            }


            foreach (Entity e in Entity.EntityList)
            {
                zoomOrigins.Add(e,e.Center);
                if (e == playerBox)
                {
                    playerBox.Center = new PointF(
                            Global.CenterOfScreen.X,
                            Global.CenterOfScreen.Y);
                    playerBox.ScaleHitbox(curZoom);
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


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            if (Global.LevelResolution == Global.Resolutions.p4k) 
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.ScaleTransform(3/2F, 3/2F);
            }

            foreach (GameUI GUI in GameUI.UiElements) GUI.Draw(g);

            // TODO: try put animation in classes
            foreach (KeyValuePair<Character, Bitmap?> img in characterAnimations)
            {
                if (img.Value is null) continue;
                g.DrawImage(img.Value, img.Key.AnimRect);
            }
            
            //Bitmap platformSprite = Platform.PlatformSprite;
            foreach (Platform plat in Platform.ActivePlatformList)
            {
                RectangleF hitbox = plat.Hitbox;
                using (Pen blackPen = new Pen(Color.Black, 6))
                { g.DrawRectangle(blackPen, hitbox); }
                //if (plat.IsMainPlat) continue;
                //g.DrawImage(platformSprite, hitbox);
            }

            if (showHitbox) drawHitboxes(g);

            ShowFPSInfo(g);
        }


        private void ShowFPSInfo(Graphics g)
        {

            g.DrawString(
                    prevFrameFPS.ToString()+"fps",
                    new Font("Arial", 10*Global.BaseScale),
                    Brushes.Black,
                    new PointF(60*Global.BaseScale,220*Global.BaseScale));
            g.DrawString(
                    prevFrameTime.ToString("F3")+"ms",
                    new Font("Arial", 10*Global.BaseScale),
                    Brushes.Black,
                    new PointF(60*Global.BaseScale,240*Global.BaseScale));
        }

        private void drawHitboxes(Graphics g)
        {
            g.DrawRectangle(playerPen, playerBox.Hitbox);

            foreach (Enemy e in Enemy.ActiveEnemyList) 
                g.DrawRectangle(Pens.Blue, e.Hitbox);
            foreach (Projectile bullet in Projectile.ProjectileList) 
                g.FillRectangle(Brushes.Red, bullet.Hitbox);
            foreach (Attacks a in Attacks.AttacksList) 
                g.FillRectangle(Brushes.Red, a.AtkHitbox.Hitbox);
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers  == Keys.Alt && e.KeyCode == Keys.F5) Application.Exit();

            switch (e.KeyCode)
            {
                case Keys.W:
                    if (playerBox.IsOnFloor) jumping = true; 
                    break;

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

                case Keys.Escape:
                    this.Close();
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
                    playerBox.DoParry();
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
                    Player.StopParry();
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
