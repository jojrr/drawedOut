namespace drawedOut 
{
    internal class GameUI 
    {
        public static List<GameUI> UiElements = new List<GameUI>();

        public PointF Origin { get; private set; }
        public bool Visible { get; private set; }
        public SizeF ElementSize { get; private set; }

        /// <summary>
        /// Creates a GameUI object
        /// </summary>
        /// <param name="origin"> the top-left position </param>
        /// <param name="elementWidth"> The width of the element </param>
        /// <param name="elementHeight"> The Height of the element </param>
        /// <param name="isVisible"> 
        /// If the element should be displayed <br/>
        /// Default: true
        /// </param>
        public GameUI(PointF origin, float elementWidth, float elementHeight, bool isVisible = true)
        {
            Origin = new PointF(origin.X * Global.BaseScale, origin.Y*Global.BaseScale);
            ElementSize = new SizeF(elementWidth*Global.BaseScale, elementHeight*Global.BaseScale);
            Visible = isVisible; 
            UiElements.Add(this);
        }

        /// <summary>
        /// Creates a GameUI object
        /// </summary>
        /// <param name="origin"> the top-left position </param>
        /// <param name="elementSize"> The size of the element </param>
        /// <param name="isVisible"> 
        /// If the element should be displayed <br/>
        /// Default: true
        /// </param>
        public GameUI (PointF origin, SizeF elementSize, bool isVisible = true)
        {
            Origin = new PointF(origin.X * Global.BaseScale, origin.Y*Global.BaseScale);
            ElementSize = new SizeF(elementSize.Width*Global.BaseScale, elementSize.Height*Global.BaseScale);
            Visible = isVisible; 
            UiElements.Add(this);
        }

        /// <summary> Do not call this method. Only call from the overridden method. </summary>
        public virtual void Draw(Graphics g) => throw new Exception("Not overridden in derived class.");
    }

    internal class BarUI : GameUI
    {
        private Brush? _bgBrush;
        private Brush _barBrush; 
        private RectangleF _progressBar;
        private float _maxVal, _baseWidth;

        /// <summary>
        /// Creates a bar UI
        /// </summary>
        /// <param name="origin"> the origin </param>
        public BarUI(PointF origin, float elementWidth, float elementHeight, Brush brush, double maxVal, double startVal,
                Brush? bgBrush = null, float borderScale = 1.0f)
            : base(origin: origin, elementWidth: elementWidth, elementHeight: elementHeight)
        {
            if (borderScale > 1 || 0 > borderScale) throw new ArgumentOutOfRangeException(
                    "Bar scale should be between 0 and 1"
                    );
            if (maxVal < 0) throw new ArgumentOutOfRangeException(
                    "Max value should be bigger than 0"
                    );
            if (startVal < 0 || startVal > maxVal) throw new ArgumentOutOfRangeException(
                    "Starting value should be between 0 and max value"
                    );
            setupBar(origin, elementWidth, elementHeight, borderScale, maxVal, brush, bgBrush);
            double startingVal = (startVal <= -1) ? maxVal : startVal;
            Update((float)startingVal);
        }

        public BarUI(PointF origin, float elementWidth, float elementHeight, Brush brush, double maxVal,
                Brush? bgBrush = null, float borderScale = 0.0f)
            : base(origin: origin, elementWidth: elementWidth, elementHeight: elementHeight)
        {
            if (borderScale > 1 || 0 > borderScale) throw new ArgumentOutOfRangeException(
                    "Border scale should be between 0 and 1"
                    );
            if (maxVal < 0) throw new ArgumentOutOfRangeException(
                    "Max value should be bigger than 0"
                    );
            setupBar(origin, elementWidth, elementHeight, borderScale, maxVal, brush, bgBrush);
            Update(_maxVal);
        }

        private void setupBar(PointF origin, float width, float height, float borderScale, double maxVal, Brush brush, Brush? bgBrush)
        {
            float borderWidth = height/2 * borderScale;
            float scaleMult = Global.BaseScale;

            _progressBar.Y = scaleMult*(origin.Y + borderWidth);
            _progressBar.X = scaleMult*(origin.X + borderWidth);

            _progressBar.Height = scaleMult*(height - (2*borderWidth));
            _progressBar.Width = scaleMult*(width - (2*borderWidth));
            _baseWidth = _progressBar.Width;
            _bgBrush = bgBrush;
            _barBrush = brush;
            _maxVal = (float)maxVal;
        }


        public void Update(float val)
        {
            if (val > _maxVal) CalcBars(_maxVal);
            if (val < 0) CalcBars(0);
            else CalcBars(val);
        }

        private void CalcBars(float val) => _progressBar.Width = _baseWidth * val/_maxVal;

        public override void Draw(Graphics g) 
        {
            RectangleF baseRect = new RectangleF(Origin, ElementSize);
            if (_bgBrush is not null) g.FillRectangle(_bgBrush, baseRect);
            g.FillRectangle(_barBrush, _progressBar);
        }
    }
}

