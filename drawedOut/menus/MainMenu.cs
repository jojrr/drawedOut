namespace drawedOut
{
    internal partial class MainMenu : Form
    {
        private enum MenuState { Start, Levels, Settings };

        private static bool _active;
        private static Point _mouseLoc;
        private const int _TICK_MS = 30;
        private static Thread _menuTimer;
        private static Keybinds.Actions? _rebindAction=null;

        private MenuState _curMenuState;

        public MainMenu()
        {
            InitializeComponent();
            UpdateSize();
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.AutoScaleMode=AutoScaleMode.None;

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

        private void UpdateSize()
        {
            this.Size = Global.LevelSize;

            GameButton.ClearAll();
            CreateMenuButtons();
            InitSettings();
            ShowSettingsMenu();
            CenterToScreen();

            Invalidate();
        }

        private void FindCursor() => _mouseLoc = PointToClient(Cursor.Position);

        private void MainMenu_Load(object sender, EventArgs e)
        {
            _active = true;
            _menuTimer.Start();
            ShowMainMenu();
            Invalidate();
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

            switch (_curMenuState)
            {
                case (MenuState.Start):
                    string titleString = "DRAWED OUT";
                    using (Font titleFont = new Font(Global.SourGummy, 100*Global.BaseScale))
                    {
                        SizeF titleSize = g.MeasureString(titleString, titleFont);
                        float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                        g.DrawString(titleString, titleFont, Brushes.Black, titlePosX, 20); 
                    }
                    break;
                
                case (MenuState.Settings):
                    DrawSettingsStrings(g);
                    UpdateKeyBtnStrings();
                    break;
            }

            GameButton.DrawAll(g);
        }

        private void UpdateKeyBtnStrings()
        {
            if (_rebindAction is null) 
            {
                _jumpRebindBtn.BtnTxt = "Click to rebind";
                _leftRebindBtn.BtnTxt = "Click to rebind";
                _rightRebindBtn.BtnTxt = "Click to rebind";
                _abilityOneRebindBtn.BtnTxt = "Click to rebind";
                _abilityTwoRebindBtn.BtnTxt = "Click to rebind";
                _abilityThreeRebindBtn.BtnTxt = "Click to rebind";
                return;
            }
            switch (_rebindAction.Value)
            {
                case (Keybinds.Actions.MoveLeft):
                    _leftRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.MoveRight):
                    _rightRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Jump):
                    _jumpRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special1):
                    _abilityOneRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special2):
                    _abilityTwoRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special3):
                    _abilityThreeRebindBtn.BtnTxt="Enter new key";
                    break;
            }
        }

        private void DrawSettingsStrings(Graphics g)
        {
            string settingsString = "Settings";
            using (Font settingsFont = new Font(Global.SourGummy, 50*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(settingsString, settingsFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(settingsString, settingsFont, Brushes.Black, 
                        titlePosX, 10);
            }
            string keyHeadingString = "Keybinds";
            using (Font keyHeadingFont = new Font(Global.SourGummy, 30*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(settingsString, keyHeadingFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(keyHeadingString, keyHeadingFont, Brushes.Black, 
                        titlePosX, 0.46f*Height);
            }

            Font defaultFont = Global.DefaultFont;
            float txtXPos = 600*Global.BaseScale; 

            foreach (KeyValuePair<String, int> element in _settingsStringsYPos)
            { 
                g.DrawString(element.Key, defaultFont, 
                        Brushes.Black, txtXPos, element.Value); 
            }

            foreach (KeyValuePair<String, Point> element in _bindingStringsPos)
            {
                g.DrawString(element.Key, defaultFont, 
                        Brushes.Black, element.Value);
            }
        }

        private void MainMenu_MouseDown(object sender, MouseEventArgs e)
        {
            GameButton.ClickSelected();
        }

        private void MainMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4) Application.Exit();
            if (_rebindAction is null) return;

            if (Keybinds.Rebind(e.KeyCode, _rebindAction.Value))
            {
                CreateBindStrings();
                Invalidate();
            }
            else MessageBox.Show("Key is already in use");

            _rebindAction = null;
        }

        private void MainMenu_Quit(object sender, FormClosingEventArgs e)
        {
            _active = false;
        }

        private void ShowMainMenu()
        {
            _curMenuState = MenuState.Start;

            GameButton.HideAll();
            _playBtn.Show();
            _settingsBtn.Show();
            _quitBtn.Show();
        }

        private void ShowSettingsMenu()
        {
            _curMenuState = MenuState.Settings;

            GameButton.HideAll();
            _backgroundBtn.Show();

            _24FpsBtn.Show();
            _30FpsBtn.Show();
            _60FpsBtn.Show();
            _120FpsBtn.Show();

            _720pBtn.Show();
            _1080pBtn.Show();
            _1440pBtn.Show();

            _timeBtn.Show();

            _jumpRebindBtn.Show();
            _leftRebindBtn.Show();
            _rightRebindBtn.Show();
            _abilityOneRebindBtn.Show();
            _abilityTwoRebindBtn.Show();
            _abilityThreeRebindBtn.Show();

            _settingsBackBtn.Show();

            this.Invalidate();
        }

    }
}


