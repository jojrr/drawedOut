namespace drawedOut
{
    internal class Player : Character
    {
        public int AttackPower { get; private set; }
        public bool _doingAttack { get; private set; }

        //private _curAnimation=Animations.IdleAnimation;
        private Attacks
            _curAttack,
            _basic1,
            _basic2;
        private AnimationPlayer 
            _idleAnim,
            _runAnim,
            _basic1Anim,
            _basic2Anim,
            _curAnimation;

        private float _endLag = 0F;
        private int _energy;

        private Global.XDirections _curFacingDirection = Global.XDirections.left;
        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();

        public Player(Point origin, int width, int height, int attackPower, int energy)
            :base(origin: origin, width: width, height: height)
        {
            AttackPower = attackPower;
            _energy = energy;

            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);

            initBasic1();
            initBasic2();
        }

        private void initBasic1()
        {
            _basic1 = new Attacks(
                    parent: this,
                    xOffset: 50,
                    yOffset: 50,
                    width: 50,
                    height: 50,
                    durationS: 0.2);
        }

        private void initBasic2()
        {
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
            Hp -= dmg;
            hpBar.ComputeHP(Hp);
        }

        public void DoBasicAttack()
        {
            float hitboxXOffset = 50*Global.BaseScale;
            if (_curFacingDirection == Global.XDirections.right)
                hitboxXOffset = -hitboxXOffset;

            _doingAttack = true;
            // curAnimation = Animation.PlayerIdle;
        }

        public Image NextAnimation() 
        {
            if ((_curAnimation.CurFrame == _curAnimation.LastFrame) && _doingAttack)
                _curAnimation = _idleAnim;

            return _curAnimation.NextFrame();
        }
    }
}

