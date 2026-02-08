namespace drawedOut
{
    public partial class MainMenu : Form
    {
        private enum MenuState { Start, Levels, Settings };

        // Main menu components
        private const int _TICK_MS = 30;
        private static Thread _menuTimer;
        private static bool _active;
        private static Point _mouseLoc;

        private static GameButton 
            _playBtn,
            _quitBtn,
            _settingsBtn;

        // Settings menu components
        private static string 
            _backgroundTxt,
            _fpsTxt,
            _timeTxt,

            _keysHeading,
            _jumpTxt,
            _leftTxt,
            _rightTxt,
            _abilityOneTxt,
            _abilityTwoTxt,
            _abilityThreeTxt;

        private static GameButton 
            _backgroundBtn,
            _fpsBtn,
            _timeBtn,

            _jumpRebindBtn,
            _leftRebindBtn,
            _rightRebindBtn,
            _abilityOneRebindBtn,
            _abilityTwoRebindBtn,
            _abilityThreeRebindBtn,
            
            _settingsBackBtn;
        
        private MenuState _curState;

        public MainMenu()
        {
            InitializeComponent();
            this.Size = Global.LevelSize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            CreateButtons();

            Stopwatch timerSW = Stopwatch.StartNew();
            _menuTimer = new Thread (() => 
            {
                while (_active)
                {
                    if (timerSW.Elapsed.TotalMilliseconds < _TICK_MS) continue;

                    TryInvoke(FindCursor);
                    bool needUpdate = GameButton.CheckAllMouseHover(_mouseLoc);
                    if (needUpdate) TryInvoke(this.Refresh);
                    timerSW.Restart();
                }
            });
        }

        private void FindCursor() => _mouseLoc = PointToClient(Cursor.Position);

        private void CreateButtons()
        {
            // main menu
            _quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.75f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: QuitButtonClickEvent,
                    fontScale: 1.2f,
                    txt: "Quit");
            _settingsBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.55f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: (()=>{_curState=MenuState.Settings;}),
                    fontScale: 2f, 
                    txt: "Settings");
            _playBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.4f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: PlayBtnClickEvent,
                    fontScale: 2f,
                    txt: "Play");
            
            // settings menu
            // _backgroundBtn = new GameButton(
            //         xCenterPos: 0.6f,
            //         yCenterPos: 0.08f,
            //         relWidth: 0.06f,
            //         relHeight: 0.05f,
            //         clickEvent: BackBtnClickEvent,
            //         txt: "On");
            // _fpsBtn = new GameButton(
            //         xCenterPos: 0.6f,
            //         yCenterPos: 0.1f,
            //         relWidth: 0.06f,
            //         relHeight: 0.05f,
            //         clickEvent: FpsBtnClickEvent,
            //         txt: "On");
            // _timeBtn = new GameButton(
            //         xCenterPos: 0.6f,
            //         yCenterPos: 0.12f,
            //         relWidth: 0.06f,
            //         relHeight: 0.05f,
            //         clickEvent: TimeBtnClickEvent,
            //         txt: "On");
            //
            // _leftRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.54f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: LeftRebindClick,
            //         txt: "Click to rebind");
            // _rightRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.56f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: RightRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.58f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: JumpRebindClick,
            //         txt: "Click to rebind");
            // _abilityOneRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.54f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityOneRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.56f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityTwoRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.58f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityThreeRebindClick,
            //         txt: "Click to rebind");
            //
            _settingsBackBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.9f,
                    relWidth: 0.1f,
                    relHeight: 0.05f,
                    clickEvent: (()=>{_curState=MenuState.Start;}),
                    txt: "Click to rebind");
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            _curState = MenuState.Start;
            _active = true;
            _menuTimer.Start();
            Invalidate();
        }

        private void PlayBtnClickEvent() => TryInvoke(OpenLevelMenu); 
        private void QuitButtonClickEvent() => TryInvoke(QuitGame);

        private void OpenLevelMenu()
        {
            GameButton.ClearAll();
            _active=false;
            this.Close();
            TutorialLevel level = new TutorialLevel();
            level.Show();
        }

        private void QuitGame()
        {
            FormHandler.CloseHandler();
            this.Close();
        }

        private void TryInvoke(Action action)
        {
            if (IsDisposed) return;

            try { BeginInvoke(action); } 
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
        }

        private void MainMenu_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            string titleString = "DRAWED OUT";
            using (Font titleFont = new Font("Sour Gummy", 100*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(titleString, titleFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(titleString, titleFont, Brushes.Black, titlePosX, 20); 
            }

            GameButton.DrawAll(g);
        }

        private void MainMenu_MouseDown(object sender, MouseEventArgs e)
        {
            GameButton.ClickSelected();
        }

        private void MainMenu_Quit(object sender, FormClosingEventArgs e)
        {
            _active = false;
        }
    }
}


