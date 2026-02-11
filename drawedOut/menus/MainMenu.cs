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

            _24FpsBtn,
            _30FpsBtn,
            _60FpsBtn,
            _120FpsBtn,

            _720pBtn,
            _1080pBtn,
            _1440pBtn,

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
            UpdateSettingsButtons();

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

        private void CreateMenuButtons()
        {
            // main menu
            _quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.75f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: ()=>TryInvoke(QuitGame),
                    fontScale: 1.2f,
                    txt: "Quit");
            _settingsBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.55f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: ShowSettingsMenu,
                    fontScale: 2f, 
                    txt: "Settings");
            _playBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.4f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: ()=>TryInvoke(OpenLevelMenu),
                    fontScale: 2f,
                    txt: "Play");
        }

        private void CreateSettingsBtns()
        {
            // settings menu
            _backgroundBtn = new GameButton(
                    xCenterPos: 0.6f,
                    yCenterPos: 0.18f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: ()=>{Global.ShowBG=false;},
                    txt: "On");
            CreateFPSBtns(0.58f, 0.26f);
            CreateResBtns(0.58f, 0.34f);
            _timeBtn = new GameButton(
                    xCenterPos: 0.6f,
                    yCenterPos: 0.12f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: TimeBtnClickEvent,
                    txt: "On");
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
                    clickEvent: ShowMainMenu,
                    txt: "Back to menu");
        }

        private void UpdateSettingsButtons()
        {
            // settings menu
            _backgroundBtn.BtnTxt = "on";
            // _timeBtn = new GameButton(
            //         xCenterPos: 0.6f,
            //         yCenterPos: 0.12f,
            //         relWidth: 0.06f,
            //         relHeight: 0.05f,
            //         clickEvent: TimeBtnClickEvent,
            //         txt: "On");

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
        }

        private void CreateFPSBtns(float xLevel, float yLevel)
        {
            float spacing = 0.05f;
            float btnWidth = 0.03f;
            float btnHeight = 0.03f;
            _24FpsBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(24),
                    txt: "24");
            _30FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(30),
                    txt: "30");
            _60FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(60),
                    txt: "60");
            _120FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing*3,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(120),
                    txt: "120");
        }

        private void CreateResBtns(float xLevel, float yLevel)
        {
            float spacing = 0.08f;
            float btnWidth = 0.05f;
            float btnHeight = 0.03f;
            _720pBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{},
                    txt: "24");
            _1080pBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{},
                    txt: "30");
            _1440pBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{},
                    txt: "60");
        }

        private void FpsBtnClick(UInt16 fps)
        {
            Global.GameTickFreq = fps;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            _active = true;
            _menuTimer.Start();
            ShowMainMenu();

            Invalidate();
        }

        private void ShowMainMenu()
        {
            _curState = MenuState.Start;

            GameButton.HideAll();
            _playBtn.Show();
            _settingsBtn.Show();
            _quitBtn.Show();
        }

        private void ShowSettingsMenu()
        {
            _curState = MenuState.Start;

            GameButton.HideAll();
            _backgroundBtn.Show();
            _24FpsBtn.Show();
            _30FpsBtn.Show();
            _60FpsBtn.Show();
            _120FpsBtn.Show();
            _settingsBackBtn.Show();

            TryInvoke(Invalidate);
        }

        private void OpenLevelMenu()
        {
            GameButton.ClearAll();
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

        private void MainMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4) Application.Exit();
        }

        private void MainMenu_Quit(object sender, FormClosingEventArgs e)
        {
            _active = false;
        }
    }
}


