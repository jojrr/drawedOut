namespace drawedOut
{
    internal class Checkpoint: Entity
    {
        public static IReadOnlyCollection<Checkpoint> CheckPointList => _checkPointList;

        private static HashSet<Checkpoint> _checkPointList = new HashSet<Checkpoint>();
        private static Checkpoint? _lastSavedPoint = null;
        private static PointF? _storePlayerLocation = null;
        private static Bitmap _defaultSprite, _usedSprite;
        private const int
            _BASE_WIDTH = 200,
            _BASE_HEIGHT = 140;
        private static float _levelXOffset; 

        static Checkpoint()
        {
            string spriteFolder = @"checkpointSprites\";
            _defaultSprite = Global.GetSingleImage(spriteFolder, "checkpointInactive.png");
            _usedSprite = Global.GetSingleImage(spriteFolder, "checkpointActive.png");
        }
        public Checkpoint(Point origin, int width=_BASE_WIDTH, int height=_BASE_HEIGHT)
            : base(origin: origin, width: width, height: height)
        { 
            FindFloor(); 
            _checkPointList.Add(this);
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


        public void SaveState(Player player, Platform basePlat)
        {
            if (_lastSavedPoint == this) return;
            _levelXOffset = basePlat.OriginLocation.X - basePlat.LocationX;
            _storePlayerLocation = player.Location;
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
            throw new Exception("No Floor found for checkpoint");
        }

        ///<summary>
        ///Loads the state of the entities stored in the dictionary.
        ///</summary>
        public static void LoadState()
        { 
            foreach (Projectile p in Projectile.ProjectileList) p.Dipose();
            foreach (Entity entity in EntityList) 
            {
                if (entity is Player && _storePlayerLocation is not null)
                { 
                    entity.Location = _storePlayerLocation.Value; 
                    continue;
                }
                entity.Reset();
                entity.LocationX -= _levelXOffset;
            }
        }

        public static void DrawAll(Graphics g)
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

        public new static void ClearAllLists()
        {
            _lastSavedPoint=null;
            _checkPointList.Clear();
        }
    }
}
