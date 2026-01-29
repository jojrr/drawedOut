namespace drawedOut
{
    public class Platform : Entity
    {
        public static HashSet<Platform> ActivePlatformList = new HashSet<Platform>();
        public static HashSet<Platform> InactivePlatformList = new HashSet<Platform>();

        private PointF _originalLocation;
        private bool _toggleable;

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

        public Platform(Point origin, int width, int height)
            : base(origin, width, height)
        {
            InactivePlatformList.Add(this);
            _originalLocation = origin;
            _toggleable=false;
        } 

        public Platform(Point origin, int width, int height, bool toggleable, bool defaultState=false)
            : base(origin, width, height)
        {
            _toggleable=toggleable;
            _originalLocation = origin;
            IsActive = defaultState;
            if (defaultState) ActivePlatformList.Add(this);
            else InactivePlatformList.Add(this);
        }

        public void Activate() 
        {
            if (!_toggleable) throw new Exception("untoggleable platform tried to be toggled");
            if (IsActive) return;
            IsActive = true;
            InactivePlatformList.Remove(this);
            ActivePlatformList.Add(this);
        }

        public void Deactivate()
        {
            if (!_toggleable) throw new Exception("untoggleable platform tried to be toggled");
            if (!IsActive) return;
            IsActive = false;
            ActivePlatformList.Remove(this);
            InactivePlatformList.Add(this);
        }

        public static void DrawAll(Graphics g)
        {
            foreach (Platform plat in Platform.ActivePlatformList)
            {
                RectangleF hitbox = plat.Hitbox;
                using (Pen blackPen = new Pen(Color.Black, 6))
                { g.DrawRectangle(blackPen, hitbox); }
            }
        }

        public override void CheckActive()
        {
            if (_toggleable) return;
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
