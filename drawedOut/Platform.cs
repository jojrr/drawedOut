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
            : base(origin, width, height)
        {
            InactivePlatformList.Add(this);
        } 

        public override void CheckActive()
        {
            if (Math.Abs(this.DistToMid) > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;

                IsActive = false;
                InactivePlatformList.Add(this);
                ActivePlatformList.Remove(this);
            }

            if (IsActive) return;
                
            IsActive = true;
            ActivePlatformList.Add(this);
            InactivePlatformList.Remove(this);
        }
    }
}
