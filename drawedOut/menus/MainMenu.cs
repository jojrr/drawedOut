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

            Preferences.LoadInstance(SaveData.GetSettings());
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
            CreateLevelButtons();
            InitSettings();
            ShowSettingsMenu();
            CenterToScreen();

            Invalidate();
        }

        private void FindCursor() => _mouseLoc = PointToClient(Cursor.Position);

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

                case (MenuState.Levels):
                    DrawLevelMenu(g);
                    break;
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
            if (_rebindAction is null) return;

            if (Keybinds.Rebind(e.KeyCode, _rebindAction.Value))
            {
                CreateBindStrings();
                Invalidate();
            }
            else MessageBox.Show("Key is already in use");

            _rebindAction = null;
        }

        private void QuitGame()
        {
            FormHandler.CloseHandler();
            Application.Exit();
        }

        private void MainMenu_Closing(object sender, FormClosingEventArgs e)
        { 
            GameButton.ClearAll();
            _active = false; 
        }


# region main menu
        private void MainMenu_Load(object sender, EventArgs e)
        {
            _active = true;
            _menuTimer.Start();
            ShowMainMenu();
            Invalidate();
        }

        private void ShowMainMenu()
        {
            _curMenuState = MenuState.Start;

            GameButton.HideAll();
            _playBtn.Show();
            _settingsBtn.Show();
            _quitBtn.Show();
        }

        private void OpenLevelMenu()
        {
            _curMenuState = MenuState.Levels;

            GameButton.HideAll();

            _tutorialBtn.Show();
            _level1Btn.Show();
            _level2Btn.Show();

            _backBtn.Show();
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

            _backBtn.Show();

            this.Invalidate();
        }
# endregion



# region settings menu

        private void DrawSettingsStrings(Graphics g)
        {
            string settingsString = "Settings";
            using (Font settingsFont = new Font(Global.SourGummy, 55*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(settingsString, settingsFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(settingsString, settingsFont, Brushes.Black, 
                        titlePosX, 20);
            }
            string keyHeadingString = "Keybinds";
            using (Font keyHeadingFont = new Font(Global.SourGummy, 40*Global.BaseScale))
            {
                SizeF titleSize = g.MeasureString(settingsString, keyHeadingFont);
                float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                g.DrawString(keyHeadingString, keyHeadingFont, Brushes.Black, 
                        titlePosX, 0.5f*Height);
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


        private void BgBtnClick() 
        {
            Preferences.ShowBG=!Preferences.ShowBG;
            _backgroundBtn.BtnTxt = BoolToString(Preferences.ShowBG);
            SaveData.SaveSettings();
            Invalidate();
        }

        private void TimeBtnClick()
        {
            Preferences.ShowTime=!Preferences.ShowTime;
            _timeBtn.BtnTxt = BoolToString(Preferences.ShowTime);
            SaveData.SaveSettings();
            Invalidate();
        }

        private void FpsBtnClick(UInt16 fps)
        {
            if (Global.GameTickFreq == fps) return;
            Global.GameTickFreq = fps;
            SaveData.SaveSettings();
            CreateSettingsStrings();
        }

        private void resBtnClick(Global.Resolutions newRes)
        {
            if (newRes == Global.LevelResolution) return;

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to set the resolution to {newRes.ToString()}",
                "confirm change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            Global.LevelResolution = newRes;
            CreateSettingsStrings();
            SaveData.SaveSettings();
            UpdateSize();
        }
# endregion

# region Levels Menu
        private void DrawLevelMenu(Graphics g)
        {
            int absCompDist = _level1Btn.Y - _tutorialBtn.Y;
            for (UInt16 i = 0; i < 3; i++)
            {
                int componentDistnace = absCompDist*i;

                g.DrawString(
                        $"Fastest time: {TimeToString(SaveData.GetFastestScore(i))}", 
                        Global.DefaultFont,
                        Brushes.Black,
                        _tutorialBtn.X,
                        _tutorialBtn.Rect.Bottom + _timeOffsetY + componentDistnace);

                using (Pen pen = new Pen(Color.Black, (int)(4*Global.BaseScale)))
                {
                    g.DrawRectangle(
                            pen,
                            _tutorialBtn.Rect.Right + (int)(10*Global.BaseScale),
                            _tutorialBtn.Y + componentDistnace,
                            _rankSize,
                            _rankSize);
                }
            }
        }

        // private static string TimeToString(double timeS)
        // {
        //     TimeSpan time = TimeSpan.FromSeconds(timeS);
        //     return time.ToString(@"mm\:ss\:ff");
        // }
        
        private static string TimeToString(double? timeS)
        {
            if (timeS is null) return "--:--.--";

            int mins = (int)Math.Floor(timeS.Value)/60;
            int secs = (int)(timeS%60);
            int ms = (int)Math.Round(timeS.Value * 100) % 100;
            return $"{mins:00}:{secs:00}.{ms:00}";
        }

        private void LoadTutorial()
        {
            this.Close();
            TutorialLevel level = new TutorialLevel();
            level.Show();
        }
        private void LoadLevel1()
        {
        }
        private void LoadLevel2()
        {
        }
#endregion
    }
}


