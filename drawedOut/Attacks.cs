namespace drawedOut
{
    internal class Attacks
    {
        public static List<Attacks> AttacksList = new List<Attacks>();
        public AnimationPlayer Animation { get; private init; }
        public Character Parent { get; private init; }

        private readonly int _atkDmg, _width, _height;
        private readonly float _xOffset, _yOffset;
        private int _despawnFrame { get; init; }
        private int _spawnFrame { get; init; }
        private AtkHitboxEntity? _atkHitbox;

        public AtkHitboxEntity? AtkHitbox
        {
            get
            {
                if (_atkHitbox is null)
                    throw new Exception("AtkHitbox should not be accessible when null");
                return _atkHitbox;
            }
            private set { _atkHitbox = value; }
        }


        internal class AtkHitboxEntity : Entity
        {
            public AtkHitboxEntity(PointF origin, int width, int height) 
            : base(origin: origin, width: width, height: height)
            { }

            public override void CheckActive() { }
        }


        /// <summary>
        /// Creates an attack object which can create attack hitboxes.
        /// </summary>
        /// <param name="Parent"> The Parent <see cref="Character"/> </param>
        /// <param name="xOffset"> The horizontal distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="yOffset"> The vertical distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="width"> The width of the AtkHitbox </param>
        /// <param name="height"> The height of the AtkHitbox </param>
        /// <param name="spawn"> The first frame at which a AtkHitbox should be created </param>
        /// <param name="despawn"> The frame at which a AtkHitbox should be removed </param>
        /// <param name="dmg"> 
        /// The damage of the attack.<br/>
        /// Default = 1
        /// </param>
        public Attacks(Character parent, int width, int height, AnimationPlayer animation,
                float xOffset = 0, float yOffset = 0, int spawn = 0, int despawn = -1, int dmg = 1)
        {
            Parent = parent;
            Animation = animation;
            _spawnFrame = spawn;
            _despawnFrame = (despawn == -1) ? Animation.LastFrame : despawn;
            _xOffset = xOffset * Global.BaseScale;
            _yOffset = yOffset * Global.BaseScale;
            _width = width;
            _height = height;
            if (dmg<=0) throw new ArgumentException("atk dmg should be bigger than 0");
            _atkDmg = dmg;
        }

        /// <summary>
        /// Destroys the AtkHitbox of the attack
        /// </summary>
        public void Dispose()
        {
            AttacksList.Remove(this);
            AtkHitbox = null;
        }


        /// <summary>
        /// Creates a AtkHitbox
        /// </summary>
        public void CreateHitbox(Global.XDirections direction = Global.XDirections.right)
        {
            if (_atkHitbox is not null) return;
            AtkHitbox = new AtkHitboxEntity(
                    origin: Parent.Location,
                    width: _width,
                    height: _height);
            AttacksList.Add(this);
        }


        /// <summary>
        /// Update the AtkHitbox' location
        /// </summary>
        public void UpdateHitboxCenter(float x, float y)
        {
            if (AtkHitbox is null) throw new Exception($"{this} is in AttackList but null");
            AtkHitbox.Center = new PointF(x, y);
        }


        public Bitmap NextAnimFrame(Global.XDirections facingDir = Global.XDirections.right)
        {
            if (Animation.CurFrame == _despawnFrame)
                Dispose();
            else if (Animation.CurFrame == _spawnFrame)
                CreateHitbox();

            return Animation.NextFrame();
        }



        /// <summary> Update all AtkHitbox positions </summary>
        /// <param name="dt"> delta time </param>
        public static void UpdateHitboxes()
        {
            if (AttacksList.Count == 0) return;
            foreach (Attacks atk in AttacksList)
            {
                PointF ParentCenter = atk.Parent.Center;

                float xOffset = atk._xOffset;
                if (atk.Parent.FacingDirection == Global.XDirections.left)
                    xOffset = -atk._xOffset;

                atk.UpdateHitboxCenter( 
                        x: ParentCenter.X + xOffset,
                        y: ParentCenter.Y - atk._yOffset);
            }
        }
    }
}
