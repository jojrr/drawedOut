namespace drawedOut
{
    public class GameButton
    {
        public static IReadOnlyCollection<GameButton> BtnList => _btnList;
        public static GameButton? SelectedButton { get; private set; }
        public Action ClickEvent { get; private init; }
        public string? BtnTxt { get; set; }

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
        }

        public void Show() => _hidden = false;
        public void Hide() => _hidden = true;

        public bool CheckMouseHover(Point mouseP) 
        {
            if (_bounds.Contains(mouseP)) 
            {
                SelectedButton=this;
                return true;
            }
            return false;
        }

        public void Draw(Graphics g)
        {
            if (_hidden) return;

            TextFormatFlags textFormat =
                TextFormatFlags.WordBreak|
                TextFormatFlags.VerticalCenter|
                TextFormatFlags.HorizontalCenter;

            Color txtColor = Color.Black;
            if (SelectedButton==this)
            {
                using (Brush bgBrush = Brushes.Black)
                { g.FillRectangle(bgBrush, _bounds); }
                txtColor = Color.White;
            }

            if (BtnTxt is not null)
            {
                TextRenderer.DrawText(
                        g, 
                        BtnTxt, 
                        Global.DefaultFont, 
                        _bounds, 
                        txtColor,
                        textFormat);
            }

            if (_showOutline) 
            {
                using (Pen btnPen = new Pen(Color.Black, 4))
                { g.DrawRectangle(btnPen, _bounds); }
            }
        }

        public static void CheckAllMouseHover(Point mouseP)
        {
            foreach (GameButton btn in BtnList)
            { if (btn.CheckMouseHover(mouseP)) return; }
        }


        public static void DrawAll(Graphics g)
        {
            foreach (GameButton b in BtnList) 
            { b.Draw(g); }
        }
    }
}
