namespace drawedOut
{
    internal class Platform : Entity
    {
        public static IReadOnlyCollection<Platform> ActivePlatformList => _activePlatformList;
        public static IReadOnlyCollection<Platform> InactivePlatformList => _inactivePlatformList;
        public static Bitmap PlatformSprite { get; private set; }

        private static HashSet<Platform> _activePlatformList = new HashSet<Platform>();
        private static HashSet<Platform> _inactivePlatformList = new HashSet<Platform>();
        private static string _spritePath = @"sprites/platforms/platformSprite/platformSprite.png";

        private PointF _originalLocation;
        private bool _toggleable;

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
            _inactivePlatformList.Add(this);
            _originalLocation = origin;
            _toggleable=false;
        } 

        public Platform(Point origin, int width, int height, bool toggleable, bool defaultState=false)
            : base(origin, width, height)
        {
            _toggleable=toggleable;
            _originalLocation = origin;
            IsActive = defaultState;
            if (defaultState) _activePlatformList.Add(this);
            else _inactivePlatformList.Add(this);
        }

        public void Activate() 
        {
            if (!_toggleable) throw new Exception("untoggleable platform tried to be toggled");
            if (IsActive) return;
            IsActive = true;
            _inactivePlatformList.Remove(this);
            _activePlatformList.Add(this);
        }

        public void Deactivate()
        {
            if (!_toggleable) throw new Exception("untoggleable platform tried to be toggled");
            if (!IsActive) return;
            IsActive = false;
            _activePlatformList.Remove(this);
            _inactivePlatformList.Add(this);
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

        public new static void ClearAllLists()
        {
            _activePlatformList.Clear();
            _inactivePlatformList.Clear();
        }

        public override void CheckActive()
        {
            if (_toggleable) return;
            if (this.DistToMid > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;

                IsActive = false;
                _inactivePlatformList.Add(this);
                _activePlatformList.Remove(this);
                return;
            }

            if (IsActive) return;
                
            IsActive = true;
            _activePlatformList.Add(this);
            _inactivePlatformList.Remove(this);
        }
    }
}
