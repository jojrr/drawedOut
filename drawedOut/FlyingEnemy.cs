namespace drawedOut
{
    internal class FlyingEnemy : Enemy
    {
        private const int 
            _DEFAULT_PROJECTILE_VELOCITY = 1000, 
            _DEFAULT_PREFERRED_HEIGHT = 160,
            _FRICTION = 20;
        private const float
            _MOV_ENDLAG_S = 1,
            _ATK_ENDLAG_S = 3,
            _MAX_MOVEMENT_TIME_S = 2;
        private static Bitmap _downedSprite;
        private readonly float _maxRangeSqrd, _minRangeSqrd, _preferredHeight;
        private readonly int _projectileSpeed;
        private readonly Size _projectileSize;
        private float _maxXSpeed, _maxYSpeed;
        private double _movementTimer = 0;

        static FlyingEnemy()
        {
            string downedSpriteFolder = @"six\";
            _downedSprite = getDownedSprite(downedSpriteFolder);
        }

        public FlyingEnemy(Point origin,
                int projectileWidth = 40, int projectileHeight = 40, float maxRange = 400, float minRange = 200, 
                int projectileSpeed=_DEFAULT_PROJECTILE_VELOCITY, int preferredHeight=_DEFAULT_PREFERRED_HEIGHT)
            : base(origin: origin, width: 40, height: 40, hp: 3, maxXVelocity: 200)
        {
            float _minRange = Math.Abs(minRange);
            float _maxRange = Math.Abs(maxRange);
            if (_minRange > _maxRange) _minRange = _maxRange;

            _maxRangeSqrd = (_maxRange * Global.BaseScale);
            _maxRangeSqrd *= _maxRangeSqrd;
            _minRangeSqrd = (_minRange * Global.BaseScale);
            _minRangeSqrd *= _minRangeSqrd;
            _preferredHeight = preferredHeight*Global.BaseScale;
            _projectileSpeed = (int)(projectileSpeed*Global.BaseScale);

            _projectileSize = new Size(
                    projectileWidth, 
                    projectileHeight
                    );

            setIdleAnim(@"fillerAnim\");
        }

        public override void DoMovement(double dt, double scrollVelocity, PointF playerCenter)
        {
            if (isDowned) 
            {
                MoveCharacter(dt, null, scrollVelocity);
                return;
            }

            float xDiff = Center.X - playerCenter.X;
            if (endlagS > 0) 
            {
                MoveCharacter(dt, 0, 0, scrollVelocity);
                FacingDirection = (xDiff > 0) ? Global.XDirections.left : Global.XDirections.right;
                return;
            }

            float yDiff = Center.Y - playerCenter.Y;
            if (yDiff > -_preferredHeight) yDiff = Global.LevelSize.Height;
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
            float CosAngle = (float)Math.Cos(angleToPlayer) * Math.Sign(xDiff); 

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

            CheckAllPlatformCollision();

            if (Math.Abs(scrollVelocity) > 0) ScrollChar(dt, scrollVelocity);
            else Location = new PointF(
                    Location.X + (float)(xVelocity * dt), Location.Y + (float)(yVelocity * dt)
                    ); 

            yVelocity = clampSpeedF(yVelocity, _maxYSpeed);
            if (!knockedBack) xVelocity = clampSpeedF(xVelocity, _maxXSpeed);
            else xVelocity = clampSpeedF(xVelocity, knockBackVelocity);

            if (xAccel == 0 || knockedBack) decelerate(dt);
            if (Math.Abs(xVelocity) <= _maxXSpeed) knockedBack = false;

            checkInBoundary();
        }

        private void decelerate(double dt)
        {
            float deceleration = (knockedBack) ? (_FRICTION/2): _FRICTION;
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
                    velocity: _projectileSpeed,
                    angle: angle,
                    xDiff: -xDiff,
                    yDiff: -yDiff,
                    parent: this);
        }

        public override Bitmap NextAnimFrame()
        {
            if (isDowned) return _downedSprite;
            if (curAttack is not null)
            {
                if (curAttack.Animation.CurFrame == curAttack.Animation.LastFrame)
                {
                    Bitmap atkAnim = curAttack.NextAnimFrame(FacingDirection);
                    curAttack = null;
                    return atkAnim;
                }
                return curAttack.NextAnimFrame(FacingDirection);
            }
            return idleAnim.NextFrame(FacingDirection);
        }
    }
}
