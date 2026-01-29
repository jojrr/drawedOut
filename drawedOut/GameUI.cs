namespace drawedOut 
{
    public class GameUI 
    {
        public static List<GameUI> UiElements = new List<GameUI>();

        public bool Visible { get; set; }
        public PointF Origin { get; protected set; }
        public SizeF ElementSize { get => _elementSize; protected set => _elementSize = value; }
        public float Width { get => _elementSize.Width; protected set => _elementSize.Width = value; }
        public float Height { get => _elementSize.Height; protected set => _elementSize.Height = value; }

        private SizeF _elementSize;

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
        public Brush SetBarBrush(Brush barBrush) => _barBrush = barBrush;
        public Brush? SetBgBrush(Brush bgBrush) => _bgBrush = bgBrush;

        private Brush? _bgBrush;
        private Brush _barBrush; 
        private RectangleF _progressBar;
        private float _maxVal, _curVal, _baseWidth, _borderScale;

        /// <summary>
        /// Creates a bar UI
        /// </summary>
        /// <param name="origin"> the origin </param>
        public BarUI(PointF origin, float elementWidth, float elementHeight, Brush brush, float maxVal, float startVal,
                Brush? bgBrush = null, float borderScale = 0.0f)
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

            _borderScale = borderScale;
            _bgBrush = bgBrush;
            _barBrush = brush;

            setupBar(origin, elementWidth, elementHeight, maxVal);
            float startingVal = (startVal <= -1) ? maxVal : startVal;
            Update(startingVal);
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

            _borderScale = borderScale;
            _bgBrush = bgBrush;
            _barBrush = brush;

            setupBar(origin, elementWidth, elementHeight, maxVal);
            Update(_maxVal);
        }

        private void setupBar(PointF origin, float width, float height, double maxVal)
        {
            float borderWidth = height/2 * _borderScale;
            float scaleMult = Global.BaseScale;

            _progressBar.Y = scaleMult*(origin.Y + borderWidth);
            _progressBar.X = scaleMult*(origin.X + borderWidth);

            _progressBar.Height = scaleMult*(height - (2*borderWidth));
            _progressBar.Width = scaleMult*(width - (2*borderWidth));
            _baseWidth = _progressBar.Width;
            _maxVal = (float)maxVal;
        }

        public void Update(float val)
        {
            _curVal = (val < 0) ? 0 : Math.Min(_maxVal,val);
            _progressBar.Width = _baseWidth * _curVal/_maxVal;
        }

        public void SetMax(float maxVal, bool keepSize = false)
        {
            if (!keepSize) 
            {
                float sizeFactor = maxVal/_maxVal;
                this.Width *= sizeFactor;
                setupBar(Origin, Width, Height, maxVal);
            }
            _maxVal = maxVal;
            Update(_curVal);
        }

        public override void Draw(Graphics g) 
        {
            if (!Visible) return;
            RectangleF baseRect = new RectangleF(Origin, ElementSize);
            if (_bgBrush is not null) g.FillRectangle(_bgBrush, baseRect);
            g.FillRectangle(_barBrush, _progressBar);
        }
    }
}

