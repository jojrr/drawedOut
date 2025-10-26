namespace drawedOut
{
    internal class Attacks
    {
        protected static List<Attacks> AttacksList = new List<Attacks>();
        private static List<Attacks> _diposedAttacks = new List<Attacks>();

        protected Character Parent { get; private set; }
        protected Entity? Hitbox 
        { 
            get 
            {
                if (Hitbox is null)
                    throw new Exception("Hitbox should not be accessible when null");
                return Hitbox;
            }
            private set { Hitbox = value; }
        }

        protected float XOffset { get; private set; }
        protected float YOffset { get; private set; }
        private int
            _width,
            _height;
        private readonly double _durationS;
        protected double DurationS;
        public int AtkDmg { get; private set; }

        /// <summary>
        /// Creates an attack object which can create attack hitboxes.
        /// </summary>
        /// <param name="parent"> The parent <see cref="Character"/> </param>
        /// <param name="xOffset"> The horizontal distance between the parent's centre and the hitbox's centre</param>
        /// <param name="yOffset"> The vertical distance between the parent's centre and the hitbox's centre</param>
        /// <param name="width"> The width of the hitbox </param>
        /// <param name="height"> The height of the hitbox </param>
        /// <param name="durationS"> The duration that the hitbox should last (in seconds)</param>
        /// <param name="dmg"> 
        /// The damage of the attack.<br/>
        /// Default = 1
        /// </param>
        public Attacks(Character parent, float xOffset, float yOffset, int width, int height, double durationS, int dmg=1)
        {
            Parent = parent;
            XOffset = xOffset;
            YOffset = yOffset;
            AtkDmg = dmg;
            _width = width;
            _height = height;
            _durationS = durationS;
        }

        /// <summary>
        /// Removes the hitbox of the attack
        /// </summary>
        public void Dispose() 
        {
            AttacksList.Remove(this);
            Hitbox = null;
        }


        /// <summary>
        /// Creates a hitbox
        /// </summary>
        public void CreateHitbox(Global.XDirections direction=Global.XDirections.right) 
        {
            Hitbox = new Entity(
                    origin: Parent.Location,
                    width: _width,
                    height: _height);
            AttacksList.Add(this);
        }

        /// <summary>
        /// Update the hitbox' location
        /// </summary>
        public void UpdateHitboxLocation( float x, float y )
        {
            if (Hitbox is null) throw new Exception($"{this} is in AttackList but null");
            Hitbox.Center = new PointF(x,y);
        }


        /// <summary> Update all hitbox positions </summary>
        /// <param name="dt"> delta time </param>
        public static void UpdateHitboxes(double dt)
        {
            foreach (Attacks atk in AttacksList)
            {
                if (atk.DurationS <= 0) 
                {
                    _diposedAttacks.Add(atk);
                    continue;
                }

                PointF parentCentre = atk.Parent.Center;
                float xOffset;

                if (atk.Parent.FacingDirection == Global.XDirections.left)
                    xOffset = -atk.XOffset;
                else
                    xOffset = atk.XOffset;

                atk.UpdateHitboxLocation( 
                        x: parentCentre.X + xOffset,
                        y: parentCentre.Y + atk.YOffset);

                atk.DurationS -= dt;
            }

            if (_diposedAttacks.Count == 0) return;

            foreach (Attacks atk in _diposedAttacks) { atk.Dispose(); }
            _diposedAttacks.Clear();
        }

    }
}



