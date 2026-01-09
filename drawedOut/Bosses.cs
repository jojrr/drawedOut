namespace drawedOut
{
    internal class FirstBoss : Enemy
    {
        private const int 
            ATK_ENDLAG_S = 1,
            ATK_X_OFFSET = 100,
            MOV_ENDLAG_S = 3,
            MAX_MOVEMENT_TIME_S = 3;
        private readonly double 
            _maxRange,
            _jumpRange;
        private static Bitmap _downedSprite;
        private readonly Attacks _attackOne;
        private readonly Attacks _rangedAttackOne;
        private int _curState = 0;
        private double 
            _movementTimer,
            _angleToPlayer,
            _xDiffToPlayer,
            _yDiffToPlayer;

        public FirstBoss(Point origin, int width, int height, int hp=3)
            :base(origin:origin, width:width, height:height, hp:hp)
        {
            Size atkSize = new Size(380,520);

            _maxRange = Width+(atkSize.Width * Global.BaseScale);
            _jumpRange = 1.5*Height;

            _attackOne = new Attacks(
                    parent: this,
                    size: atkSize,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: ATK_X_OFFSET,
                    spawn: 7,
                    despawn: 11,
                    endlag: ATK_ENDLAG_S);
            _rangedAttackOne = new ProjectileAttack(
                    parent:this,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    endlag: ATK_ENDLAG_S,
                    spawn: 5,
                    projectileEvent: DoAttack);

            string downedSpriteFolder = @"seven\";
            _downedSprite = Global.GetSingleImage(downedSpriteFolder);
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

            switch (_curState)
            {
                case 0:
                    AttackOneLogic(
                            direction:direction,
                            xDistance:xDistance,
                            scrollVelocity:scrollVelocity,
                            dt:dt);
                    break;
                case 1:
                    AttackTwoLogic(
                            direction:direction,
                            xDistance:xDistance,
                            scrollVelocity:scrollVelocity,
                            dt:dt,
                            playerCenter:playerCenter);
                    break;
            }


            if (_movementTimer >= MAX_MOVEMENT_TIME_S)
            {
                endlagS = MOV_ENDLAG_S;
                _movementTimer = 0;
            }
        }

        private void AttackOneLogic(Global.XDirections direction, double xDistance, double scrollVelocity, double dt)
        {
            if (Math.Abs(xDistance) > _maxRange)
            {
                direction = (direction == Global.XDirections.left) ? Global.XDirections.right : Global.XDirections.left;
                MoveCharacter(dt, direction, scrollVelocity);
                _movementTimer += dt;
            }
            else 
            {
                curAttack = _attackOne;
                MoveCharacter(dt, null, scrollVelocity);
                _curState = 1;
            }
        }

        private void AttackTwoLogic(Global.XDirections direction, double xDistance, double scrollVelocity, double dt, PointF playerCenter)
        {
            if ( Math.Abs(xDistance) < _maxRange )
            {
                direction = (direction == Global.XDirections.left) ? Global.XDirections.right : Global.XDirections.left;
                MoveCharacter(dt, direction, scrollVelocity);
                _movementTimer += dt;
            }
            else 
            {
                calculateVelocities(playerCenter);
                curAttack = _rangedAttackOne;
                MoveCharacter(dt, null, scrollVelocity);
                _curState = 0;
            }
        }

        private void calculateVelocities(PointF target)
        {
            _xDiffToPlayer = target.X - Center.X;
            _yDiffToPlayer = target.Y - Center.Y;
            _angleToPlayer = (float)Math.Abs(Math.Atan(_yDiffToPlayer/_xDiffToPlayer)); 
        }


        public void DoAttack()
        {
            Size projectileSize = new Size(160,160);

            Projectile flyingEnemyProj = new Projectile(
                    origin: this.Center,
                    width: projectileSize.Width,
                    height: projectileSize.Height,
                    velocity: 1000,
                    angle: _angleToPlayer,
                    xDiff: _xDiffToPlayer,
                    yDiff: _yDiffToPlayer,
                    parent: this);
        }
    }
}


