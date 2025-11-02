namespace drawedOut
{
    internal class Attacks
    {
        public static List<Attacks> AttacksList = new List<Attacks>();
        private static Queue<Attacks> _diposedAttacks = new Queue<Attacks>();

        public Character parent { get; init; }
        public AnimationPlayer animation { get; init; }

        private AtkHitboxEntity? _atkHitbox;
        public AtkHitboxEntity? AtkHitbox 
        { 
            get 
            {
                if (_atkHitbox is null) throw new Exception("AtkHitbox should not be accessible when null");
                return _atkHitbox;
            }
            private set { _atkHitbox = value; }
        }

        internal class AtkHitboxEntity : Entity
        {
            public AtkHitboxEntity(PointF origin, int width, int height)
            : base(origin: origin, width: width, height: height)
            { }

            public override void CheckActive() {}
        }


        private readonly int _atkDmg;
        private float _xOffset, _yOffset;
        private int _width, _height;

        private int spawnFrame { get; init;}
        private int despawnFrame { get; init;}

        /// <summary>
        /// Creates an attack object which can create attack hitboxes.
        /// </summary>
        /// <param name="parent"> The parent <see cref="Character"/> </param>
        /// <param name="xOffset"> The horizontal distance between the parent's centre and the AtkHitbox's centre</param>
        /// <param name="yOffset"> The vertical distance between the parent's centre and the AtkHitbox's centre</param>
        /// <param name="width"> The width of the AtkHitbox </param>
        /// <param name="height"> The height of the AtkHitbox </param>
        /// <oaram name="spawn"> The first frame at which a AtkHitbox should be created </param>
        /// <oaram name="despawn"> The frame at which a AtkHitbox should be removed </param>
        /// <param name="dmg"> 
        /// The damage of the attack.<br/>
        /// Default = 1
        /// </param>
        public Attacks(Character parent, float xOffset, float yOffset, int width, int height, 
                int spawn, int despawn, AnimationPlayer animation, int dmg=1) 
        {
            this.parent = parent;
            this.animation = animation;
            spawnFrame = spawn;
            despawnFrame = despawn;
            _atkDmg = dmg;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _width = width;
        }

        /// <summary>
        /// Removes the AtkHitbox of the attack
        /// </summary>
        public void Dispose() 
        {
            AttacksList.Remove(this);
            AtkHitbox = null;
        }


        /// <summary>
        /// Creates a AtkHitbox
        /// </summary>
        public void CreateHitbox(Global.XDirections direction=Global.XDirections.right) 
        {
            if (_atkHitbox is not null) return;
            AtkHitbox = new AtkHitboxEntity(
                    origin: parent.Location,
                    width: _width,
                    height: _height);
            AttacksList.Add(this);
        }


        /// <summary>
        /// Update the AtkHitbox' location
        /// </summary>
        public void UpdateHitboxCenter( float x, float y )
        {
            if (AtkHitbox is null) throw new Exception($"{this} is in AttackList but null");
            AtkHitbox.Center = new PointF(x,y);
        }


        public Bitmap NextAnimFrame(Global.XDirections facingDir = Global.XDirections.right)
        {
            if (animation.CurFrame == despawnFrame)
                Dispose();
            else if (animation.CurFrame == spawnFrame)
                CreateHitbox();

            return animation.NextFrame();
        }



        /// <summary> Update all AtkHitbox positions </summary>
        /// <param name="dt"> delta time </param>
        public static void UpdateHitboxes()
        {
            foreach (Attacks atk in AttacksList)
            {
                PointF parentCentre = atk.parent.Center;

                float xOffset = atk._xOffset;
                if (atk.parent.FacingDirection == Global.XDirections.left)
                    xOffset = -atk._xOffset;

                atk.UpdateHitboxCenter( 
                        x: parentCentre.X + xOffset,
                        y: parentCentre.Y + atk._yOffset);
            }
        }
    }
}



