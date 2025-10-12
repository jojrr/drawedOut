namespace drawedOut
{
    internal class Platform : Entity
    {
        /// <summary>
        /// 2D array: [level] [chunk]
        /// </summary>
        public static List<Platform> ActivePlatformList = new List<Platform>();
        public static List<Platform> InactivePlatformList = new List<Platform>();


        public Platform(Point origin, int width, int height, int LocatedLevel, int LocatedChunk)
            : base(origin, width, height, LocatedLevel, LocatedChunk)
        {
            InactivePlatformList.Add(this);
        } 

    }
}
