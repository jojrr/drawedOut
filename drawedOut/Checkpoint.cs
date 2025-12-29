namespace drawedOut
{
    internal class Checkpoint: Entity
    {
        public static List<Checkpoint> CheckPointList = new List<Checkpoint>();
        private static Dictionary<Entity, PointF> _entityPosKeys = new Dictionary<Entity, PointF>();
        private static Checkpoint? _lastSavedPoint = null;
        private const int
            _BASE_WIDTH = 200,
            _BASE_HEIGHT = 140;

        public Checkpoint(Point origin, int width=_BASE_WIDTH, int height=_BASE_HEIGHT)
            : base(origin: origin, width: width, height: height)
        { 
            FindFloor(); 
            CheckPointList.Add(this);
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

            while (y1 <= Global.LevelSize.Height)
            {
                y1 += 1;
                foreach (Platform p in Platform.ActivePlatformList)
                { 
                    if (!p.Hitbox.Contains(new Point(x,y1))) continue;
                    LocationY = p.Hitbox.Top-Height; 
                    return;
                }
            }
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
                using (Pen bvPen = new Pen(Color.BlueViolet, 8))
                { g.DrawRectangle(bvPen, c.Hitbox); }
            }
        }

        public override void CheckActive()
        {
            if (this.DistToMid > Global.EntityLoadThreshold) IsActive = false; 
            else IsActive = true;
        }
    }
}
