namespace drawedOut
{
    internal class Attacks
    {
        public static HashSet<Attacks> AttacksList = new HashSet<Attacks>();
        public AnimationPlayer Animation { get; private init; }
        public bool IsActive { get => (_atkHitbox is not null); }
        public bool IsLethal { get; private init; }
        public int AtkDmg { get; private init; }

        private static HashSet<Attacks> _disposedAttacks = new HashSet<Attacks>();
        private readonly float _xOffset, _yOffset, _endlagS;
        private readonly int _width, _height;
        private AtkHitboxEntity? _atkHitbox;
        private Character _parent;
        private int _despawnFrame { get; init; }
        private int _spawnFrame { get; init; }

        public AtkHitboxEntity? AtkHitbox
        {
            get
            {
                if (_atkHitbox is null) throw new Exception("AtkHitbox should not be accessible when null");
                return _atkHitbox;
            }
            private set { _atkHitbox = value; }
        }

        /// <summary>
        /// The <see cref="Character"/> which this attack belongs to.
        /// </summary>
        public Character Parent { // support for static attacks in classes
            get 
            {
                if (_parent is null) throw new NullReferenceException(
                        "Parent is accessed when null"
                        ); 
                return _parent;
            }
            set
            {
                if (_parent is not null) throw new NullReferenceException(
                        "Parent attempted to be overwritten when already having value"
                        ); 
                _parent = value; _parent = value; 
            }
        }


        internal class AtkHitboxEntity : Entity
        {
            internal AtkHitboxEntity(PointF origin, int width, int height) 
            : base(origin: origin, width: width, height: height)
            { }

            public override void CheckActive() { }
        }


        /// <summary>
        /// Creates an attack object which can create attack hitboxes.
        /// </summary>
        /// <param name="parent"> The Parent <see cref="Character"/> </param>
        /// <param name="width"> The width of the AtkHitbox </param>
        /// <param name="height"> The height of the AtkHitbox </param>
        /// <param name="xOffset"> The horizontal distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="yOffset"> The vertical distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="spawn"> 
        /// The first frame at which a AtkHitbox should be created <br/>
        /// (Default: first frame)
        /// </param>
        /// <param name="despawn"> 
        /// The frame at which a AtkHitbox should be removed. <br/>
        /// (Default: last frame)
        /// </param>
        /// <param name="dmg"> 
        /// The damage of the attack.<br/>
        /// Default = 1
        /// </param>
        public Attacks(Character? parent, int width, int height, AnimationPlayer animation, float endlag,
                float xOffset=0, float yOffset=0, int spawn=0, int despawn=-1, int dmg=1, bool isLethal=false)
        {
            if (dmg<=0) throw new ArgumentException("atk dmg should be bigger than 0");
            if (_endlagS < 0) throw new ArgumentException( "Endlag cannot be smaller than 0" ); 
            if (spawn > animation.LastFrame || spawn < 0) 
            {
                throw new ArgumentException(
                        "Spawn frame cannot be bigger than the animationLength or <0"
                        );
            }
            if (despawn > animation.LastFrame || despawn < -1) 
            {
                throw new ArgumentException(
                        "Despawn frame cannot be bigger than the animationLength or <-1"
                        );
            }

            if (parent is not null) _parent = parent;


            AtkDmg = dmg;
            _width = width;
            _height = height;
            _endlagS = endlag;
            IsLethal = isLethal;
            Animation = animation;
            _spawnFrame = spawn;
            _despawnFrame = (despawn == -1) ? animation.LastFrame : despawn;
            _xOffset = xOffset * Global.BaseScale;
            _yOffset = yOffset * Global.BaseScale;
        }

        /// <summary>
        /// Creates an attack object which can create attack hitboxes.
        /// </summary>
        /// <param name="parent"> The Parent <see cref="Character"/> </param>
        /// <param name="size"> The <see cref="Size" of the AtkHitbox </param>
        //u <param name="xOffset"> The horizontal distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="yOffset"> The vertical distance between the Parent's centre and the AtkHitbox's centre</param>
        /// <param name="spawn"> 
        /// The first frame at which a AtkHitbox should be created <br/>
        /// (Default: first frame)
        /// </param>
        /// <param name="despawn"> 
        /// The frame at which a AtkHitbox should be removed. <br/>
        /// (Default: last frame)
        /// </param>
        /// <param name="dmg"> 
        /// The damage of the attack.<br/>
        /// Default = 1
        /// </param>
        public Attacks(Character? parent, Size size, AnimationPlayer animation, float endlag,
                float xOffset=0, float yOffset=0, int spawn=0, int despawn=-1, int dmg=1, bool isLethal=false)
        {
            if (dmg<=0) throw new ArgumentException("atk dmg should be bigger than 0");
            if (_endlagS < 0) throw new ArgumentException( "Endlag cannot be smaller than 0" ); 
            if (spawn > animation.LastFrame || spawn < 0) 
            {
                throw new ArgumentException(
                        "Spawn frame cannot be bigger than the animationLength or <0"
                        );
            }
            if (despawn > animation.LastFrame || despawn < -1) 
            {
                throw new ArgumentException(
                        "Despawn frame cannot be bigger than the animationLength or <-1"
                        );
            }

            if (parent is not null) _parent = parent;

            AtkDmg = dmg;
            _width = size.Width;
            _height = size.Height;
            _endlagS = endlag;
            IsLethal = isLethal;
            Animation = animation;
            _spawnFrame = spawn;
            _despawnFrame = (despawn == -1) ? animation.LastFrame : despawn;
            _xOffset = xOffset * Global.BaseScale;
            _yOffset = yOffset * Global.BaseScale;
        }

        /// <summary>
        /// Destroys the AtkHitbox of the attack
        /// </summary>
        public void Dispose() 
        {
            if (_atkHitbox is null) return;
            _disposedAttacks.Add(this);
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
            {
                Dispose();
                Parent.ApplyEndlag(_endlagS);
            }
            else if (Animation.CurFrame == _spawnFrame) CreateHitbox();
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

            if (_disposedAttacks.Count == 0) return;

            foreach (Attacks atk in _disposedAttacks) 
            {
                AttacksList.Remove(atk);
                atk.AtkHitbox.Delete();
                atk.AtkHitbox = null;
            }

            _disposedAttacks.Clear();
        }
    }
}
