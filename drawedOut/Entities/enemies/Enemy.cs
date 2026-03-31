namespace drawedOut
{
    internal abstract class Enemy : Character
    {
        public static IReadOnlyCollection<Enemy> ActiveEnemyList => _activeEnemyList;
        public static IReadOnlyCollection<Enemy> InactiveEnemyList => _inactiveEnemyList;

        protected bool isDowned = false;

        private static HashSet<Enemy> _activeEnemyList = new HashSet<Enemy>();
        private static HashSet<Enemy> _inactiveEnemyList = new HashSet<Enemy>();
        private static bool _allInactive = false;
        // default knockback taken in pixels/second from attacks
        private const int 
            _DEFAULT_X_KNOCKBACK = 1000, 
            _DEFAULT_Y_KNOCKBACK = 500;
        private const float _DEFAULT_DOWN_TIME_S = 5;
        // knocbback dampening from player attacsk 
        private readonly int _xKnockDampen, _yKnockDampen;
        private readonly float _maxDownTime;
        // timer to track how long the enemy has been downed for
        private double _downTimer = 0;

        public Enemy(Point origin, int width, int height, int hp,
                int xAccel=100, int maxXVelocity=600, int xKnockDampen=0, int yKnockDampen=0,
                float maxDownTime=_DEFAULT_DOWN_TIME_S, int jumpVelocity=1100)
            : base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, 
                    maxXVelocity: maxXVelocity, jumpVelocity: jumpVelocity)
        { 
            _xKnockDampen = xKnockDampen;
            _yKnockDampen = yKnockDampen;
            _maxDownTime = maxDownTime;
            _inactiveEnemyList.Add(this); 
        }
        
        public static void DeactivateAll() => _allInactive = true;
        public static void ReActivate() => _allInactive = false;

        public virtual void DoMovement(double dt, double scrollVelocity, PointF playerCenter) => 
            throw new Exception($"DoMove is not implemented in {this.GetType()}");

        public void DoDamage(Attacks sourceAtk, int xKnock=_DEFAULT_X_KNOCKBACK, int yKnock=_DEFAULT_Y_KNOCKBACK)
        {
            base.DoDamage(sourceAtk, xKnock, yKnock, _xKnockDampen, _yKnockDampen);
            CheckDowned(sourceAtk.IsLethal);
        }

        public void DoDamage(Projectile sourceProjectile)
        {
            base.DoDamage(sourceProjectile, _xKnockDampen, _yKnockDampen);
            CheckDowned(sourceProjectile.IsLethal);
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
            foreach (Enemy e in Enemy._activeEnemyList) 
            { 
                e.TickAllCounters(dt); 
                e.IncDownTimer(dt);
            }
        }

        protected void SetActive()
        {
            if (IsActive) return;
            IsActive = true;
            _activeEnemyList.Add(this);
            _inactiveEnemyList.Remove(this);
        }

        protected void SetInactive()
        {
            if (!IsActive) return;
            IsActive = false;
            _inactiveEnemyList.Add(this);
            _activeEnemyList.Remove(this);
        }

        public new static void ClearAllLists()
        {
            _activeEnemyList.Clear();
            _inactiveEnemyList.Clear();
        }


        public override void CheckActive()
        {
            if (_allInactive) 
            {
                SetInactive();
                return;
            }

            if ((Hp <= 0 && !isDowned) || !checkInBoundary())
            { 
                DoDeath(); 
                return;
            }

            if (DistToMid > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;
                SetInactive();
                return;
            }

            if (IsActive) return;
            SetActive();
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
            iFrames = 67;   // apply arbitrary duration of iFrames as long as the enemy is downed
                            // (should probably be bigger than duration of one game tick)
        }

        protected void DoDeath()
        {
            if (!IsActive) return;
            SetInactive();
            if (this.curAttack is not null) this.curAttack.Dispose();
        }

        protected virtual void CheckDowned(bool isLethal)
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
    }


    internal class MeleeEnemy : Enemy
    {
        private const int 
            ATK_ENDLAG_S = 1,
            MOV_ENDLAG_S = 3,
            ATK_X_OFFSET = 100,     // in pixels
            MAX_MOVEMENT_TIME_S = 3;
        private readonly double 
            _maxRange,  // (in pixels) if the player is further than this horizontally, the enemy will move towards the player.
            _jumpRange; // (in pixels) if the player is further than this vertically, the enemy will jump
        private readonly AnimationPlayer _atkAnim = new AnimationPlayer(@"meleeEnemy\atk\");
        private static Bitmap _downedSprite;
        private readonly Attacks _mainAtk;
        private double _movementTimer;

        static MeleeEnemy()
        {
            _downedSprite = Global.GetSingleImage(@"meleeEnemy\", "downed.png");
        }

        public MeleeEnemy(Point origin)
            : base(origin: origin, width: 100, height: 180, hp: 3, maxXVelocity: 400)
        {
            Size atkSize = new Size(180,220);

            _maxRange = Width+(atkSize.Width - ATK_X_OFFSET* Global.BaseScale);
            _jumpRange = 1.5*Height;

            _mainAtk = new Attacks(
                    parent: this,
                    size: atkSize,
                    animation: _atkAnim,
                    xOffset: ATK_X_OFFSET,
                    spawn: 12,
                    despawn: 16,
                    endlag: ATK_ENDLAG_S);
            setRunAnim(@"meleeEnemy\run");
            setIdleAnim(@"meleeEnemy\idle\");
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
