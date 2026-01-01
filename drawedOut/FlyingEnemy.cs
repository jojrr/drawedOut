namespace drawedOut
{
    internal class FlyingEnemy : Enemy
    {
        private const int 
            _DEFAULT_PROJECTILE_VELOCITY = 1000, 
            _DEFAULT_PREFERRED_HEIGHT = 160,
            _FRICTION = 140;
        private const float
            _ATTACK_FRAME = 8,
            _MOV_ENDLAG_S = 1,
            _ATK_ENDLAG_S = 3,
            _MAX_MOVEMENT_TIME_S = 2;
        private static readonly Bitmap _downedSprite;
        private readonly AnimationPlayer _attackAnimation;
        private readonly float _maxRangeSqrd, _minRangeSqrd, _preferredHeight;
        private readonly int _projectileSpeed;
        private readonly Size _projectileSize;
        private float _maxXSpeed, _maxYSpeed;
        private double _movementTimer = 0;
        private bool _attacking = false;

        static FlyingEnemy()
        {
            string downedSpriteFolder = @"six\";
            _downedSprite = Global.GetSingleImage(downedSpriteFolder);
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

            _attackAnimation = new AnimationPlayer(animationFolder: @"fillerAnim\");
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
            //if (yDiff > -_preferredHeight) yDiff = Global.LevelSize.Height;
            double angleToPlayer = Math.Abs(Math.Atan(yDiff/xDiff));
            float distToPlayerSqrd = yDiff*yDiff + xDiff*xDiff;

            float xAccel=0;
            float yAccel=0;

            if (yDiff > -_preferredHeight) yAccel = -accel;

            if ( _minRangeSqrd < distToPlayerSqrd && distToPlayerSqrd < _maxRangeSqrd)
            {
                MoveCharacter(dt, 0, yAccel, scrollVelocity);
                _attacking = true;
                if (_attackAnimation.CurFrame == _ATTACK_FRAME) createProjectile(angleToPlayer, xDiff, yDiff);
                return;
            }

            float SinAngle = (float)Math.Sin(angleToPlayer) * Math.Sign(yDiff);
            float CosAngle = (float)Math.Cos(angleToPlayer) * Math.Sign(xDiff); 

            _maxXSpeed = Math.Abs(CosAngle*maxVelocity);
            _maxYSpeed = Math.Abs(SinAngle*maxVelocity);

            if (distToPlayerSqrd < _minRangeSqrd) //TODO: add "move to point" and "move away from point";
            {
                xAccel += CosAngle*accel;
                yAccel += SinAngle*accel;
            }
            else if (distToPlayerSqrd > _maxRangeSqrd)
            {
                xAccel -= CosAngle*accel;
                yAccel -= SinAngle*accel;
            }
            if (yDiff + yVelocity > _preferredHeight && !knockedBack) yAccel = 0; // reduces the effect of stutter when flying down

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

            if (!knockedBack) 
            {
                xVelocity = clampSpeedF(xVelocity, _maxXSpeed);
                yVelocity = clampSpeedF(yVelocity, _maxYSpeed);
            }

            if (xAccel == 0 || knockedBack) decelerate(dt);
            if (xVelocity == 0 && yVelocity == 0)  knockedBack = false;

            checkInBoundary();
        }

        private void decelerate(double dt)
        {
            double deceleration = (knockedBack) ? (_FRICTION/2): _FRICTION;
            deceleration = Math.Max(1.0001F, deceleration*dt);
            if (Math.Abs(xVelocity) > 0.0001)  xVelocity /= deceleration;
            else xVelocity = 0; 
            if (Math.Abs(yVelocity) > 0.0001)  yVelocity /= deceleration;
            else yVelocity = 0; 
        }

        private double clampSpeedF(double velocity, float maxSpeed) => (Math.Abs(velocity) > maxSpeed) ? Math.CopySign(maxSpeed, velocity) : velocity;


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
            if (_attacking)
            {
                if (_attackAnimation.CurFrame == _attackAnimation.LastFrame) _attacking = false;
                return _attackAnimation.NextFrame(FacingDirection);
            }
            return idleAnim.NextFrame(FacingDirection);
        }
    }
}
