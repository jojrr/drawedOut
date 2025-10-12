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

        protected PointF location;
        protected PointF center;

        public PointF Location {get;}
        public PointF Center {get;}

        private SizeF _scaledSize;
        private readonly SizeF Size;

        protected float Width { get => _scaledSize.Width;  }
        protected float Height { get => _scaledSize.Height;  }

        protected RectangleF Hitbox { get; private set;}

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
            location = origin;
            Size = new Size(width, height);
            _scaledSize = Size;
            Hitbox = new RectangleF(origin, Size); 
            center = new PointF (Hitbox.X + Size.Width/2, Hitbox.Y + Size.Height/2);

            EntityList.Add(this);
        }

        /// <summary>
        /// returns the hitbox as a rectangle
        /// </summary>
        /// <returns>hitbox of type rectangle</returns>
        public RectangleF GetHitbox() 
        {
            Hitbox = new RectangleF( location, _scaledSize ); // WARNING: used to be calclated after moving every time - might break something havent tested yet.
            return Hitbox; 
        }


        /// <summary>
        /// Assigns a new position to the top-left of the hitbox
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public void UpdateLocation(double x, double y)
        {
            location = new PointF((float)x, (float)y);
            center = new PointF (location.X + Width/2, location.Y + Height/2);
        }

        /// <summary>
        /// Assigns a new horizontal position to the top-left of the hitbox
        /// </summary>
        /// <param name="x">x position</param>
        public void UpdateLocation(double x)
        {
            location = new PointF((float)x, location.Y);
            center = new PointF (location.X + Width/2, center.Y);
        }

        /// <summary>
        /// Assigns a new position to the center of the hitbox
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public void Updatecenter(float x, float y)
        {
            center = new PointF(x, y);
            location = new PointF (center.X - Width/2, center.Y - Height/2);
        }

        /// <summary>
        /// make the hitbox bigger by specified param
        /// </summary>
        /// <param name="scaleF"> scale to enlarge dimensions </param>
        public void ScaleHitbox(float scaleF)
        {
            _scaledSize = new SizeF (Size.Width*scaleF, Size.Height*scaleF);
        }

        /// <summary>
        /// return to original scaled size before zoom
        /// </summary>
        public void ResetScale()
        {
            _scaledSize = Size;
        }

        public void CheckActive(ref List<Entity> activeList, ref List<Entity> inactiveList, float curcenter)
        {
            float dist = Math.Abs(curcenter - center.X);

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
