namespace drawedOut
{
    internal class Player : Character
    {
        
        private readonly Attacks
            _basic1,
            _basic2;

        private int _energy;
        public double XVelocity { get => xVelocity; }

        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();

        static Player()
        {
            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);
        }

        public Player(Point origin, int width, int height, int attackPower, int energy, int maxHp)
            :base(origin: origin, width: width, height: height, hp: maxHp)
        {
            _energy = energy;
            IsActive = true;
            setIdleAnim(@"playerChar\idle\");
            setRunAnim(@"playerChar\run\");
            _basic1 = new Attacks(
                    parent: this,
                    width: 180,
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

        public void DoBasicAttack()
        {
            //if (endlagS <= 0) 
                _curAttack = _basic1;
            //endlagS = 0.3;
        }

        int count = 0;
        public override Bitmap NextAnimFrame()
        {
            if (_curAttack is null)
            {
                if (yVelocity == 0)
                {
                    if (curXAccel == 0) return _idleAnim.NextFrame(FacingDirection);
                    return _runAnim.NextFrame(FacingDirection);
                }
                /*
                else if (yVelocity > 0)
                {
                    return _fallAnim.NextFrame(FacingDirection);
                }
                else 
                {
                    return _jumpAnim.NextFrame(FacingDirection);
                }
                */
                return _idleAnim.NextFrame(FacingDirection);
            }

            if (_curAttack.Animation.CurFrame == _curAttack.Animation.LastFrame)
            {
                Bitmap atkAnim = _curAttack.NextAnimFrame(FacingDirection);
                _curAttack = null;
                return atkAnim;
            }

            return _curAttack.NextAnimFrame(FacingDirection);
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

