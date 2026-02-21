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

        public RectangleF? GetDrawingBox(float levelW, float levelH, int buffer)
        {
            SizeF recSize = Size;
            levelW += buffer;
            levelH += buffer;

            float 
                top = (int)LocationY,
                bottom = (int)Hitbox.Bottom,
                left = (int)LocationX,
                right = (int)Hitbox.Right;

            if (left > levelW) return null;
            if (top > levelH) return null;
            if (right < -buffer) return null;
            if (bottom < -buffer) return null;

            if (bottom > levelH) recSize.Height -= bottom - levelH;
            if (right > levelW) recSize.Width -= right - levelW;

            if (top < -buffer) 
            {
                recSize.Height -= -top - buffer;
                top = -buffer;
            }
            if (left < -buffer)
            {
                recSize.Width -= -left - buffer;
                left = -buffer;
            }

            return new RectangleF(left, top, recSize.Width, recSize.Height);
        }

        public static void DrawAll(Graphics g)
        {
            float levelWidth = Global.LevelSize.Width;
            float levelHeight = Global.LevelSize.Height;
            int lineSize = (int)(6*Global.BaseScale);

            foreach (Platform plat in Platform.ActivePlatformList)
            {
                RectangleF? hitbox = plat.GetDrawingBox(levelWidth, levelHeight, lineSize);
                using (Pen blackPen = new Pen(Color.Black, lineSize))
                { 
                    if (hitbox is null) continue;
                    g.DrawRectangle(blackPen, hitbox.Value); 
                }
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
            if (this.DistToMid > Global.EntityLoadThreshold*2)
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
