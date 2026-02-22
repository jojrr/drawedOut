namespace drawedOut
{
    internal class FlyingEnemy : Enemy
    {
        private const int 
            _FRICTION = 200,
            _ATTACK_FRAME = 8,
            _MOV_ENDLAG_S = 1,
            _ATK_ENDLAG_S = 3,
            _MAX_MOVEMENT_TIME_S = 2,
            _DEFAULT_PREFERRED_HEIGHT = 160,
            _DEFAULT_PROJECTILE_VELOCITY = 1000;
        private static readonly Bitmap 
            _projectileSprite,
            _downedSprite;
        private readonly ProjectileAttack _projectileAttack;
        private readonly float _maxRangeSqrd, _minRangeSqrd, _preferredHeight;
        private readonly int _projectileSpeed;
        private readonly Size _projectileSize;
        private int _maxXSpeed, _maxYSpeed;
        private double _movementTimer = 0;
        private double? 
            _xDiff, 
            _yDiff,
            _angleToFire;

        static FlyingEnemy()
        {
            string downedSpriteFolder = @"fillerAnim\";
            _downedSprite = Global.GetSingleImage(downedSpriteFolder);
            _projectileSprite = Global.GetSingleImage(@"fillerAnim\");
        }

        public FlyingEnemy(Point origin,
                int projectileWidth = 30, int projectileHeight = 30, float maxRange = 400, float minRange = 200,
                int projectileSpeed=_DEFAULT_PROJECTILE_VELOCITY, int preferredHeight=_DEFAULT_PREFERRED_HEIGHT)
            : base(origin: origin, width: 100, height: 100, hp: 3, maxXVelocity: 200, xAccel:300)
        {
            float _minRange = Math.Abs(minRange);
            float _maxRange = Math.Abs(maxRange);
            if (_minRange > _maxRange) _minRange = _maxRange;

            _maxRangeSqrd = (_maxRange*Global.BaseScale);
            _maxRangeSqrd *= _maxRangeSqrd;
            _minRangeSqrd = (_minRange*Global.BaseScale);
            _minRangeSqrd *= _minRangeSqrd;
            _preferredHeight = preferredHeight*Global.BaseScale;
            _projectileSpeed = (int)(projectileSpeed*Global.BaseScale);

            _projectileSize = new Size(
                    projectileWidth, 
                    projectileHeight
                    );

            _projectileAttack = new ProjectileAttack(
                    parent: this,
                    animation: new AnimationPlayer(animationFolder: @"fillerAnim\"),
                    endlag: _ATK_ENDLAG_S,
                    spawn:_ATTACK_FRAME,
                    projectileEvent: createProjectile);

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

            if ( _minRangeSqrd < distToPlayerSqrd && distToPlayerSqrd < _maxRangeSqrd )
            {
                MoveCharacter(dt, 0, yAccel, scrollVelocity);
                _angleToFire = angleToPlayer;
                _xDiff = xDiff;
                _yDiff = yDiff;
                curAttack = _projectileAttack;
                return;
            }

            float SinAngle = (float)Math.Sin(angleToPlayer) * Math.Sign(yDiff);
            float CosAngle = (float)Math.Cos(angleToPlayer) * Math.Sign(xDiff); 

            _maxXSpeed = (int)Math.Abs(CosAngle*maxVelocity);
            _maxYSpeed = (int)Math.Abs(SinAngle*maxVelocity);

            if (distToPlayerSqrd < _minRangeSqrd)
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
            if (!knockedBack) 
            {
                xVelocity += xAccel*dt;
                yVelocity += yAccel*dt;
                clampSpeed(_maxXSpeed, _maxYSpeed);
            }

            CheckAllPlatformCollision(dt);

            if (Math.Abs(scrollVelocity) > 0) ScrollChar(dt, scrollVelocity);
            else Location = new PointF(
                    Location.X + (float)(xVelocity * dt * Global.BaseScale), 
                    Location.Y + (float)(yVelocity * dt * Global.BaseScale)
                    ); 

            if ((xAccel == 0 && yAccel == 0) || knockedBack) decelerate(dt);
            if (xVelocity == 0 && yVelocity == 0)  knockedBack = false;

            checkInBoundary();
        }

        private void decelerate(double dt)
        {
            double deceleration = _FRICTION*dt;
            if (knockedBack) deceleration *= 10;
            double xSpeed = Math.Abs(xVelocity);
            double ySpeed = Math.Abs(yVelocity);
            if (Math.Abs(xVelocity) > deceleration)  xVelocity = Math.CopySign(xSpeed-deceleration, xVelocity);
            else xVelocity = 0; 
            if (Math.Abs(yVelocity) > deceleration)  yVelocity = Math.CopySign(ySpeed-deceleration, yVelocity);
            else yVelocity = 0; 
        }


        private void createProjectile()
        {
            if (_xDiff is null || _yDiff is null || _angleToFire is null) throw new NullReferenceException();

            Projectile flyingEnemyProj = new Projectile(
                    origin: this.Center,
                    width: _projectileSize.Width,
                    height: _projectileSize.Height,
                    sprite: _projectileSprite,
                    velocity: _projectileSpeed,
                    angle: _angleToFire.Value,
                    xDiff: -_xDiff.Value,
                    yDiff: -_yDiff.Value,
                    parent: this);

            _angleToFire = null;
            _xDiff = null;
            _yDiff = null;
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
