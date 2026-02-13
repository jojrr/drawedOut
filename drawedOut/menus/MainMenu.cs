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

        private MenuState _curMenuState;

        public MainMenu()
        {
            InitializeComponent();
            Global.LevelResolution = Global.Resolutions.p1080;
            UpdateSize();
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

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
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateMenuButtons();
            InitSettings();

            Invalidate();
        }

        private void FindCursor() => _mouseLoc = PointToClient(Cursor.Position);

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
                    using (Font titleFont = new Font("Sour Gummy", 100*Global.BaseScale))
                    {
                        SizeF titleSize = g.MeasureString(titleString, titleFont);
                        float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                        g.DrawString(titleString, titleFont, Brushes.Black, titlePosX, 20); 
                    }
                    break;
                
                case (MenuState.Settings):
                    DrawSettingsStrings(g);
                    break;
            }

            GameButton.DrawAll(g);
        }

        private void DrawSettingsStrings(Graphics g)
        {
            string settingsString = "Settings";
            using (Font settingsFont = new Font("Sour Gummy", 50*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(settingsString, settingsFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(settingsString, settingsFont, Brushes.Black, 
                        titlePosX, 10);
            }
            string keyHeadingString = "Keybinds";
            using (Font keyHeadingFont = new Font("Sour Gummy", 30*Global.BaseScale))
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

            TryInvoke(Invalidate);
        }

    }
}


