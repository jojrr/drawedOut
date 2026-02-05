namespace drawedOut
{
    public partial class MainMenu : Form
    {
        private const int _TICK_MS = 30;
        private static Thread _menuTimer;
        private static bool _active;

        private static GameButton _quitBtn;
        private static GameButton _settingsBtn;
        private static GameButton _playBtn;

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

                    // Point mouseLoc = this.Invoke(new Func<Point>(() => PointToClient(Cursor.Position)));
                    Point mouseLoc = PointToClient(Cursor.Position);
                    bool needUpdate = GameButton.CheckAllMouseHover(mouseLoc);
                    if (needUpdate) TryInvoke(this.Refresh);
                    timerSW.Restart();
                }
            });
        }

        private void CreateButtons()
        {
            _quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.75f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: this.Close,
                    fontScale: 1.2f,
                    txt: "Quit");
            _settingsBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.55f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: (()=>{}),
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
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            _active = true;
            _menuTimer.Start();
            Invalidate();
        }

        private void PlayBtnClickEvent() => TryInvoke(OpenLevelMenu); 

        private void OpenLevelMenu()
        {
            TutorialLevel level = new TutorialLevel();
            _active=false;
            this.Hide();
            level.Show();
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
            FormHandler.CloseHandler();
        }
    }
}


