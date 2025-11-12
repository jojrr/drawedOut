namespace drawedOut
{
    internal class Platform : Entity
    {
        /// <summary>
        /// 2D array: [level] [chunk]
        /// </summary>
        public static List<Platform> ActivePlatformList = new List<Platform>();
        public static List<Platform> InactivePlatformList = new List<Platform>();
        public bool IsMainPlat { get; init; }

        public static Bitmap PlatformSprite { get; private set; }
        private static string _spritePath = @"sprites/platforms/platformSprite/platformSprite.png";
        private static void _setPlatformSprite()
        {
            string directory = Path.Combine(Global.GetProjFolder(), _spritePath);
            PlatformSprite = new Bitmap(
                    Image.FromFile(directory),
                    (int)(128*Global.BaseScale),
                    (int)(128*Global.BaseScale)
                    );
        }

        static Platform() => _setPlatformSprite();

        public Platform(Point origin, int width, int height, bool isMainPlat=false)
            : base(origin, width, height)
        {
            InactivePlatformList.Add(this);
            if (isMainPlat) 
            {
                IsMainPlat = true;
                ActivePlatformList.Add(this);
                IsActive = true;
            }
        } 

        public override void CheckActive()
        {
            if (IsMainPlat) return;
            if (this.DistToMid > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;

                IsActive = false;
                InactivePlatformList.Add(this);
                ActivePlatformList.Remove(this);
                return;
            }

            if (IsActive) return;
                
            IsActive = true;
            ActivePlatformList.Add(this);
            InactivePlatformList.Remove(this);
        }
    }
}
