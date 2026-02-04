namespace drawedOut
{
    public partial class MainMenu : Form
    {
        private const int _TICK_MS = 30;
        private static Thread _menuTimer;
        private static bool _active;

        private static GameButton _quitBtn;

        public MainMenu()
        {
            InitializeComponent();
            this.Size = Global.LevelSize;
            // this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

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
            Size quitSize = new Size(
                    (int)(0.1*ClientSize.Width*Global.BaseScale), 
                    (int)(0.05*ClientSize.Width*Global.BaseScale));
            _quitBtn = new GameButton(
                    origin: new Point((ClientSize.Width-quitSize.Width)/2, (int)(ClientSize.Height*0.9-quitSize.Height)),
                    width: quitSize.Width,
                    height: quitSize.Height,
                    clickEvent: this.Close,
                    txt: "Quit");
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            _active = true;
            _menuTimer.Start();
            Invalidate();
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
            using (Graphics g = e.Graphics)
            {
                string titleString = "DRAWED OUT";
                using (Font titleFont = new Font("Sour Gummy", 100*Global.BaseScale))
                {
                    SizeF titleSize = g.MeasureString(titleString, titleFont);
                    float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                    g.DrawString(titleString, titleFont, Brushes.Black, titlePosX, 20); 
                }

                GameButton.DrawAll(g);

            }
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


