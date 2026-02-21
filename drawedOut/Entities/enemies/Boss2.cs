namespace drawedOut
{
    internal class SecondBoss : Enemy
    {
        private const int 
            _FRICTION = 200,
            ATK_X_OFFSET = 100,
            _X_KNOCK_DAMPEN = 200,
            _Y_KNOCK_DAMPEN = 200,
            _PREFERRED_HEIGHT = 400,
            _ROOST_TIME = 2,
            _HP = 9;
        private static readonly Bitmap 
            _projectileSprite = Global.GetSingleImage(@"fillerPic\"),
            _downedSprite = Global.GetSingleImage(@"fillerPic\");
        private static Stopwatch _levelTimerSW;
        private static Random _rnd = new Random();
        private static readonly ProjectileAttack 
            _attackOne = new ProjectileAttack(
                    parent: null,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    spawn: 3,
                    endlag: 2,
                    dmg: 1,
                    projectileEvent: ()=>{}),
            _attackTwo = new ProjectileAttack(
                    parent: null,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    spawn: 3,
                    endlag: 1,
                    dmg: 2,
                    projectileEvent: ()=>{});

        private readonly double _maxRangeSqrd, _minRangeSqrd;
        private readonly Platform _activationDoor; 
        private readonly Action _itemDrop;
        private bool _roost = false;
        private int 
            _curState = 0,
            _maxXSpeed,
            _maxYSpeed;
        private double 
            _angleToPlayer,
            _xDiff,
            _yDiff;

        public SecondBoss(Point origin, int width, int height, ref Platform activationDoor, Action itemDrop, 
                ref Stopwatch levelTimerSW)
            :base(origin:origin, width:width, height:height, 
                    xKnockDampen:_X_KNOCK_DAMPEN, yKnockDampen:_Y_KNOCK_DAMPEN, hp:_HP, xAccel:500)
        {
            _itemDrop = itemDrop;
            _maxRangeSqrd = 500*Global.BaseScale;
            _maxRangeSqrd*=_maxRangeSqrd;
            _minRangeSqrd = 200*Global.BaseScale;
            _minRangeSqrd*=_minRangeSqrd;
            _levelTimerSW = levelTimerSW;
            _activationDoor = activationDoor;

            setIdleAnim(@"fillerPic\");
            _attackOne.Reset();
            _attackOne.Parent=this;
            _attackOne.SetEvent(doAttack1);
            _attackTwo.Reset();
            _attackTwo.Parent=this;
            _attackTwo.SetEvent(doAttack2);
        }
        
        private void doAttack1()
        {
            Projectile p;
            
            int projCount = 8;
            for (int i=0; i<projCount; i++)
            {
                p = new Projectile(
                        origin: this.Center,
                        width: 60,
                        height: 60,
                        velocity: 1000,
                        angle: (i*2*Math.PI/projCount),
                        xDiff: 1,
                        yDiff: 1,
                        parent: this,
                        sprite: Global.GetSingleImage(@"fillerPic\"));
            }
            _curState++;
        }

        private void doAttack2()
        {
            Projectile p;
            
            int projCount = 2;
            for (int i=0; i<projCount; i++)
            {
                p = new Projectile(
                        origin: this.Center,
                        width: 60,
                        height: 60,
                        velocity: 1000,
                        angle: (_angleToPlayer + i*Math.PI),
                        xDiff: 1,
                        yDiff: 1,
                        parent: this,
                        bouncy: true,
                        dmg: 2,
                        sprite: Global.GetSingleImage(@"fillerPic\"));
            }
            _curState++;
        }

        private void calculateAngles(PointF target)
        {
            _xDiff = target.X - Center.X;
            _yDiff = target.Y - Center.Y;
            _angleToPlayer = (float)Math.Abs(Math.Atan(_yDiff/_xDiff)); 
        }

        private void DoDrop()
        {
            Item bossDrop = new Item(
                    origin:new PointF (Center.X, Center.Y - 1000),
                    width: 80,
                    height: 80,
                    action: _itemDrop,
                    sprite: Global.GetSingleImage(@"fillerAnim\")
                    );
        }

        public override void DoMovement(double dt, double scrollVelocity, PointF playerCenter)
        {
            if (_roost && endlagS == 0) _roost = false;
            if (isDowned || _roost)
            {
                MoveCharacter(dt, null, scrollVelocity);
                return;
            }

            float xDiff = Center.X - playerCenter.X;
            if (endlagS > 0) 
            {
                MoveCharacter(dt, 0, 0);
                FacingDirection = (xDiff > 0) ? Global.XDirections.left : Global.XDirections.right;
                return;
            }

            float yDiff = Center.Y - playerCenter.Y;
            double angleToPlayer = Math.Abs(Math.Atan(yDiff/xDiff));
            float distToPlayerSqrd = yDiff*yDiff + xDiff*xDiff;

            float xAccel=0;
            float yAccel=0;

            if (yDiff > -_PREFERRED_HEIGHT) yAccel = -accel;

            if ( _minRangeSqrd < distToPlayerSqrd && distToPlayerSqrd < _maxRangeSqrd )
            {
                MoveCharacter(dt, 0, yAccel);
                _angleToPlayer = angleToPlayer;
                _xDiff = xDiff;
                _yDiff = yDiff;
                if (curAttack is null && endlagS == 0)
                {
                    if (_rnd.Next(0,2) == 0)
                    {
                        curAttack = _attackOne;
                        xAccel = (float)Math.CopySign(accel,xDiff)*100;
                        MoveCharacter(dt, -xAccel, 0);
                        return;
                    }
                    else curAttack = _attackTwo;
                }
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
            if (yDiff + yVelocity > _PREFERRED_HEIGHT && !knockedBack) yAccel = 0; 

            MoveCharacter(dt, xAccel, yAccel);

            if (_curState == 5) 
            {
                _roost = true;
                endlagS = _ROOST_TIME;
            }
        }

        private void MoveCharacter(double dt, float xAccel, float yAccel)
        {
            if (!knockedBack) 
            {
                xVelocity += xAccel*dt;
                yVelocity += yAccel*dt;
                clampSpeed(_maxXSpeed, _maxYSpeed);
            }

            Location = new PointF(
                    Location.X + (float)(xVelocity * dt * Global.BaseScale), 
                    Location.Y + (float)(yVelocity * dt * Global.BaseScale)
                    ); 

            CheckAllPlatformCollision(dt);

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

        protected override void CheckDowned(bool isLethal)
        {
            if (isDowned && isLethal) 
            {
                isDowned = false;
                _levelTimerSW.Stop();
                DoDrop();
                return;
            }
            else if (Hp <= 0)
            {
                isDowned = true;
                if (curAttack is not null)
                {
                    curAttack.Dispose();
                    curAttack = null;
                }
            }
        }

        public override void CheckActive()
        {
            base.CheckActive();
            if (IsActive) return;
            if (_activationDoor.IsActive && Hp > 0) SetActive();
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


