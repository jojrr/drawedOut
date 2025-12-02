namespace drawedOut 
{
    internal class GameUI 
    {
        public static List<GameUI> UiElements = new List<GameUI>();

        public PointF Origin { get; private set; }
        public bool Visible { get; private set; }
        public SizeF ElementSize { get; private set; }

        public GameUI (PointF origin, float elementWidth, float elementHeight, bool isVisible = true)
        {
            Origin = new PointF(origin.X * Global.BaseScale, origin.Y*Global.BaseScale);
            ElementSize = new SizeF(elementWidth*Global.BaseScale, elementHeight*Global.BaseScale);
            Visible = isVisible; 
            UiElements.Add(this);
        }
    }
}

