namespace drawedOut
{
    internal class BgObj: Entity
    {
        public Bitmap Sprite { get; init; }

        private static List<BgObj> _bgObjList = new List<BgObj>();
        private const int
            _BASE_WIDTH = 150,
            _BASE_HEIGHT = 150;

        public BgObj(Point origin, Bitmap sprite, int width=_BASE_WIDTH, int height=_BASE_HEIGHT)
            : base(origin: origin, width: width, height: height)
        { 
            FindFloor(); 
            _bgObjList.Add(this);
            Sprite = sprite;
        }


        private RectangleF SpriteRect
        {
            get 
            {
                SizeF recSize = new SizeF(Width,Width);
                PointF point = new PointF(Center.X - recSize.Width/2, Hitbox.Bottom - recSize.Width);
                return new RectangleF(point, recSize);
            }
        }


        private void FindFloor()
        {
            int x = (int)(this.LocationX);
            int y1 = (int)(this.Hitbox.Bottom);

            while (++y1 <= Global.LevelSize.Height)
            {
                foreach (Platform p in Platform.ActivePlatformList)
                { 
                    if (!p.Hitbox.Contains(new Point(x,y1))) continue;
                    LocationY = y1-Height;
                    OriginLocation = new PointF(x,LocationY);
                    return;
                }
                foreach (Platform p in Platform.InactivePlatformList)
                { 
                    if (!p.Hitbox.Contains(new Point(x,y1))) continue;
                    LocationY = y1-Height;
                    OriginLocation = new PointF(x,LocationY);
                    return;
                }
            }
            throw new Exception("No Floor found for bg object");
        }


        public static void DrawAll(Graphics g, Rectangle clientRect)
        {
            foreach (BgObj b in _bgObjList)
            { 
                if (!b.IsActive) continue;
                if (!b.SpriteRect.IntersectsWith(clientRect)) continue;
                g.DrawImage(b.Sprite, b.SpriteRect); 
            }
        }

        public override void CheckActive()
        {
            if (this.DistToMid > Global.EntityLoadThreshold) IsActive = false; 
            else IsActive = true;
        }

        public new static void ClearAllLists()
        { _bgObjList.Clear(); }
    }
}
