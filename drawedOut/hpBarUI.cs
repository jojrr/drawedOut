namespace drawedOut
{
    internal class hpBarUI: gameUI 
    {
        public RectangleF[] HpRectangles;
        public Brush[] HpRecColours;
        public int IconCount { get; private set; }

        private const int _baseIconOffset = 50;
        private float _hpIconOffset;

        private int _maxHp;

        public hpBarUI (PointF origin, float barWidth, float barHeight, int maxHp, float scaleF = 1, bool isVisible = true)
            :base( origin: origin, elementWidth: barWidth, elementHeight: barHeight, scaleF: scaleF , isVisible: isVisible) 
        {
            _hpIconOffset = _baseIconOffset*scaleF;

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

                xOffset += _hpIconOffset * this.ScaleF;
                currentHp -= 2;
            }
        }
    }
}
