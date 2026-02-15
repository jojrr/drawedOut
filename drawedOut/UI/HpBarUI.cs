namespace drawedOut
{
    internal class HpBarUI: GameUI 
    {
        public RectangleF[] HpRectangles;
        public Bitmap[] HpRecImages;
        public int IconCount { get; private set; }

        private static readonly Bitmap _spriteEmpty, _spriteHalf, _spriteFull;
        private static readonly Point DEFAULT_ORIGIN = new Point(90, 50);
        private const int DEFAULT_ICON_OFFSET = 50;
        private const int 
            DEFAULT_WIDTH = 30,
            DEFAULT_HEIGHT = 60;

        private float _hpIconOffset;
        private int _maxHp;

        static HpBarUI()
        {
            string spriteFolder = @"hpBarIcon\";
            _spriteEmpty = Global.GetSingleImage(
                    spriteFolder, 
                    "hpIcon0.png",
                    512, 512);
            _spriteHalf = Global.GetSingleImage(
                    spriteFolder, 
                    "hpIcon1.png",
                    512, 512);
            _spriteFull = Global.GetSingleImage(
                    spriteFolder, 
                    "hpIcon2.png",
                    512, 512);
        }

        public HpBarUI ( int maxHp, PointF origin, float barWidth=DEFAULT_WIDTH, float barHeight=DEFAULT_HEIGHT, float scaleF = 1, bool isVisible = true)
            :base( origin: origin, elementWidth: barWidth, elementHeight: barHeight)
        {
            _hpIconOffset = DEFAULT_ICON_OFFSET*Global.BaseScale;
            UpdateMaxHp(maxHp);
        }

        public HpBarUI ( int maxHp, float barWidth=DEFAULT_WIDTH, float barHeight=DEFAULT_HEIGHT, float scaleF = 1, bool isVisible = true)
            :base( origin: DEFAULT_ORIGIN, elementWidth: barWidth, elementHeight: barHeight)
        {
            _hpIconOffset = DEFAULT_ICON_OFFSET*Global.BaseScale;
            UpdateMaxHp(maxHp);
        }

        public void UpdateMaxHp(int maxHp)
        {
            if (maxHp <= 0) throw new Exception("Max HP must be bigger than zero.");

            _maxHp = maxHp;
            IconCount = (int)Math.Ceiling(_maxHp / 2.0F);
            ComputeHP(_maxHp);
        }

        public void ComputeHP(int currentHp)
        {
            float xOffset = 0;

            HpRectangles = new RectangleF[IconCount];
            HpRecImages = new Bitmap[IconCount];

            for (int i = 0; i < IconCount; i++)
            {
                PointF rectangleOrigin = new PointF(this.Origin.X + xOffset, this.Origin.Y);
                this.HpRectangles[i] = new RectangleF(rectangleOrigin, this.ElementSize);

                switch (currentHp)
                {
                    case >= 2:
                        this.HpRecImages[i] = _spriteFull;
                        break;
                    case 1:
                        this.HpRecImages[i] = _spriteHalf;
                        break;
                    default:
                        this.HpRecImages[i] = _spriteEmpty;
                        break;
                }

                xOffset += _hpIconOffset; 
                currentHp -= 2;
            }
        }

        public override void Draw(Graphics g)
        {
            for (int i = 0; i < IconCount; i++) g.DrawImage(HpRecImages[i], HpRectangles[i]);
        }

    }
}
