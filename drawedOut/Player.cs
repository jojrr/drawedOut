namespace drawedOut
{
    internal class Player : Character
    {
        public double XVelocity { get => xVelocity; }
        public bool IsHit;

        private Attacks? _curAttack;
        private Attacks
            _basic1,
            _basic2;
        private AnimationPlayer 
            _idleAnim;
        //    _runAnim,
        //    _basic2Anim,
        //    _curAnimation;

        private int _energy;

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

            initAnimations();
            initBasics();
        }


        private void initAnimations()
        {
            _idleAnim = new AnimationPlayer(@"playerChar\idle\");
        }


        private void initBasics()
        {
            AnimationPlayer _basic1Anim = new AnimationPlayer(@"fillerAnim\");
            _basic1 = new Attacks(
                    parent: this,
                    xOffset: 50,
                    yOffset: 50,
                    width: 50,
                    height: 50,
                    spawn: 4,
                    despawn: 14,
                    animation: _basic1Anim);
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
                /*
                if (yVelocity == 0)
                {
                    if (xVelocity == 0)
                      return _idleAnim.NextFrame(FacingDirection);
                    else 
                      return _runAnim.NextFrame(FacingDirection);
                }
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

            if (_curAttack.animation.CurFrame == _curAttack.animation.LastFrame)
            {
                _curAttack = null;
                return NextAnimFrame();
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

            //if (!ShouldDoMove())  return false;
            //if (Global.LeftScrollBound<=Center.X && Center.X<=Global.RightScrollBound) return false;

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

