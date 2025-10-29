namespace drawedOut
{
    internal class Player : Character
    {
        public int AttackPower { get; private set; }
        public double XVelocity { get => xVelocity; }
        public bool IsHit;

        //private _curAnimation=Animations.IdleAnimation;
        private Attacks? _curAttack;
        private Attacks
            _basic1,
            _basic2;
        //private AnimationPlayer 
        //    _idleAnim,
        //    _runAnim,
        //    _basic1Anim,
        //    _basic2Anim,
        //    _curAnimation;

        private int _energy;

        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();
        private static Dictionary<Attacks, int> _atkSpawnFrames = new Dictionary<Attacks, int>();

        public Player(Point origin, int width, int height, int attackPower, double accel, int energy, int maxHp)
            :base(origin: origin, width: width, height: height, hp: maxHp, xAccel: accel)
        {
            AttackPower = attackPower;
            _energy = energy;

            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);

            IsActive = true;

            initBasics();
            initAtkSpawnFrames();
        }

        private void initAtkSpawnFrames()
        {
            _atkSpawnFrames.Add(_basic1, 6);
            _atkSpawnFrames.Add(_basic2, 7);
        }

        private void initBasics()
        {
            _basic1 = new Attacks(
                    parent: this,
                    xOffset: 50,
                    yOffset: 50,
                    width: 50,
                    height: 50,
                    durationS: 0.2);
            _basic2 = new Attacks(
                    parent: this,
                    xOffset: 50,
                    yOffset: 50,
                    width: 50,
                    height: 50,
                    durationS: 0.4);
        }

        // public UnlockMoves() {}

        public void DoDamage(int dmg, ref HpBarUI hpBar)
        {
            IsHit = true;
            Hp -= dmg;
            hpBar.ComputeHP(Hp);
        }

        public void DoBasicAttack()
        {
            _curAttack = _basic1;
            // curAnimation = Animation.PlayerIdle;
        }

        // public Image NextAnimation() 
        // {
        //     if (_curAttack is not null)
        //     {
        //         if (_curAnimation.CurFrame == _curAnimation.LastFrame)
        //             _curAnimation = _idleAnim;
        //         if (_curAnimation.CurFrame == _atkSpawnFrames[_curAttack])
        //             _curAttack.CreateHitbox();
        //     }

        //     return _curAnimation.NextFrame(FacingDirection);
        // }

        public void HealPlayer(int heal)
        {
            Hp += heal;
            if (Hp > MaxHp) Hp = MaxHp;
        }


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

