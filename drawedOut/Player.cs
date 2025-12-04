namespace drawedOut
{
    internal class Player : Character
    {
        
        public double XVelocity { get => xVelocity; }
        private readonly Attacks _basic1, _basic2;
        private static new double endlagS;
        private int _energy;

        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();

        static Player()
        {
            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);
        }

        protected static void TickEndlagS(double dt) { if (Player.endlagS > 0) Player.endlagS -= dt; }

        public Player(Point origin, int width, int height, int attackPower, int energy, int hp, 
                int xAccel=100, int maxXVelocity=600)
            :base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        {
            _energy = energy;
            IsActive = true;
            setIdleAnim(@"playerChar\idle\");
            setRunAnim(@"playerChar\run\");
            _basic1 = new Attacks(
                    parent: this,
                    width: 380,
                    height: 220,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 100,
                    spawn: 2,
                    despawn: 14);
        }

        public void UnlockMoves(){}

        public void DoDamage(int dmg, ref HpBarUI hpBar)
        {
            IsHit = true;
            Hp -= dmg;
            hpBar.ComputeHP(Hp);
        }

        ///<summary>
        ///reduces endlag by <paramref name="dt"/>
        ///</summary>
        ///<param name="dt"> delta time </param>
        public static void TickEndlag(double dt) => endlagS -= dt;

        public void DoBasicAttack()
        {
            if (Player.endlagS > 0) return;
            curAttack = _basic1;
            Player.endlagS = 1;
        }

        int count = 0;
        public override Bitmap NextAnimFrame()
        {
            if (runAnim is null || idleAnim is null) throw new Exception("Player runAnim or idle null");
            if (curAttack is null)
            {
                if (yVelocity == 0)
                {
                    if (curXAccel == 0) return idleAnim.NextFrame(FacingDirection);
                    return runAnim.NextFrame(FacingDirection);
                }
                /*
                else if (yVelocity > 0)
                {
                    return fallAnim.NextFrame(FacingDirection);
                }
                else 
                {
                    return jumpAnim.NextFrame(FacingDirection);
                }
                */
                return idleAnim.NextFrame(FacingDirection);
            }

            if (curAttack.Animation.CurFrame == curAttack.Animation.LastFrame)
            {
                Bitmap atkAnim = curAttack.NextAnimFrame(FacingDirection);
                curAttack = null;
                return atkAnim;
            }

            return curAttack.NextAnimFrame(FacingDirection);
        }

        public void HealPlayer(int heal) => Hp += heal; 


        /// <summary>
        /// Check if the level should scroll
        /// </summary>
        /// <param name="baseBox"> The base rectangle that defines the bounds of the level. </param>
        public bool CheckScrolling(Platform baseBox)
        {
            Global.XDirections? onWorldBoundary = null;
            Global.XDirections? scrollDirection = null;

            if (!ShouldDoMove())  return false;
            if (Global.LeftScrollBound<=Center.X && Center.X<=Global.RightScrollBound) return false;

            if (0 < baseBox.Hitbox.Left) onWorldBoundary = Global.XDirections.left; 
            else if (Global.LevelSize.Width > baseBox.Hitbox.Right) onWorldBoundary = Global.XDirections.right;

            if (Center.X < Global.LeftScrollBound && xVelocity < 0)
                scrollDirection = Global.XDirections.left; 
            else if (Center.X > Global.RightScrollBound && xVelocity > 0)
                scrollDirection = Global.XDirections.right;

            if (onWorldBoundary == scrollDirection) return false;
            if (scrollDirection is null) return false;
            return true;
        }

        public override void CheckActive() { }
    }
}

