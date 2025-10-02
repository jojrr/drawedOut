using System.Collections.Concurrent;

namespace drawedOut 
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {

        protected PointF Location;
        protected PointF Center;

        protected SizeF scaledSize;
        public SizeF Size { get;  protected set; }
        protected float Width { get { return scaledSize.Width; } }
        protected float Height { get { return scaledSize.Height; } }

        protected RectangleF Hitbox;
        /// <summary>
        /// returns the hitbox as a rectangle
        /// </summary>
        /// <returns>hitbox of type rectangle</returns>
        public RectangleF getHitbox() { return Hitbox; }


        protected const int TotalLevels = 1;
        protected static readonly int[] ChunksInLvl = { 3 };

        public static ConcurrentBag<Entity>[][] EntityList = new ConcurrentBag<Entity>[TotalLevels][];


        static Entity()
        {
            /*
            Loops through all levels and looks at the number of chunks of each level stored in the array
            [ChunksInLvl] at each according level and initalises the jagged array [CharacterList] accordingly
            */
            for (int level = 0; level < TotalLevels; level++)
            {
                int chunks = ChunksInLvl[level];
                EntityList[level] = new ConcurrentBag<Entity>[chunks];

                //loops through each chunk and adds the list into the dimension of said chunk
                for (int i = 0; i < chunks; i++)
                {
                    EntityList[level][i] = new ConcurrentBag<Entity>();
                }
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
        public Entity(PointF origin, int width, int height, int level = 0, int chunk = 0)
        {
            Location = origin;
            Size = new Size( width, height);
            scaledSize = Size;
            Hitbox = new RectangleF(origin, Size); 
            Center = new PointF (Hitbox.X + Size.Width/2, Hitbox.Y + Size.Height/2);

            EntityList[level][chunk].Add(this);
        }

        /// <summary>
        /// Assigns a new position to the top-left of the hitbox
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void updateLocation(double x, double y)
        {
            Location = new PointF((float)x, (float)y);
            Center = new PointF (Location.X + Width/2, Location.Y + Height/2);
            Hitbox = new RectangleF(Location, scaledSize);
        }

        public void updateLocation(double x)
        {
            Location.X = (float)x;
            Center.X = (float)x;
            Hitbox = new RectangleF(Location, scaledSize);
        }

        public void updateCenter(float x, float y)
        {
            Center = new PointF(x, y);
            Location = new PointF (Center.X - Width/2, Center.Y - Height/2);
            Hitbox = new RectangleF(Location, scaledSize);
        }

        /// <summary>
        /// returns the point of the center
        /// </summary>
        /// <returns></returns>
        public PointF getCenter()
        {
            return Center;
        }

        /// <summary>
        /// returns the point of the top-left 
        /// </summary>
        /// <returns></returns>
        public PointF getLocation() { return Location; }

        public void scaleHitbox(float scaleF)
        {
            scaledSize = new SizeF (Size.Width*scaleF, Size.Height*scaleF);
            this.Hitbox = new RectangleF( Location, scaledSize );
        }

        public void resetScale()
        {
            scaledSize = Size;
            this.Hitbox = new RectangleF( Location, scaledSize );
        }
    }
}
