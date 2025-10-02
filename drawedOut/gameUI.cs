namespace drawedOut 
{
    internal class gameUI 
    {
        public static List<gameUI> UiElements = new List<gameUI>();

        public PointF Origin { get; set; }
        public bool Visible { get; set; }
        public SizeF ElementSize { get; set; }
        public float ScaleF { get; }


        public gameUI ( PointF origin, float elementWidth, float elementHeight, float scaleF = 1, bool isVisible = true)
        {
            Origin = new PointF(origin.X * scaleF, origin.Y * scaleF);
            ElementSize = new SizeF(elementWidth*scaleF, elementHeight*scaleF);
            Visible = isVisible; 
            ScaleF = scaleF;
            UiElements.Add(this);
        }


    }
}
