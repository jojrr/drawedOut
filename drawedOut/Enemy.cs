namespace drawedOut
{
    internal class Enemy : Character
    {
        public static List<Enemy> ActiveEnemyList = new List<Enemy>();
        public static List<Enemy> InactiveEnemyList = new List<Enemy>();

        public Enemy(Point origin, int width, int height, int hp, int xAccel=100, int maxXVelocity=600)
            : base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        { InactiveEnemyList.Add(this); }

        public virtual void DoMovement(double dt, double scrollVelocity, PointF playerCenter) => 
            throw new Exception($"DoMove is not implemented in {this.GetType()}");

        public override void CheckActive()
        {
            if (Hp <= 0) 
            {
                IsActive = false;
                ActiveEnemyList.Remove(this);
                InactiveEnemyList.Remove(this);
                if (this.curAttack is not null) this.curAttack.Dispose();
                this.Delete();
                return;
            }

            if (DistToMid > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;

                IsActive = false;
                InactiveEnemyList.Add(this);
                ActiveEnemyList.Remove(this);
                return;
            }

            if (IsActive) return;
                
            IsActive = true;
            ActiveEnemyList.Add(this);
            InactiveEnemyList.Remove(this);
        }
    }


    internal class MeleeEnemy : Enemy
    {
        private const int 
            ATK_ENDLAG_S = 1,
            MOV_ENDLAG_S = 3,
            ATK_X_OFFSET = 100,
            MAX_MOVEMENT_TIME_S = 3;
        private readonly double 
            _maxRange,
            _jumpRange;
        private readonly Attacks _mainAtk;
        private double _movementTimer;

        public MeleeEnemy(Point origin)
            : base(origin: origin, width: 80, height: 160, hp: 3, maxXVelocity: 400)
        {
            Size atkSize = new Size(180,220);

            _maxRange = Width+(atkSize.Width * Global.BaseScale);
            _jumpRange = 1.5*Height;

            _mainAtk = new Attacks(
                    parent: this,
                    size: atkSize,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: ATK_X_OFFSET,
                    spawn: 7,
                    despawn: 11);
            setRunAnim(@"fillerAnim\");
            setIdleAnim(@"fillerPic\");
        }

        public override void DoMovement(double dt, double scrollVelocity, PointF playerCenter)
        {
            float xDistance = (Center.X - playerCenter.X);
            float yDistance = (Center.Y - playerCenter.Y);
            Global.XDirections direction = (xDistance > 0) ? Global.XDirections.left : Global.XDirections.right;

            if (endlagS > 0) 
            {
                MoveCharacter(dt, null, scrollVelocity);
                FacingDirection = direction;
                return;
            }

            if ( yDistance > _jumpRange ) DoJump();

            if ( Math.Abs(xDistance) > _maxRange )
            {
                MoveCharacter(dt, direction, scrollVelocity);
                _movementTimer += dt;
            }
            else { DoBasicAttack(); }

            if (_movementTimer >= MAX_MOVEMENT_TIME_S)
            {
                endlagS = MOV_ENDLAG_S;
                _movementTimer = 0;
            }
        }

        private void DoBasicAttack()
        {
            curAttack = _mainAtk;
            endlagS = ATK_ENDLAG_S;
        }

    }
}
