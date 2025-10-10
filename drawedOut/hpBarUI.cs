namespace drawedOut
{
    internal class hpBarUI: gameUI 
    {
        public RectangleF[] HpRectangles;
        public Brush[] HpRecColours;
        public int IconCount { get; }

        private const int baseIconOffset = 50;
        public readonly float hpIconOffset;

        public hpBarUI (PointF origin, float barWidth, float barHeight, int iconCount, float scaleF = 1, bool isVisible = true)
            :base( origin: origin, elementWidth: barWidth, elementHeight: barHeight, scaleF: scaleF , isVisible: isVisible) 
        {
            HpRectangles = new RectangleF[iconCount];
            HpRecColours = new Brush[iconCount];
            IconCount = iconCount;
            hpIconOffset = baseIconOffset*scaleF;
        }

        public void computeHP(int currentHp)
        {
            float xOffset = 0;

            for (int i = 0; i < this.IconCount; i++)
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

                xOffset += hpIconOffset * this.ScaleF;
                currentHp -= 2;
            }
        }
    }
}
