namespace drawedOut
{
    internal class HpBarUI: GameUI 
    {
        public RectangleF[] HpRectangles;
        public Brush[] HpRecColours;
        public int IconCount { get; private set; }

        private const int BASE_ICON_OFFSET = 50;
        private float _hpIconOffset;

        private int _maxHp;

        public HpBarUI (PointF origin, float barWidth, float barHeight, int maxHp, float scaleF = 1, bool isVisible = true)
            :base( origin: origin, elementWidth: barWidth, elementHeight: barHeight)
        {
            _hpIconOffset = BASE_ICON_OFFSET*Global.BaseScale;

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
            HpRecColours = new Brush[IconCount];

            for (int i = 0; i < IconCount; i++)
            {
                PointF rectangleOrigin = new PointF(this.Origin.X + xOffset, this.Origin.Y);
                this.HpRectangles[i] = new RectangleF(rectangleOrigin, this.ElementSize);

                switch (currentHp)
                {
                    case >= 2:
                        this.HpRecColours[i] = Brushes.Green;
                        break;
                    case 1:
                        this.HpRecColours[i] = Brushes.Orange;
                        break;
                    default:
                        this.HpRecColours[i] = Brushes.DimGray;
                        break;
                }

                xOffset += _hpIconOffset; 
                currentHp -= 2;
            }
        }

        public override void Draw(Graphics g)
        {
            for (int i = 0; i < IconCount; i++) g.FillRectangle(HpRecColours[i], HpRectangles[i]);
        }

    }
}
