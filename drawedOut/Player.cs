namespace drawedOut
{
    internal class Player : Character
    {
        public int AttackPower { get; private set; }
        public bool _doingAttack { get; private set; }

        //private _curAnimation=Animations.IdleAnimation;
        private int _energy;
        private Attacks _curAttack;
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
        }

        private class basicAttack : Attacks
        {
            private const int 
                _xOffset = 50,
                _yOffset = 0,
                _width = 20,
                _height = 20,
                _atkFrame1 = 6,
                _atkFrame2 = 12;


            public basicAttack(PointF curCenter) 
                : base(parentOrigin: curCenter, xOffset: _xOffset, yOffset: _yOffset, width: _width, height: _height)
            { }

            public 
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
            if (curFacingDirection == Global.XDirections.right)
                hitboxXOffset = -hitboxXOffset;

            _doingAttack = true;
            _curAttack = new Attacks(

            // curAnimation = Animation.PlayerIdle;
        }

        public Image CycleAnimation() 
        {
            if (_doingAttack)
            {
                switch (_curAttack.GetType)
                {
                    case 
                if (curAnimation.CurFrame == curAnimation.Length-1)
                {
                    _doingAttack = false;
                    _curAttack.Dispose();
                }
            }
            // return curAnimation.NextFrame();
        }
    }
}

