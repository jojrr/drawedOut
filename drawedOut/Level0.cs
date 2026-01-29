// TODO: move all const values into global
namespace drawedOut
{
    public abstract partial class Level0 : Form
    {
        private HpBarUI hpBar;
        private BarUI energyBar;
        private Platform endWall, roomWall;

        private static Dictionary<Entity, PointF> zoomOrigins = new Dictionary<Entity, PointF>();
        private static Dictionary<Character, Bitmap?> characterAnimations = new Dictionary<Character, Bitmap?>();
        private static Keys? prevLeftRight;
        private static float gameTickInterval, _baseScale;

        private static float curZoom = 1, slowTimeS = 0;
        private readonly int _levelWidth;
        private readonly UInt16 _levelNo;
        private readonly Point _playerStartPos;
        private bool
            _levelActive = true,
            _showLevelTime = true,
            showHitbox = false,

            movingLeft = false,
            movingRight = false,
            jumping = false,

            isPaused = false,
            slowedMov = false,
            levelLoaded = false;

        protected static Player playerCharacter;
        protected Platform roomDoor;
        protected Platform basePlate;

        protected static Stopwatch levelTimerSW = new Stopwatch();
        // threading 
        private static CancellationTokenSource cancelTokenSrc = new CancellationTokenSource(); 
        private static ParallelOptions threadSettings = new ParallelOptions();
        private static Stopwatch deltaTimeSW = new Stopwatch();
        private static Thread gameTickThread;

        private void InitUI()
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
            energyBar.SetMax((float)(playerCharacter.MaxEnergy), true);
        }

        private void InitEntities()
        {
            if (levelLoaded) return;
            playerCharacter = new Player(
                    origin: _playerStartPos,
                    width: 30,
                    height: 160,
                    attackPower: 1,
                    energy: 100,
                    hp: 6);
            InitPlatforms();
            InitCheckpoints();
            InitEnemies();
        }
        protected virtual void InitEnemies() => throw new Exception("InitEnemies not defined");
        protected virtual void InitCheckpoints() => throw new Exception("InitCheckpoints not defined");
        protected virtual void InitPlatforms() 
        {
            basePlate = new(
               origin: new Point(0, 3000),
               width: _levelWidth,
               height: 1,
               toggleable: true,
               defaultState: true);

            float levelRight = basePlate.Width;
            endWall = new(
                    origin: new Point((int)levelRight-20, 0),
                    width: 100,
                    height: 750);

            int roomWallX = (int)levelRight-1920;
            roomWall = new(
                    origin: new Point(roomWallX, 0),
                    width: 40,
                    height: 550);
            roomDoor = new(
                    origin: new Point(roomWallX, 550),
                    width: 30,
                    height: 200,
                    toggleable: true,
                    defaultState: false);
        }


        public Level0(UInt16 levelNo, int levelWidth, Point playerStartPos)
        {
            if (levelWidth < 2000) throw new Exception("level too small");

            InitializeComponent();
            Global.LevelResolution = Global.Resolutions.p1080;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // set height and width of window
            this.Width = Global.LevelSize.Width;
            this.Height = Global.LevelSize.Height;
            this.StartPosition = FormStartPosition.CenterScreen;

            _levelNo = levelNo;
            _levelWidth = levelWidth;
            _playerStartPos = playerStartPos;

            // sets the refresh interval
            gameTickInterval = (1000.0F / Global.GameTickFreq);
            _baseScale = Global.BaseScale;

            // starts the clocks for screen refresh and animation update
            Stopwatch threadDelaySW = Stopwatch.StartNew();
            Stopwatch animTickSW = Stopwatch.StartNew();
            CancellationToken threadCT = cancelTokenSrc.Token;
            threadSettings.CancellationToken = threadCT;
            threadSettings.MaxDegreeOfParallelism = Global.MAX_THREADS_TO_USE;

            ResetLevel();
            LinkAnimations();

            gameTickThread = new Thread(() =>
            {
                while (_levelActive)
                {
                    if (threadCT.IsCancellationRequested) return;
                    if (threadDelaySW.Elapsed.TotalMilliseconds <= gameTickInterval) continue;

                    double deltaTime = slowTime(getDeltaTime());
                    double animationInterval = animTickSW.Elapsed.TotalMilliseconds;

                    if (slowTimeS > 0) animationInterval /= Global.SLOW_FACTOR;
                    if (animationInterval >= Global.ANIMATION_FPS)
                    {
                        TickAnimations();
                        animTickSW.Restart();
                    }

                    threadDelaySW.Restart();
                    if (isPaused) continue; 
                    movementTick(deltaTime);
                    attackHandler(deltaTime); 
                    renderGraphics(deltaTime);
                    TryInvoke(this.Refresh);
                }
                this.TryInvoke(Close);
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
            playerCharacter.Reset();
            InitUI();
            hpBar.UpdateMaxHp(playerCharacter.MaxHp);
            hpBar.ComputeHP(playerCharacter.Hp);
            Player.LinkHpBar(ref hpBar);
            energyBar.Update((float)(playerCharacter.Energy));
        }

        // <summary>
        // assign enemies their respective animations in the characterAnimations Dictionary.
        // </summary>
        private void LinkAnimations()
        {
            characterAnimations.Clear();
            characterAnimations.Add(playerCharacter, playerCharacter.NextAnimFrame());
            foreach (Enemy e in Enemy.InactiveEnemyList) characterAnimations.Add(e, e.NextAnimFrame());
            foreach (Enemy e in Enemy.ActiveEnemyList) characterAnimations.Add(e, e.NextAnimFrame());
        }


        private double getDeltaTime()
        {
            double deltaTime = deltaTimeSW.Elapsed.TotalSeconds;
            deltaTimeSW.Restart();

            isPaused = false;
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
            foreach (Attacks a in Attacks.AttacksList)
            {
                RectangleF atkBox = a.AtkHitbox.Hitbox;
                foreach (Enemy e in Enemy.ActiveEnemyList)
                {
                    if (a.Parent == e) continue;
                    if (!atkBox.IntersectsWith(e.Hitbox)) continue; 
                    e.DoDamage(a);
                    a.Dispose();
                }
                if (a.Parent is Player) 
                {
                    foreach (Checkpoint c in Checkpoint.CheckPointList)
                    { if (atkBox.IntersectsWith(c.Hitbox)) c.SaveState(playerCharacter, basePlate); }
                }
                else if (atkBox.IntersectsWith(playerCharacter.Hitbox)) 
                {
                    if (playerCharacter.CheckParrying(a)) 
                    {
                        a.Dispose();
                        continue;
                    }

                    playerCharacter.DoDamage(a);
                    a.Dispose();
                }
            }

            try { Projectile.CheckProjectileCollisions(deltaTime, this, playerCharacter, threadSettings); }
            catch (OperationCanceledException) { return; }

            playerCharacter.TickCounters(deltaTime);
            Enemy.TickCounters(deltaTime);
            Attacks.UpdateHitboxes();
            Entity.DisposeRemoved();
            energyBar.Update((float)(playerCharacter.Energy));

            if (playerCharacter.Hp <= 0) PlayerDeath();
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
            bool isScrolling = playerCharacter.CheckScrolling(basePlate);

            foreach (Entity e in Entity.EntityList)
                e.CheckActive();

            if (playerCharacter.IsOnFloor && jumping) { playerCharacter.DoJump(); }
            if (isScrolling) scrollVelocity -= playerCharacter.XVelocity;

            if (movingLeft) playerMovDir = Global.XDirections.left;
            else if (movingRight) playerMovDir = Global.XDirections.right;

            playerCharacter.MoveCharacter(deltaTime, playerMovDir, scrollVelocity);

            if (playerCharacter.Hitbox.Left > roomDoor.Hitbox.Right) 
            {
                roomDoor.Activate(); 
                scrollTillEnd(deltaTime);
            }
            else if (playerCharacter.Hitbox.Right < roomDoor.Hitbox.Left) roomDoor.Deactivate();

            try { Parallel.ForEach(Enemy.ActiveEnemyList, threadSettings, enemy => 
                    { enemy.DoMovement( deltaTime, scrollVelocity, playerCharacter.Center ); }
                );}
            catch (OperationCanceledException) { return; }

            Item.DoAllGravTick(deltaTime);
            Item.CheckPlayerCollisions(playerCharacter.Hitbox);

            ScrollEntities(scrollVelocity, deltaTime);
        }


        public void ScrollEntities(double velocity, double deltaTime, bool includePlayer=false)
        {
            foreach(Entity e in Entity.EntityList)
            {
                if (e is Player && !includePlayer) { continue; }
                e.UpdateX(velocity * deltaTime * _baseScale);
            }
        }


        public void scrollTillEnd(double dt)
        {
            if (basePlate.Hitbox.Right > Global.LevelSize.Width && curZoom == 1) 
                ScrollEntities(-1200, dt, true);
        }


        private ushort prevFrameFPS = 0;
        private float prevFrameTime = 0;
        private static Pen playerPen = Pens.Blue;

        /// <summary> rendering graphics method </summary>
        /// <param name="dt"> deltaTime for fps calculations </param>
        private void renderGraphics(double dt)
        {
            if (Math.Abs(prevFrameFPS - (ushort)(1/dt)) > 4) 
            {
                prevFrameFPS = (ushort)(1/dt);
                prevFrameTime = (float)dt; 
            }

            if (playerCharacter.IsParrying) playerPen = Pens.Gray;
            else if (playerCharacter.IsHit) playerPen = Pens.Red; // visual hit indicator
            else playerPen = Pens.Blue;
        }

        // pause game
        private void togglePause() => isPaused = !isPaused; 

        // pause game
        private void togglePause(bool pause) => isPaused = pause; 


        // TODO: remove this logic and use graphics scaling instead.
        // (Center Player on screen method)
        public static void ZoomScreen() 
        {
            curZoom = Global.ZOOM_FACTOR;

            float playerX = playerCharacter.Center.X;
            float playerY = playerCharacter.Center.Y;

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
                if (e == playerCharacter)
                {
                    playerCharacter.Center = new PointF(
                            Global.CenterOfScreen.X,
                            Global.CenterOfScreen.Y);
                    playerCharacter.ScaleHitbox(curZoom);
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

            playerCharacter.ResetScale();
            zoomOrigins.Clear();
            curZoom = 1; // screen is no longer scaled
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            Checkpoint.DrawAll(g);
            Item.DrawAll(g);
            DrawCharacters(g);
            Projectile.DrawAll(g);
            Platform.DrawAll(g);

            if (_showLevelTime) ShowSpeedrunTime(g);
            if (showHitbox) drawHitboxes(g);
            foreach (GameUI GUI in GameUI.UiElements) GUI.Draw(g);
            ShowFPSInfo(g);
        }

        private void DrawCharacters(Graphics g)
        {
            // TODO: try put animation in classes
            foreach (KeyValuePair<Character, Bitmap?> img in characterAnimations)
            {
                if (img.Value is null) continue;
                g.DrawImage(img.Value, img.Key.AnimRect);
            }
        }


        private void ShowSpeedrunTime(Graphics g)
        {
            float baseScale = Global.BaseScale;
            Font defaultFont = new Font("Sour Gummy Black", 18*baseScale);

            g.DrawString(
                    levelTimerSW.Elapsed.TotalSeconds.ToString("F3"),
                    defaultFont,
                    Brushes.Black,
                    new PointF(1800*baseScale,30*baseScale));
        }


        private void ShowFPSInfo(Graphics g)
        {
            float baseScale = Global.BaseScale;
            Font debugFont = new Font("Arial", 10*baseScale);
            g.DrawString(
                    prevFrameFPS.ToString()+"fps",
                    debugFont,
                    Brushes.Black,
                    new PointF(60*baseScale,220*baseScale));
            g.DrawString(
                    prevFrameTime.ToString("F3")+"ms",
                    debugFont,
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

            g.DrawRectangle(playerPen, playerCharacter.Hitbox);
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers  == Keys.Alt && e.KeyCode == Keys.F5) Application.Exit();

            switch (e.KeyCode)
            {
                case Keys.W:
                    if (playerCharacter.IsOnFloor) jumping = true; 
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

                case Keys.E:
                    playerCharacter.DoSpecial1();
                    break;

                case Keys.Escape:
                    this.Close();
                    break;
            }

            if (!levelTimerSW.IsRunning && basePlate.LocationX > -1800) levelTimerSW.Start();
        }


        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (slowTimeS > 0)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                    jumping = false;
                    playerCharacter.StopJump();
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
                    playerCharacter.DoParry();
                    break;
                case MouseButtons.Left:
                    playerCharacter.DoBasicAttack();
                    break;
            }
        }


        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // stops parrying when mouseup but doesnt reset timer > only on mouse down 
                case MouseButtons.Right:
                    playerCharacter.StopParry();
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSrc.Cancel();
        }

        protected void LevelEnd()
        {
            Enemy.ClearAllLists();
            Entity.ClearAllLists();
            Attacks.ClearAllLists();
            Projectile.ClearAllLists();
            Checkpoint.ClearAllLists();
            characterAnimations.Clear();
            SaveData.AddScore(_levelNo, (float)levelTimerSW.Elapsed.TotalSeconds);
            _levelActive = false;
        }
    }
}
