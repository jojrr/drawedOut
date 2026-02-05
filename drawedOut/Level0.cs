using System.Windows;
namespace drawedOut
{
    public abstract partial class Level0 : Form
    {

        protected static Stopwatch levelTimerSW = new Stopwatch();
        protected static Player playerCharacter;
        protected Platform roomDoor;
        protected Platform basePlate;

        private readonly int _levelWidth;
        private readonly UInt16 _levelNo;
        private readonly Point _playerStartPos;

        private static Dictionary<Entity, PointF> _zoomOrigins = new Dictionary<Entity, PointF>();
        private static Dictionary<Character, Bitmap?> _characterAnimations = new Dictionary<Character, Bitmap?>();

        // threading 
        private static Stopwatch _deltaTimeSW = new Stopwatch();
        private static Keys? _prevLeftRight;
        private static float 
            _gameTickInterval, 
            _baseScale,
            _curZoom = 1, 
            _slowTimeS = 0;

        private CancellationTokenSource _cancelTokenSrc = new CancellationTokenSource(); 
        private ParallelOptions _threadSettings = new ParallelOptions();
        private Thread _gameTickThread;

        private HpBarUI _hpBar;
        private Point _mouseLoc;
        private BarUI _energyBar;
        private Platform _endWall, _roomWall;
        private bool
            _levelActive = true,
            _showLevelTime = true,
            _showHitbox = false,

            _movingLeft = false,
            _movingRight = false,
            _jumping = false,

            _slowedMov = false,
            _levelLoaded = false,
            _isPaused = false;

        private void InitUI()
        {
            _hpBar = new HpBarUI(
                    origin: new PointF(90, 50),
                    barWidth: 30,
                    barHeight: 60,
                    maxHp: 6);

            _energyBar = new BarUI(
                    origin: new PointF(90, 140),
                    elementWidth: 300,
                    elementHeight: 30,
                    brush: Brushes.Blue,
                    bgBrush: Brushes.Gray,
                    maxVal: 1,
                    borderScale: 0.4f);
            _energyBar.SetMax((float)(playerCharacter.MaxEnergy), true);
        }

        private void InitEntities()
        {
            if (_levelLoaded) return;
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
            _endWall = new(
                    origin: new Point((int)levelRight-20, 0),
                    width: 100,
                    height: 750);

            int roomWallX = (int)levelRight-1920;
            _roomWall = new(
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
            _gameTickInterval = (1000.0F / Global.GameTickFreq);
            _baseScale = Global.BaseScale;

            // starts the clocks for screen refresh and animation update
            Stopwatch threadDelaySW = Stopwatch.StartNew();
            Stopwatch animTickSW = Stopwatch.StartNew();
            CancellationToken threadCT = _cancelTokenSrc.Token;
            _threadSettings.CancellationToken = threadCT;
            _threadSettings.MaxDegreeOfParallelism = Global.MAX_THREADS_TO_USE;

            ResetLevel();
            LinkAnimations();

            _gameTickThread = new Thread(() =>
            {
                while (_levelActive)
                {
                    if (threadCT.IsCancellationRequested) return;
                    if (threadDelaySW.Elapsed.TotalMilliseconds <= _gameTickInterval) continue;

                    threadDelaySW.Restart();
                    if (_isPaused) 
                    {
                        TryInvoke(FindMouse);
                        bool needUpdate = GameButton.CheckAllMouseHover(_mouseLoc);
                        if (needUpdate) TryInvoke(this.Refresh);
                        continue; 
                    }

                    double deltaTime = slowTime(getDeltaTime());
                    double animationInterval = animTickSW.Elapsed.TotalMilliseconds;

                    if (_slowTimeS > 0) animationInterval /= Global.SLOW_FACTOR;
                    if (animationInterval >= Global.ANIMATION_FPS)
                    {
                        TickAnimations();
                        animTickSW.Restart();
                    }

                    movementTick(deltaTime);
                    attackHandler(deltaTime); 
                    calcFrameInfo(deltaTime);
                    TryInvoke(this.Refresh);
                }
                this.TryInvoke(Close);
            });
        }

        private void FindMouse() => _mouseLoc = PointToClient(Cursor.Position);

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
            foreach (KeyValuePair<Character, Bitmap?> c in _characterAnimations)
            {
                if (c.Key.IsActive) _characterAnimations[c.Key] = c.Key.NextAnimFrame(); 
                else _characterAnimations[c.Key] = null;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            _isPaused = false;
            _deltaTimeSW.Start();

            if (_gameTickThread is null) throw new Exception("_gameTickThread not initialsed");
            _gameTickThread.Start();

            _levelLoaded = true;
            GC.Collect();
        }

        private void ResetLevel()
        {
            foreach (Attacks a in Attacks.AttacksList) a.Dispose();
            InitEntities();
            playerCharacter.Reset();
            InitUI();
            _hpBar.UpdateMaxHp(playerCharacter.MaxHp);
            _hpBar.ComputeHP(playerCharacter.Hp);
            Player.LinkHpBar(ref _hpBar);
            _energyBar.Update((float)(playerCharacter.Energy));
        }

        // <summary>
        // assign enemies their respective animations in the _characterAnimations Dictionary.
        // </summary>
        private void LinkAnimations()
        {
            _characterAnimations.Clear();
            _characterAnimations.Add(playerCharacter, playerCharacter.NextAnimFrame());
            foreach (Enemy e in Enemy.InactiveEnemyList) _characterAnimations.Add(e, e.NextAnimFrame());
            foreach (Enemy e in Enemy.ActiveEnemyList) _characterAnimations.Add(e, e.NextAnimFrame());
        }


        private double getDeltaTime()
        {
            double deltaTime = _deltaTimeSW.Elapsed.TotalSeconds;
            _deltaTimeSW.Restart();
            return double.Clamp(deltaTime, 0, 0.1);
        }


        private double slowTime(double deltaTime)
        {
            if (_slowTimeS > 0)
            {
                if (Global.ZOOM_FACTOR <= 1)
                    throw new ArgumentException("ZOOM_FACTOR must be bigger than 1");

                _slowedMov = true;
                _slowTimeS -= (float)deltaTime;
                return (deltaTime /= Global.SLOW_FACTOR);
            }
            else 
            {
                _slowTimeS = 0;
                if (_slowedMov) _slowedMov = false;
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

            try { Projectile.CheckProjectileCollisions(deltaTime, this, playerCharacter, _threadSettings); }
            catch (OperationCanceledException) { return; }

            playerCharacter.TickCounters(deltaTime);
            Enemy.TickCounters(deltaTime);
            Attacks.UpdateHitboxes();
            Entity.DisposeRemoved();
            _energyBar.Update((float)(playerCharacter.Energy));

            if (playerCharacter.Hp <= 0) PlayerDeath();
        }

        private void PlayerDeath()
        {
            // TryInvoke(new Action( ()=> MessageBox.Show(this, "you are dead", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)));
            ResetLevel();
            Checkpoint.LoadState();
        }


        public static void SlowTime() => _slowTimeS = Global.SLOW_DURATION_S;


        private void movementTick(double deltaTime)
        {
            if (_slowTimeS <= 0 && _curZoom != 1)
            {
                unZoomScreen();
                _curZoom = 1;
            }

            double scrollVelocity = 0;
            Global.XDirections? playerMovDir = null;
            bool isScrolling = playerCharacter.CheckScrolling(basePlate);

            foreach (Entity e in Entity.EntityList)
                e.CheckActive();

            if (playerCharacter.IsOnFloor && _jumping) { playerCharacter.DoJump(); }
            if (isScrolling) scrollVelocity -= playerCharacter.XVelocity;

            if (_movingLeft) playerMovDir = Global.XDirections.left;
            else if (_movingRight) playerMovDir = Global.XDirections.right;

            playerCharacter.MoveCharacter(deltaTime, playerMovDir, scrollVelocity);

            if (playerCharacter.Hitbox.Left > roomDoor.Hitbox.Right) 
            {
                roomDoor.Activate(); 
                scrollTillEnd(deltaTime);
            }
            else if (playerCharacter.Hitbox.Right < roomDoor.Hitbox.Left) roomDoor.Deactivate();

            try { Parallel.ForEach(Enemy.ActiveEnemyList, _threadSettings, enemy => 
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
            if (basePlate.Hitbox.Right > Global.LevelSize.Width && _curZoom == 1) 
                ScrollEntities(-1200, dt, true);
        }


        private ushort prevFrameFPS = 0;
        private float prevFrameTime = 0;
        private static Pen playerPen = Pens.Blue;

        /// <summary> rendering graphics method </summary>
        /// <param name="dt"> deltaTime for fps calculations </param>
        private void calcFrameInfo(double dt)
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


        public static void ZoomScreen() 
        {
            _curZoom = Global.ZOOM_FACTOR;

            float playerX = playerCharacter.Center.X;
            float playerY = playerCharacter.Center.Y;

            float midX = Global.CenterOfScreen.X;
            float midY = Global.CenterOfScreen.Y;
            void zoomObj(Entity obj, float x, float y)
            {
                float _xDiff = obj.Center.X - playerX;
                float _yDiff = obj.Center.Y - playerY;

                float newX = x + _xDiff * _curZoom;
                float newY = y + _yDiff * _curZoom;

                obj.ScaleHitbox(_curZoom);
                obj.Center = new PointF (newX, newY);
            }


            foreach (Entity e in Entity.EntityList)
            {
                _zoomOrigins.Add(e,e.Center);
                if (e == playerCharacter)
                {
                    playerCharacter.Center = new PointF(
                            Global.CenterOfScreen.X,
                            Global.CenterOfScreen.Y);
                    playerCharacter.ScaleHitbox(_curZoom);
                    continue;
                }
                zoomObj(e, midX, midY); 
            }
        }



        private void unZoomScreen()
        {
            foreach (KeyValuePair<Entity, PointF> EntityPoints in _zoomOrigins)
            { 
                Entity obj = EntityPoints.Key;
                PointF point = EntityPoints.Value;
                obj.ResetScale();
                obj.Center = point;
            }

            playerCharacter.ResetScale();
            _zoomOrigins.Clear();
            _curZoom = 1; // screen is no longer scaled
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
            if (_showHitbox) DrawHitboxes(g);
            foreach (GameUI GUI in GameUI.UiElements) GUI.Draw(g);
            ShowFPSInfo(g);
            if (_isPaused) DrawPauseMenu(g);
        }

        private void DrawCharacters(Graphics g)
        {
            // TODO: try put animation in classes
            foreach (KeyValuePair<Character, Bitmap?> img in _characterAnimations)
            {
                if (img.Value is null) continue;
                g.DrawImage(img.Value, img.Key.AnimRect);
            }
        }


        private void ShowSpeedrunTime(Graphics g)
        {
            float baseScale = Global.BaseScale;
            g.DrawString(
                    levelTimerSW.Elapsed.TotalSeconds.ToString("F3"),
                    Global.DefaultFont,
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

        private void DrawHitboxes(Graphics g)
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

        private void DrawPauseMenu(Graphics g)
        {
            float recWidth = (float)ClientSize.Width*0.15f;
            float recHeight =(float)ClientSize.Height*0.4f;
            float recStartX = ClientSize.Width/2 - recWidth/2;
            float recStartY = ClientSize.Height/2 - recHeight/2;

            GameButton resumeBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.45f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: (()=>{_isPaused=false;}),
                    fontScale: 1f,
                    txt: "Resume");


            GameButton quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.55f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: this.Close,
                    fontScale: 1f,
                    txt: "Quit");

            RectangleF baseRec = new RectangleF(recStartX, recStartY, recWidth, recHeight);

            using (Brush bgBrush = new SolidBrush(Color.FromArgb(150, 100, 100, 100)))
            { g.FillRectangle(bgBrush, ClientRectangle); }
            using (Brush menuBgBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            { g.FillRectangle(menuBgBrush, baseRec); }
            using (Pen recPen = new Pen(Color.FromArgb(200, 50, 50, 50), 6))
            { g.DrawRectangle(recPen, baseRec); }

            using (Font pauseFont = new Font("Sour Gummy", 30*Global.BaseScale))
            {
                string pauseString = "PAUSED";
                SizeF pauseSize = g.MeasureString(pauseString, pauseFont);
                float pausePosX = ClientSize.Width/2 - (pauseSize.Width/2) + 5*Global.BaseScale;
                g.DrawString(pauseString, pauseFont, Brushes.Black, pausePosX, baseRec.Y + 5); 
            }

            GameButton.DrawAll(g);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers  == Keys.Alt && e.KeyCode == Keys.F5) Application.Exit();

            switch (e.KeyCode)
            {
                case Keys.W:
                    if (playerCharacter.IsOnFloor) _jumping = true; 
                    break;

                case Keys.A:
                    if (_movingRight && !_movingLeft && _prevLeftRight is null)
                    {
                        _prevLeftRight = Keys.D;
                        _movingRight = false;
                    }
                    _movingLeft = true;
                    break;

                case Keys.D:
                    if (_movingLeft && !_movingRight && _prevLeftRight is null)
                    {
                        _prevLeftRight = Keys.A;
                        _movingLeft = false;
                    }
                    _movingRight = true;
                    break;

                case Keys.E:
                    playerCharacter.DoSpecial1();
                    break;

                case Keys.Escape:
                    TogglePause();
                    break;
                case Keys.F4:
                    this.Close();
                    break;
            }

            if (!levelTimerSW.IsRunning && basePlate.LocationX > -1800) levelTimerSW.Start();
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            Invalidate();
        }


        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    _jumping = false;
                    playerCharacter.StopJump();
                    break;

                case Keys.A:
                    if (_prevLeftRight == Keys.D) _movingRight = true;
                    _prevLeftRight = null;
                    _movingLeft = false;
                    break;

                case Keys.D:
                    if (_prevLeftRight == Keys.A) _movingLeft = true;
                    _prevLeftRight = null;
                    _movingRight = false;
                    break;
            }
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isPaused) 
            {
                GameButton.ClickSelected();
                return;
            }
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
            Enemy.ClearAllLists();
            Entity.ClearAllLists();
            Attacks.ClearAllLists();
            Platform.ClearAllLists();
            Projectile.ClearAllLists();
            Checkpoint.ClearAllLists();
            GameButton.ClearAll();
            _characterAnimations.Clear();
            _deltaTimeSW.Reset();

            _levelActive = false;
            _cancelTokenSrc.Cancel();

            MainMenu menu = new MainMenu();
            menu.Show();
        }

        public void BossDeath()
        { 
            SaveData.AddScore(0, (float)levelTimerSW.Elapsed.TotalSeconds); 
            this.Close();
        }
    }
}
