namespace drawedOut
{
    internal class Checkpoint: Entity
    {
        public static List<Checkpoint> CheckPointList = new List<Checkpoint>();
        private static Dictionary<Entity, PointF> _entityPosKeys = new Dictionary<Entity, PointF>();
        private static Checkpoint? _lastSavedPoint = null;
        private static Bitmap _defaultSprite, _usedSprite;
        private const int
            _BASE_WIDTH = 200,
            _BASE_HEIGHT = 140;

        static Checkpoint()
        {
            _defaultSprite = Global.GetSingleImage(@"fillerAnim\", "placeHolder002.JPEG");
            _usedSprite = Global.GetSingleImage(@"fillerAnim\", "placeHolder006.JPEG");
        }
        public Checkpoint(Point origin, int width=_BASE_WIDTH, int height=_BASE_HEIGHT)
            : base(origin: origin, width: width, height: height)
        { 
            FindFloor(); 
            CheckPointList.Add(this);
        }

        private RectangleF SpriteRect
        {
            get 
            {
                SizeF recSize = new SizeF(_BASE_WIDTH,_BASE_WIDTH);
                PointF point = new PointF(Center.X - _BASE_WIDTH/2, LocationY-_BASE_WIDTH+Height);
                return new RectangleF(point, recSize);
            }
        }


        public void SaveState()
        {
            if (_lastSavedPoint == this) return;

            _entityPosKeys.Clear();
            foreach (Entity entity in EntityList)
            {
                if (entity is Projectile) continue;
                _entityPosKeys.Add(entity, entity.Location);
            }
            _lastSavedPoint = this;
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
                    return;
                }
            }
            throw new Exception("No Floor found for checkpoint");
        }

        ///<summary>
        ///Loads the state of the entities stored in the dictionary.
        ///</summary>
        public static void LoadState() 
        { 
            if (_lastSavedPoint is null) return;
            foreach (Entity entity in EntityList) entity.Location = _entityPosKeys[entity];
        }

        public static void Draw(Graphics g)
        {
            foreach (Checkpoint c in CheckPointList)
            {
                if (!c.IsActive) continue;
                if (_lastSavedPoint == c) g.DrawImage(_usedSprite, c.SpriteRect);
                else g.DrawImage(_defaultSprite, c.SpriteRect);
            }
        }

        public override void CheckActive()
        {
            if (this.DistToMid > Global.EntityLoadThreshold) IsActive = false; 
            else IsActive = true;
        }
    }
}
