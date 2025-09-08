using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawedOut
{
    internal class hpBarUI: gameUI 
    {
        public RectangleF[] HpRectangles;
        public Brush[] HpRecColours;
        public int IconCount { get; }

        public hpBarUI (PointF origin, float barWidth, float barHeight, int iconCount, float scaleF = 1, bool isVisible = true)
            :base( origin: origin, elementWidth: barWidth, elementHeight: barHeight, scaleF: scaleF , isVisible: isVisible) 
        {
            HpRectangles = new RectangleF[iconCount];
            HpRecColours = new Brush[iconCount];
            IconCount = iconCount;
        }
    }
}
