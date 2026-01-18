namespace drawedOut
{
    internal class FirstBoss : Enemy
    {
        private const int 
            ATK_ENDLAG_S = 1,
            ATK_X_OFFSET = 100,
            _X_KNOCK_DAMPEN = 800,
            _Y_KNOCK_DAMPEN = 300,
            MOV_ENDLAG_S = 3;
        private readonly Attacks _attackOne, _rangedAttackOne;
        private readonly double _maxRange, _jumpRange;
        private readonly Platform _activationDoor; 
        private readonly Action _itemDrop;
        private static Bitmap _downedSprite;
        private int _curState = 0;
        private double 
            _angleToPlayer,
            _xDiffToPlayer,
            _yDiffToPlayer;

        public FirstBoss(Point origin, int width, int height, ref Platform activationDoor, Action itemDrop,
                int hp=6)
            :base(origin:origin, width:width, height:height, hp:hp, xKnockDampen:_X_KNOCK_DAMPEN, yKnockDampen:_Y_KNOCK_DAMPEN)
        {
            Size atkSize = new Size(380,220);

            _itemDrop = itemDrop;
            _maxRange = Width*2.5;
            _jumpRange = 1.5*Height;
            _activationDoor = activationDoor;

            _attackOne = new Attacks(
                    parent: this,
                    size: atkSize,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: ATK_X_OFFSET,
                    spawn: 7,
                    despawn: 11,
                    endlag: ATK_ENDLAG_S,
                    dmg: 2);
            _rangedAttackOne = new ProjectileAttack(
                    parent:this,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    endlag: ATK_ENDLAG_S,
                    spawn: 5,
                    projectileEvent: DoAttack,
                    dmg: 3,
                    isLethal: false);

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

            switch (_curState%2)
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

            if (_curState == 5)
            {
                endlagS = MOV_ENDLAG_S;
                _curState = 0;
            }
        }

        private void AttackOneLogic(Global.XDirections? direction, double xDistance, double scrollVelocity, double dt)
        {
            if (curAttack is not null) 
            {
                MoveCharacter(dt, null, scrollVelocity);
                return;
            }
            if (Math.Abs(xDistance) < Hitbox.Width/2+ATK_X_OFFSET && curAttack is null)
            {
                curAttack = _attackOne;
                _curState += 1;
                direction=null;
            }
            MoveCharacter(dt, direction, scrollVelocity);
        }

        private void AttackTwoLogic(Global.XDirections? direction, double xDistance, double scrollVelocity, double dt, PointF playerCenter)
        {
            if (curAttack is not null) 
            {
                MoveCharacter(dt, null, scrollVelocity);
                return;
            }

            direction = (direction == Global.XDirections.left) ? Global.XDirections.right : Global.XDirections.left;
            if ( Math.Abs(xDistance) > _maxRange || MovingIntoWall )
            {
                calculateAngles(playerCenter);
                curAttack = _rangedAttackOne;
                _curState += 1;
                direction=null;
            }
            MoveCharacter(dt, direction, scrollVelocity);
        }

        private void calculateAngles(PointF target)
        {
            _xDiffToPlayer = target.X - Center.X;
            _yDiffToPlayer = target.Y - Center.Y;
            _angleToPlayer = (float)Math.Abs(Math.Atan(_yDiffToPlayer/_xDiffToPlayer)); 
        }

        private void DoDrop()
        {
            Item bossDrop = new Item(
                    origin:new PointF (Center.X, Center.Y - 1000),
                    width: 80,
                    height: 80,
                    action: _itemDrop,
                    sprite: Global.GetSingleImage(@"seven\")
                    );
        }

        public void DoAttack()
        {
            Size projectileSize = new Size(60,60);

            Projectile flyingEnemyProj = new Projectile(
                    origin: this.Center,
                    width: projectileSize.Width,
                    height: projectileSize.Height,
                    velocity: 1000,
                    angle: _angleToPlayer,
                    xDiff: _xDiffToPlayer,
                    yDiff: _yDiffToPlayer,
                    parent: this,
                    isLethal: _rangedAttackOne.IsLethal);
        }

        protected override void CheckDowned(bool isLethal)
        {
            if (isDowned && isLethal) 
            {
                isDowned = false;
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
            return base.NextAnimFrame();
        }
    }
}


