namespace drawedOut
{
    internal abstract class Enemy : Character
    {
        public static HashSet<Enemy> ActiveEnemyList = new HashSet<Enemy>();
        public static HashSet<Enemy> InactiveEnemyList = new HashSet<Enemy>();

        protected bool isDowned = false;

        private const float _DEFAULT_DOWN_TIME_S = 5;
        private readonly float _maxDownTime;
        private double _downTimer = 0;

        public Enemy(Point origin, int width, int height, int hp,
                int xAccel=100, int maxXVelocity=600, float maxDownTime=_DEFAULT_DOWN_TIME_S)
            : base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        { 
            _maxDownTime = maxDownTime;
            InactiveEnemyList.Add(this); 
        }

        public virtual void DoMovement(double dt, double scrollVelocity, PointF playerCenter) => 
            throw new Exception($"DoMove is not implemented in {this.GetType()}");

        public void DoDamage(int dmg, Entity source, bool isLethal)
        {
            DoDamage(dmg, source);
            CheckDowned(isLethal);
        }

        public void DoDamage(Projectile sourceProjectile, bool isLethal)
        {
            DoDamage(sourceProjectile);
            CheckDowned(isLethal);
        }

        private void CheckDowned(bool isLethal)
        {
            if (Hp <= 0)
            {
                isDowned = true;
                if (curAttack is not null)
                {
                    curAttack.Dispose();
                    curAttack = null;
                }
            }
            if (isDowned && isLethal) isDowned = false;
        }

        private void IncDownTimer(double dt) 
        {
            if (!isDowned) return;
            if (_downTimer >= _maxDownTime)
            {
                iFrames = 0;
                _downTimer = 0;
                isDowned = false;
                Hp = (int)Math.Ceiling((double)MaxHp/2);
                return;
            }
            _downTimer += dt;
            iFrames = 67;
        }

        private void doDeath()
        {
            IsActive = false;
            ActiveEnemyList.Remove(this);
            InactiveEnemyList.Remove(this);
            if (this.curAttack is not null) this.curAttack.Dispose();
            this.Delete();
        }

        public override void Reset()
        {
            base.Reset();
            _downTimer = 0;
            curAttack = null;
            isDowned=false;
        }

        public static void TickCounters(double dt)
        {
            foreach (Enemy e in Enemy.ActiveEnemyList) 
            { 
                e.TickAllCounters(dt); 
                e.IncDownTimer(dt);
            }
        }

        public override void CheckActive()
        {
            if (Hp <= 0 && !isDowned) 
            { 
                doDeath(); 
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
        private static Bitmap _downedSprite;
        private readonly Attacks _mainAtk;
        private double _movementTimer;

        static MeleeEnemy()
        {
            string downedSpriteFolder = @"seven\";
            _downedSprite = Global.GetSingleImage(downedSpriteFolder);
        }

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
                    despawn: 11,
                    endlag: ATK_ENDLAG_S);
            setRunAnim(@"fillerAnim\");
            setIdleAnim(@"fillerPic\");
        }

        public override void DoMovement(double dt, double scrollVelocity, PointF playerCenter)
        {
            if (isDowned) 
            {
                MoveCharacter(dt, null, scrollVelocity);
                return;
            }

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
            else 
            {
                DoBasicAttack(); 
                MoveCharacter(dt, null, scrollVelocity);
            }

            if (_movementTimer >= MAX_MOVEMENT_TIME_S)
            {
                endlagS = MOV_ENDLAG_S;
                _movementTimer = 0;
            }
        }

        private void DoBasicAttack() => curAttack = _mainAtk;

        public override Bitmap NextAnimFrame()
        {
            if (isDowned) return _downedSprite;
            return base.NextAnimFrame();
        }
    }
}
