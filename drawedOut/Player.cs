namespace drawedOut
{
    internal class Player : Character
    {
        public int AttackPower { get; private set; }
        public double XVelocity { get => xVelocity; }
        
        public bool IsHit;
        private bool _doingAttack;

        //private _curAnimation=Animations.IdleAnimation;
        private Attacks? _curAttack;
        private Attacks
            _basic1,
            _basic2;
        private AnimationPlayer 
            _idleAnim,
            _runAnim,
            _basic1Anim,
            _basic2Anim,
            _curAnimation;

        private int _energy;

        private Global.XDirections _facingDirection = Global.XDirections.left;
        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();
        private static Dictionary<Attacks, int> _atkSpawnFrames = new Dictionary<Attacks, int>();

        public Player(Point origin, int width, int height, int attackPower, int energy, int maxHp)
            :base(origin: origin, width: width, height: height, hp: maxHp)
        {
            AttackPower = attackPower;
            _energy = energy;

            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);

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

        public Image NextAnimation() 
        {
            if (_doingAttack)
            {
                if (_curAnimation.CurFrame == _curAnimation.LastFrame)
                    _curAnimation = _idleAnim;
                if (_curAnimation.CurFrame == _atkSpawnFrames[_curAttack])
                    _curAttack.CreateHitbox();
            }

            return _curAnimation.NextFrame(_facingDirection);
        }

        if (currentHp > maxHp) currentHp = maxHp;
    }
}

