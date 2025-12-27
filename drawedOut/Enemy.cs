namespace drawedOut
{
    internal class Enemy : Character
    {
        public static List<Enemy> ActiveEnemyList = new List<Enemy>();
        public static List<Enemy> InactiveEnemyList = new List<Enemy>();

        public Enemy(Point origin, int width, int height, int hp, int xAccel=100, int maxXVelocity=600)
            : base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        {
            InactiveEnemyList.Add(this);
        }

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
            Global.XDirections direction = (xDistance > 0) 
                ? Global.XDirections.left : Global.XDirections.right;

            if (endlagS > 0) 
            {
                this.MoveCharacter(dt, null, scrollVelocity);
                FacingDirection = direction;
                return;
            }

            if ( yDistance > _jumpRange ) DoJump();

            if ( Math.Abs(xDistance) > _maxRange )
            {
                this.MoveCharacter(dt, direction, scrollVelocity);
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

    internal class FlyingEnemy : Enemy
    {

        private const int _PROJECTILE_VELOCITY = 3000, FRICTION = 40;
        private const float
            _MOV_ENDLAG_S = 3,
            _ATK_ENDLAG_S = 5,
            _MAX_MOVEMENT_TIME_S = 2;
        private readonly float
            _maxRangeSqrd,
            _minRangeSqrd;
        private readonly Size _projectileSize;

        private double _movementTimer=0;
        private float
            _maxXSpeed,
            _maxYSpeed;

        public FlyingEnemy(Point origin, int projectileWidth = 10, int projectileHeight = 10, float maxRange = 200, float minRange = 100, int projectileSpeed = _PROJECTILE_VELOCITY)
            : base(origin: origin, width: 40, height: 40, hp: 3, maxXVelocity: 200)
        {
            _maxRangeSqrd = (maxRange * Global.BaseScale);
            _maxRangeSqrd *= _maxRangeSqrd;
            _minRangeSqrd = (minRange * Global.BaseScale);
            _minRangeSqrd *= _minRangeSqrd;

            _projectileSize = new Size(
                    projectileWidth, 
                    projectileHeight
                    );
        }

        public override void DoMovement(double dt, double scrollVelocity, PointF playerCenter)
        {
            // if (endlagS > 0) return;

            float xDiff = Center.X - playerCenter.X;
            float yDiff = Center.Y - playerCenter.Y;
            double angleToPlayer = Math.Abs(Math.Atan(yDiff/xDiff));
            float distToPlayerSqrd = yDiff*yDiff + xDiff*xDiff;

            float xAccel=0;
            float yAccel=0;

            if ( _minRangeSqrd < distToPlayerSqrd && distToPlayerSqrd < _maxRangeSqrd)
            {
                MoveCharacter(dt, xAccel, yAccel, scrollVelocity);
                createProjectile(angleToPlayer, xDiff, yDiff);
                return;
            }

            float SinAngle = (float)Math.Sin(angleToPlayer) * Math.Sign(yDiff);
            float CosAngle = (float)Math.Sin(angleToPlayer) * Math.Sign(xDiff);

            _maxXSpeed = Math.Abs(CosAngle*maxVelocity);
            _maxYSpeed = Math.Abs(SinAngle*maxVelocity);

            if (distToPlayerSqrd < _minRangeSqrd)
            {
                xAccel += CosAngle*accel;
                yAccel += SinAngle*accel;
            }
            else
            {
                xAccel -= CosAngle*accel;
                yAccel -= SinAngle*accel;
            }

            _movementTimer += dt;
            MoveCharacter(dt, xAccel, yAccel, scrollVelocity);

            if (_movementTimer >= _MAX_MOVEMENT_TIME_S)
            {
                endlagS = _MOV_ENDLAG_S;
                _movementTimer = 0;
            }
        }

        private void MoveCharacter(double dt, float xAccel, float yAccel, double scrollVelocity)
        {
            xVelocity += xAccel;
            yVelocity += yAccel;

            if (Math.Abs(scrollVelocity) > 0) ScrollChar(dt, scrollVelocity);
            else Location = new PointF(
                    Location.X + (float)(xVelocity * dt), Location.Y + (float)(yVelocity * dt)
                    ); 

            if (xVelocity == 0 && yVelocity == 0) return;

            yVelocity = clampSpeedF(yVelocity, _maxYSpeed);
            if (!knockedBack) xVelocity = clampSpeedF(xVelocity, _maxXSpeed);
            else xVelocity = clampSpeedF(xVelocity, knockBackVelocity);

            if (xAccel == 0 || knockedBack) decelerate(dt);
            if (Math.Abs(xVelocity) <= _maxXSpeed) knockedBack = false;

            checkInBoundary();
        }

        private void decelerate(double dt)
        {
             float deceleration = (knockedBack) ? (FRICTION/2): FRICTION;
             deceleration = MathF.Max(1.05F, (float)(deceleration*dt));
             if (Math.Abs(xVelocity) > deceleration)  xVelocity /= deceleration;
             else xVelocity = 0; 
             if (Math.Abs(yVelocity) > deceleration)  yVelocity /= deceleration;
             else yVelocity = 0; 
        }

        private double clampSpeedF(double speed, float maxSpeed) => (Math.Abs(speed) > maxSpeed) ? Math.CopySign(maxSpeed, speed) : speed;


        private void createProjectile(double angle, double xDiff, double yDiff)
        {
            endlagS += _ATK_ENDLAG_S;
            Projectile flyingEnemyProj = new Projectile(
                    origin: this.Center,
                    width: _projectileSize.Width,
                    height: _projectileSize.Height,
                    velocity: _PROJECTILE_VELOCITY,
                    angle: angle,
                    xDiff: xDiff,
                    yDiff: yDiff);
        }
    }
}

