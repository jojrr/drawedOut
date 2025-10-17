namespace drawedOut 
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {
        private static int _loadInThreshold;
        public static void setLoadInThreashold(int loadInThreshold) => _loadInThreshold = loadInThreshold;

        private static PointF _centerOfScreen; 
        public static void defineScreenCenter(PointF screenCenter) => _centerOfScreen = screenCenter;

        private PointF _location;
        private PointF _center;

        public PointF Location
        { 
            get => _location;
            set
            {
                _location = value;
                _center = new PointF(value.X + _scaledSize.Width/2, value.Y + _scaledSize.Height/2);
            }
        }

        protected float LocationX
        {
            get => _location.X;
            set => _location.X = value;
        }

        protected float LocationY
        {
            get => _location.Y;
            set => _location.Y = value;
        }

        public PointF Center
        {
            get => _center;
            set
            {
                _center = value;
                _location = new PointF(value.X - _scaledSize.Width/2, value.Y - _scaledSize.Height/2);
            }
        }

        private SizeF _scaledSize;
        private readonly SizeF _size;

        protected float Width { get => _scaledSize.Width;  }
        protected float Height { get => _scaledSize.Height;  }

        private RectangleF _hitbox;
        public RectangleF Hitbox
        {
            get => new RectangleF(_location, _scaledSize); 
            private set => _hitbox = value;
        }

        public static List<Entity> EntityList = new List<Entity>();

        /// <summary>
        /// Creates a hitbox at specified paramters
        /// </summary>
        /// <param name="origin">the point of the top-left of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        /// <param name="level">level which the Entity belongs to. (Default = 0: always loaded)</param>
        /// <param name="chunk">chunk in the level which the entity belongs to. (default = 0: always loaded)</param>
        public Entity(PointF origin, int width, int height)
        {
            _location = origin;
            _size = new Size(width, height);
            _scaledSize = _size;
            Hitbox = new RectangleF(origin, _size); 
            _center = new PointF (Hitbox.X + _size.Width/2, Hitbox.Y + _size.Height/2);

            EntityList.Add(this);
        }

        /// <summary>
        /// Assigns a new horizontal position to the top-left of the hitbox
        /// </summary>
        /// <param name="x">x position</param>
        public void UpdateX(double x)
        {
            _location.X += (float)x;
            _center = new PointF (_location.X + Width/2, _center.Y + Height/2);
        }

        /// <summary>
        /// make the hitbox bigger by specified param
        /// </summary>
        /// <param name="scaleF"> scale to enlarge dimensions </param>
        public void ScaleHitbox(float scaleF)
        {
            _scaledSize = new SizeF (_size.Width*scaleF, _size.Height*scaleF);
        }

        /// <summary>
        /// return to original scaled size before zoom
        /// </summary>
        public void ResetScale()
        {
            _scaledSize = _size;
        }

        public void CheckActive(ref List<Entity> activeList, ref List<Entity> inactiveList, float curCenter)
        {
            float dist = Math.Abs(curCenter - _center.X);

            if (dist <= _loadInThreshold)
            {
                if (activeList.Contains(this)) return;

                activeList.Add(this);
                inactiveList.Remove(this);
                return;
            }

            if (inactiveList.Contains(this)) return;

            inactiveList.Add(this);
            activeList.Add(this);
        }
    }
}
