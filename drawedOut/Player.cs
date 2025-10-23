namespace drawedOut
{
    internal class Player : Character
    {
        public int AttackPower { get; private set; }
        public bool DoingAttack { get; private set; }

        //private _curAnimation=Animations.IdleAnimation;
        private int _energy;
        private Global.XDirections curFacingDirection = Global.XDirections.Left;
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

        // public UnlockMoves() {}

        public void DoDamage(int dmg, ref HpBarUI hpBar)
        {
            Hp -= dmg;
            HpBarUI.ComputeHP(Hp);
        }

        public void DoBasicAttack()
        {
            float hitboxXOffset = 50*Global.BaseScale;
            if (curFacingDirection == Global.XDirections.right)
                hitboxXOffset = -hitboxXOffset;

            DoingAttack = true;
            Attack basicAttackBox = new Attack(
                    origin: LocationX + hitboxXOffset,
                    width: 20,
                    height: 20);

            // curAnimation = Animation.PlayerIdle;
        }

        //public Image CycleAnimation() => curAnimation.NextFrame();
        //
    }
}

