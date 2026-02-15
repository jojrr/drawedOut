namespace drawedOut 
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {
        public static IReadOnlyCollection<Entity> EntityList => _entityList;
        public SizeF Size { get => _scaledSize; }
        public float Width { get => _scaledSize.Width; }
        public float Height { get => _scaledSize.Height; }
        public RectangleF Hitbox { get => _calcHitbox(); }
        public PointF OriginLocation { get => _originalLocation; protected set => _originalLocation = value; } 
        public bool IsActive { get => _isActive; protected set => _isActive = value; }

        ///<summary>
        ///The distance between the object's center and the center of the screen.
        ///Distance is scalar
        ///</summary>
        protected float DistToMid { get => Math.Abs(Center.X - Global.CenterOfScreen.X); }

        private static HashSet<Entity> _entityList = new HashSet<Entity>();
        private static HashSet<Entity> _toRemoveList = new HashSet<Entity>();

        private bool _isActive = false;
        private SizeF _scaledSize;
        private RectangleF _hitbox;
        private readonly SizeF _baseSize;
        private PointF 
            _originalLocation,
            _location,
            _center;

        public PointF Location
        { 
            get => _location;
            set
            {
                _location = value;
                _center = new PointF(value.X + _scaledSize.Width/2, value.Y + _scaledSize.Height/2);
                _calcHitbox();
            }
        }

        public float LocationX
        {
            get => _location.X;
            set 
            {
                _location.X = value;
                _center = new PointF(value + _scaledSize.Width/2, _center.Y);
                _calcHitbox();
            }
        }
        public float LocationY
        {
            get => _location.Y;
            protected set 
            {
                _location.Y = value;
                _center = new PointF(_center.X, value + _scaledSize.Height/2);
                _calcHitbox();
            }
        }

        public PointF Center
        {
            get => _center;
            set
            {
                _center = value;
                _location = new PointF(value.X - _scaledSize.Width/2, value.Y - _scaledSize.Height/2);
                _calcHitbox();
            }
        }

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
            _location = new PointF(origin.X * Global.BaseScale, origin.Y * Global.BaseScale);
            _baseSize = new SizeF(width*Global.BaseScale, height*Global.BaseScale);
            _scaledSize = _baseSize;
            _hitbox = new RectangleF(_location, _scaledSize); 
            _center = new PointF (_hitbox.X + _scaledSize.Width/2, _hitbox.Y + _scaledSize.Height/2);
            _originalLocation = _location;

            _entityList.Add(this);
        }

        public static void DisposeRemoved()
        {
            foreach (Entity e in _toRemoveList) _entityList.Remove(e);
            _toRemoveList.Clear();
        }

        public static void ClearAllLists()
        {
            _toRemoveList.Clear();
            _entityList.Clear();
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

        ///<summary>
        ///removes this entity from the _entityList
        ///</summary>
        public void Delete() => _toRemoveList.Add(this);

        ///<summary>
        ///Reset the entity to the state that the entity was initialised in
        ///</summary>
        public virtual void Reset() => Location = _originalLocation;

        private RectangleF _calcHitbox() 
        {
            _hitbox = new RectangleF(_location, _scaledSize);;
            return _hitbox;
        }

        /// <summary>
        /// make the hitbox bigger by specified param
        /// </summary>
        /// <param name="scaleF"> scale to enlarge dimensions </param>
        public void ScaleHitbox(float scaleF) => _scaledSize = new SizeF(_baseSize.Width*scaleF, _baseSize.Height*scaleF);

        /// <summary>
        /// return to original scaled size before zoom
        /// </summary>
        public void ResetScale() => _scaledSize = _baseSize; 

        public virtual void CheckActive() => throw new Exception($"CheckActive() is not implemented in {this.GetType()}");
    }
}
