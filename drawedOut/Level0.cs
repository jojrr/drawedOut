using System.Diagnostics;
using InterpolationMode = System.Drawing.Drawing2D.InterpolationMode;

// TODO: move all const values into global
namespace drawedOut
{
    public partial class Level0 : Form
    {
        private static Player playerBox;
        private static MeleeEnemy meleeOne;
        private static FlyingEnemy flyingOne;
        private static Checkpoint checkpointOne;

        private static Platform mainPlat;
        private static Platform box3;
        private static Platform box4;
        private static Platform box5;

        private static HpBarUI hpBar;
        private static BarUI energyBar;

        private static Dictionary<Entity, PointF> zoomOrigins = new Dictionary<Entity, PointF>();
        private static Dictionary<Character, Bitmap?> characterAnimations = new Dictionary<Character, Bitmap?>();

        private static Keys? prevLeftRight;
        private static int gameTickInterval;

        private static bool
            gameTickEnabled = true,
            showHitbox = true,

            movingLeft = false,
            movingRight = false,
            jumping = false,

            isPaused = false,
            slowedMov = false,

            levelLoaded = false;


        private static float
            curZoom = 1,
            // TODO: redo freeze and slow logic
            slowTimeS = 0,
            freezeTimeS = 0;

        // threading 
        private static CancellationTokenSource cancelTokenSrc = new CancellationTokenSource(); 
        private static ParallelOptions threadSettings = new ParallelOptions();
        private static Stopwatch deltaTimeSW = new Stopwatch();
        private static Thread gameTickThread;

        private static void InitUI()
        {
            hpBar = new HpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    maxHp: 6);

            energyBar = new BarUI(
                    origin: new PointF(70, 120),
                    elementWidth: 250,
                    elementHeight: 20,
                    brush: Brushes.Blue,
                    bgBrush: Brushes.Gray,
                    maxVal: 1,
                    borderScale: 0.4f);
            energyBar.SetMax((float)(playerBox.MaxEnergy), true);
        }

        private static void InitEntities()
        {
            if (levelLoaded) 
            {
                foreach (Entity e in Entity.EntityList) e.Reset();
                return;
            }

            playerBox = new Player(
                    origin: new Point(850, 550),
                    width: 30,
                    height: 160,
                    attackPower: 1,
                    energy: 100,
                    hp: 6);

            meleeOne = new(origin:new Point(2850, -550));
            flyingOne = new(origin:new Point(850, 100));

            checkpointOne = new(origin: new Point(200, 600));


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
            Global.LevelResolution = Global.Resolutions.p1440;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // set height and width of window
            this.Width = Global.LevelSize.Width;
            this.Height = Global.LevelSize.Height;
            this.StartPosition = FormStartPosition.CenterScreen;

            // sets the refresh interval
            gameTickInterval = (int)(1000.0F / Global.GameTickFreq);

            Stopwatch threadDelaySW = Stopwatch.StartNew();
            Stopwatch animTickSW = Stopwatch.StartNew();
            CancellationToken threadCT = cancelTokenSrc.Token;
            threadSettings.CancellationToken = threadCT;
            threadSettings.MaxDegreeOfParallelism = Global.MAX_THREADS_TO_USE;

            ResetLevel();

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

                    if (slowTimeS > 0) animationInterval /= Global.SLOW_FACTOR;
                    if (animationInterval >= Global.ANIMATION_FPS)
                    {
                        TickAnimations();
                        animTickSW.Restart();
                        if (++gcCounter == 32) 
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
            levelLoaded = true;
            GC.Collect();
        }

        private void ResetLevel()
        {
            foreach (Attacks a in Attacks.AttacksList) a.Dispose();
            InitEntities();
            RelinkAnimations();
            InitUI();
            hpBar.UpdateMaxHp(playerBox.MaxHp);
            hpBar.ComputeHP(playerBox.Hp);
            Player.LinkHpBar(ref hpBar);
            energyBar.Update((float)(playerBox.Energy));
        }

        private void RelinkAnimations()
        {
            characterAnimations.Clear();
            characterAnimations.Add(playerBox, playerBox.NextAnimFrame());
            foreach (Enemy e in Enemy.InactiveEnemyList) characterAnimations.Add(e, e.NextAnimFrame());
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
                deltaTime = 0;
            }
            return double.Clamp(deltaTime, 0, 0.1);
        }


        private double slowTime(double deltaTime)
        {
            if (slowTimeS > 0)
            {
                if (Global.ZOOM_FACTOR <= 1)
                    throw new ArgumentException("ZOOM_FACTOR must be bigger than 1");

                slowedMov = true;
                slowTimeS -= (float)deltaTime;
                return (deltaTime /= Global.SLOW_FACTOR);
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


        private void attackHandler(double deltaTime)
        {
            freezeTimeS = 0;

            foreach (Attacks a in Attacks.AttacksList)
            {
                RectangleF atkBox = a.AtkHitbox.Hitbox;
                foreach (Enemy e in Enemy.ActiveEnemyList)
                {
                    if (a.Parent == e) continue;
                    if (!atkBox.IntersectsWith(e.Hitbox)) continue; 
                    e.DoDamage(a.AtkDmg, a.Parent, false);
                    a.Dispose();
                }
                if (a.Parent is Player) 
                {
                    foreach (Checkpoint c in Checkpoint.CheckPointList)
                    { if (atkBox.IntersectsWith(c.Hitbox)) c.SaveState(); }
                }
                else if (atkBox.IntersectsWith(playerBox.Hitbox)) 
                {
                    if (playerBox.CheckParrying(a)) 
                    {
                        a.Dispose();
                        continue;
                    }

                    playerBox.DoDamage(a.AtkDmg, a.Parent);
                    a.Dispose();
                }
            }

            try { Projectile.CheckProjectileCollisions(deltaTime, this, playerBox, threadSettings); }
            catch (OperationCanceledException) { return; }

            Character.TickEndlags(deltaTime);
            Attacks.UpdateHitboxes();
            Entity.DisposeRemoved();
            energyBar.Update((float)(playerBox.Energy));

            if (playerBox.Hp <= 0) PlayerDeath();
        }

        private void PlayerDeath()
        {
            togglePause(true);
            // TryInvoke(new Action( ()=> MessageBox.Show(this, "you are dead", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)));
            ResetLevel();
            Checkpoint.LoadState();
            togglePause(false);
        }


        public static void SlowTime() => slowTimeS = Global.SLOW_DURATION_S;


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

            try { Parallel.ForEach(Enemy.ActiveEnemyList, threadSettings, enemy => 
                    { enemy.DoMovement( deltaTime, scrollVelocity, playerBox.Center ); }
                );}
            catch (OperationCanceledException) { return; }

            ScrollEntities(scrollVelocity, deltaTime);
        }


        public void ScrollEntities(double velocity, double deltaTime)
        {
            foreach(Entity e in Entity.EntityList)
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
            else if (Player.IsHit) playerPen = Pens.Red; // visual hit indicator
            else playerPen = Pens.Blue;
        }

        // pause game
        private static void togglePause() => isPaused = !isPaused; 

        // pause game
        private static void togglePause(bool pause) => isPaused = pause; 


        // TODO: remove this logic and use graphics scaling instead.
        // (Center Player on screen method)
        public static void ZoomScreen() 
        {
            curZoom = Global.ZOOM_FACTOR;

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
            
            foreach (Platform plat in Platform.ActivePlatformList)
            {
                RectangleF hitbox = plat.Hitbox;
                using (Pen blackPen = new Pen(Color.Black, 6))
                { g.DrawRectangle(blackPen, hitbox); }
            }
            
            if (showHitbox) drawHitboxes(g);
            Checkpoint.Draw(g);
            ShowFPSInfo(g);
        }


        private void ShowFPSInfo(Graphics g)
        {
            float baseScale = Global.BaseScale;

            g.DrawString(
                    prevFrameFPS.ToString()+"fps",
                    new Font("Arial", 10*baseScale),
                    Brushes.Black,
                    new PointF(60*baseScale,220*baseScale));
            g.DrawString(
                    prevFrameTime.ToString("F3")+"ms",
                    new Font("Arial", 10*baseScale),
                    Brushes.Black,
                    new PointF(60*baseScale,240*baseScale));
        }

        private void drawHitboxes(Graphics g)
        {
            foreach (Entity e in Entity.EntityList) 
                g.DrawRectangle(Pens.Aqua, e.Hitbox);
            foreach (Enemy e in Enemy.ActiveEnemyList) 
                g.DrawRectangle(Pens.Blue, e.Hitbox);
            foreach (Projectile bullet in Projectile.ProjectileList) 
                g.FillRectangle(Brushes.Red, bullet.Hitbox);
            foreach (Attacks a in Attacks.AttacksList) 
                g.DrawRectangle(Pens.Red, a.AtkHitbox.Hitbox);

            g.DrawRectangle(playerPen, playerBox.Hitbox);
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

                case Keys.S: throw new Exception("");

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
