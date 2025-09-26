using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace drawedOut 
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {
        public static List<Entity> EntityList = new List<Entity>();

        public bool IsPlayer { get; } 
        public bool IsMainPlat { get; } 
        public SizeF Size { get { return size; } }  

        protected PointF Location;
        protected PointF Center;
        protected SizeF 
            size,
            scaledSize;
        protected int
            Width = 0,
            Height = 0;
        protected RectangleF Hitbox;

        protected const int TotalLevels = 1;
        protected static readonly int[] ChunksInLvl = { 3 };

        /// <summary>
        /// returns the hitbox as a rectangle
        /// </summary>
        /// <returns>hitbox of type rectangle</returns>
        public RectangleF getHitbox() { return Hitbox; }

        

        /// <summary>
        /// Creates a hitbox at specified paramters
        /// </summary>
        /// <param name="origin">the point of the top-left of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        /// <param name="isPlayer">Is this entity the player </param>
        public Entity(PointF origin, int width, int height, bool isPlayer = false, bool isMainPlat = false)
        {
            Location = origin;
            Width = width;
            Height = height;
            size = new Size( width, height);
            scaledSize = Size;
            Hitbox = new RectangleF(origin, Size); 
            Center = new PointF (Hitbox.X + Width/2, Hitbox.Y + Height/2);
            IsPlayer = isPlayer;
            IsMainPlat = isMainPlat;

            EntityList.Add(this);
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
            scaledSize = new SizeF (this.Size.Width*scaleF, this.Size.Height*scaleF);
            this.Hitbox = new RectangleF( Location, scaledSize );
        }

        public void resetScale()
        {
            scaledSize = Size;
            this.Hitbox = new RectangleF( Location, scaledSize );
        }

        public int GetChunksInLvl(int level) { return ChunksInLvl[level]; }
    }
}
