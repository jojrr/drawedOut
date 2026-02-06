namespace drawedOut
{
    public class GameButton
    {
        public static IReadOnlyCollection<GameButton> BtnList => _btnList;
        public static GameButton? SelectedButton { get; private set; }
        public int X { get => _bounds.X; set => _bounds.X = value; }
        public int Y { get => _bounds.Y; set => _bounds.Y = value; }
        public Action ClickEvent { get; private init; }
        public string? BtnTxt { get; set; }
        public Size BoundSize => _bounds.Size;

        private static List<GameButton> _btnList = new();
        private Rectangle _bounds;
        private int _fontSize;
        private bool 
            _showOutline=true,
            _hidden=false;

        /// <summary> Constructor for GameButton </summary>
        /// <param name="origin"> Center x position of the button </param>
        /// <param name="recSize"> Size of the rectangle </param>
        /// <param name="clickEvent">
        /// The action that is performed when the button is clicked.
        /// </param>
        public GameButton(Point origin, Size recSize, Action clickEvent, 
                string? txt=null, Bitmap? img=null, int fontSize=Global._DEFAULT_FONT_SIZE, float fontScale=1)
        {
            float bScale = Global.BaseScale;
            _bounds = new Rectangle(
                    (int)(origin.X*bScale), 
                    (int)(origin.Y*bScale), 
                    (int)(recSize.Width*bScale), 
                    (int)(recSize.Height*bScale));
            ClickEvent = clickEvent;
            BtnTxt = txt;
            _fontSize = (int)(fontSize*fontScale*bScale);
            _btnList.Add(this);
        }

        /// <summary>
        /// Constructor for GameButton that uses sizes and positions relative to the form.
        /// </summary>
        /// <param name="xCenterPos">
        /// Center x position of the button relative to the window <br/>
        /// 0-1 where 1 = width of the screen.
        /// </param>
        /// <param name="yCenterPos">
        /// Center y position of the button relative to the window <br/>
        /// 0-1 where 1 = height of the screen.
        /// </param>
        /// <param name="relWidth">
        /// The width of the button relative to the window. <br/>
        /// 0-1 where 1 = width of window.
        /// </param>
        /// <param name="relHeight">
        /// The height of the button relative to the window. <br/> 
        /// 0-1 where 1 = height of window.
        /// </param>
        /// <param name="clickEvent">
        /// The action that is performed when the button is clicked.
        /// </param>
        public GameButton(float xCenterPos, float yCenterPos, float relWidth, float relHeight, Action clickEvent, 
                string? txt=null, Bitmap? img=null, int fontSize=Global._DEFAULT_FONT_SIZE, float fontScale=1)
        {
            ClickEvent = clickEvent;
            BtnTxt = txt;

            float bScale = Global.BaseScale;
            Size clientSize = Global.LevelSize;

            Math.Clamp(xCenterPos, 0, 1);
            Math.Clamp(yCenterPos, 0, 1);
            Math.Clamp(relWidth, 0, 1);
            Math.Clamp(relHeight, 0, 1);

            int width = (int)(relWidth*clientSize.Width);
            int height = (int)(relHeight*clientSize.Height);
            int recX = (int)(xCenterPos*clientSize.Width)-width/2;
            int recY = (int)(yCenterPos*clientSize.Height)-height/2;
            _bounds = new Rectangle(recX, recY, width, height);

            _fontSize = (int)(fontSize*fontScale*bScale);
            _btnList.Add(this);
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
                using (Brush bgBrush = new SolidBrush(Color.FromArgb(255, 20, 20, 20)))
                { g.FillRectangle(bgBrush, _bounds); }
                txtColor = Color.White;
            }

            if (_showOutline) 
            {
                using (Pen btnPen = new Pen(Color.Black, 4))
                { g.DrawRectangle(btnPen, _bounds); }
            }

            if (BtnTxt is null) return;

            using (Font btnFont = new Font("Sour Gummy", _fontSize))
            {
                TextRenderer.DrawText(
                        g, 
                        BtnTxt, 
                        btnFont,
                        _bounds, 
                        txtColor,
                        textFormat);
            }
        }

        public static bool CheckAllMouseHover(Point mouseP)
        {
            GameButton? prevHover = SelectedButton;
            SelectedButton = null;
            foreach (GameButton btn in BtnList)
            { if (btn.CheckMouseHover(mouseP)) break; }
            if (prevHover != SelectedButton) return true;
            return false;
        }

        public static void DrawAll(Graphics g)
        {
            foreach (GameButton b in BtnList) 
            { b.Draw(g); }
        }

        public static void ClickSelected()
        {
            if (SelectedButton is not null)
            { SelectedButton.ClickEvent(); }
        }

        public static void ClearAll() => _btnList.Clear();
    }
}
