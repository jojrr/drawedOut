namespace drawedOut
{
    public class GameButton
    {
        public static IReadOnlyCollection<GameButton> BtnList => _btnList;
        public static GameButton? SelectedButton { get; private set; }
        public Action ClickEvent { get; private init; }
        public string? BtnTxt { get; set; }
        public Bitmap? BtnImg { get; set; }

        private static List<GameButton> _btnList = new();
        private Rectangle _bounds;
        private bool 
            _showOutline=true,
            _hidden=false;

        public GameButton(Point origin, int width, int height, Action clickEvent, 
                string? txt=null, Bitmap? img=null)
        {
            _bounds = new Rectangle(origin, new Size(width, height));
            ClickEvent = clickEvent;
            BtnTxt = txt;
            BtnImg = img;
        }

        public void Draw(Graphics g)
        {
            TextFormatFlags textFormat =
                TextFormatFlags.WordBreak|
                TextFormatFlags.VerticalCenter|
                TextFormatFlags.HorizontalCenter;

            if (BtnImg is not null) g.DrawImage(BtnImg, _bounds);

            if (BtnTxt is not null)
            {
                TextRenderer.DrawText(
                        g, 
                        BtnTxt, 
                        Global.DefaultFont, 
                        _bounds, 
                        Color.Black, 
                        textFormat);
            }

            if (_showOutline) 
            {
                using (Pen btnPen = new Pen(Color.Black, 8))
                { g.DrawRectangle(btnPen, _bounds); }
            }
        }

        public static void DrawAll(Graphics g)
        {
            foreach (GameButton b in BtnList) 
            { b.Draw(g); }
        }
    }
}
