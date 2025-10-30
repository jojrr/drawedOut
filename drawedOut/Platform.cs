namespace drawedOut
{
    internal class Platform : Entity
    {
        /// <summary>
        /// 2D array: [level] [chunk]
        /// </summary>
        public static List<Platform> ActivePlatformList = new List<Platform>();
        public static List<Platform> InactivePlatformList = new List<Platform>();
        bool _mainPlat = false;

        public Platform(Point origin, int width, int height, bool isMainPlat=false)
            : base(origin, width, height)
        {
            InactivePlatformList.Add(this);
            if (isMainPlat) 
            {
                _mainPlat = true;
                ActivePlatformList.Add(this);
                IsActive = true;
            }
        } 

        public override void CheckActive()
        {
            if (_mainPlat) return;
            if (this.DistToMid > Global.EntityLoadThreshold)
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
