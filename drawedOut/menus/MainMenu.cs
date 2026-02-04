namespace drawedOut
{
    public partial class MainMenu : Form
    {

        public MainMenu()
        {
            InitializeComponent();
            this.Size = Global.LevelSize;
            // this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainMenu_Paint(object sender, PaintEventArgs e)
        {
            using (Graphics g = e.Graphics)
            {
                string titleString = "DRAWED OUT";
                using (Font titleFont = new Font("Sour Gummy", 100))
                {
                    SizeF titleSize = g.MeasureString(titleString, titleFont);
                    float titlePosX = ClientSize.Width/2 - (titleSize.Width/2);
                    g.DrawString(titleString, titleFont, Brushes.Black, titlePosX, 20); 
                }
            }
        }

        private void MainMenu_MouseDown(object sender, MouseEventArgs e)
        {
        }
    }
}


