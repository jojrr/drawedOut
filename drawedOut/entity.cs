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
        private readonly SizeF _baseSize;

        protected SizeF Size { get => _scaledSize;  }
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
            _baseSize = new Size(width, height);
            _scaledSize = _baseSize;
            Hitbox = new RectangleF(origin, _baseSize); 
            _center = new PointF (Hitbox.X + _baseSize.Width/2, Hitbox.Y + _baseSize.Height/2);

            EntityList.Add(this);
        }

        /// <summary>
        /// Increments the horizontal position of the entity by the given amount
        /// </summary>
        /// <param name="x">amount to increment X </param>
        public void UpdateX(double x)
        {
            float fltX = (float)x;
            if (x == 0) return;
            _location.X += fltX;
            _center.X += fltX;
        }

        /// <summary>
        /// make the hitbox bigger by specified param
        /// </summary>
        /// <param name="scaleF"> scale to enlarge dimensions </param>
        public void ScaleHitbox(float scaleF)
        {
            _scaledSize = new SizeF (_baseSize.Width*scaleF, _baseSize.Height*scaleF);
        }

        /// <summary>
        /// return to original scaled size before zoom
        /// </summary>
        public void ResetScale() => _scaledSize = _baseSize; 

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
