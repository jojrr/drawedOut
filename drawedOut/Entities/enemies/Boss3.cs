namespace drawedOut
{
    internal class ThirdBoss : Enemy
    {
        private const float _PROJ_DROP_INTERVAL = 0.6f;
        private const int 
            _X_KNOCK_DAMPEN = 600,
            _Y_KNOCK_DAMPEN = 300,
            _MOV_ENDLAG_S = 3;
        private static readonly Bitmap 
            _projectileSprite = Global.GetSingleImage(@"fillerPic\"),
            _downedSprite = Global.GetSingleImage(@"fillerPic\");
        private static readonly Random rnd = new Random();
        private static readonly Attacks 
            _chargeAtk = new Attacks(
                    parent: null,
                    width: 180,
                    height: 180,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 0,
                    spawn: 0,
                    endlag: 1,
                    dmg: 2),
            _slamAtk = new Attacks(
                    parent: null,
                    width: 600,
                    height: 80,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 0,
                    spawn: 0,
                    endlag: 2,
                    dmg: 3);
        private static readonly ProjectileAttack 
            _tripleAtk = new(
                    parent: null,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    endlag: 0,
                    spawn: 14,
                    projectileEvent: ()=>{},
                    dmg: 1,
                    isLethal: false),
            _jumpAtk = new(
                    parent: null,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    endlag: 0,
                    spawn: 14,
                    projectileEvent: ()=>{},
                    dmg: 1,
                    isLethal: false);

        private static Stopwatch _levelTimerSW;

        private readonly double _maxRange;
        private readonly Platform _activationDoor; 
        private readonly Action _itemDrop;
        private bool _toSlam;
        private int 
            _curState = 0,
            _chargeVelocity = 0,
            _tripleAtkLeft = 0;
        private double 
            _tripleDelay,
            _xDiffToPlayer;

        public ThirdBoss(Point origin, int width, int height, ref Platform activationDoor, Action itemDrop, 
                ref Stopwatch levelTimerSW,
                int hp=6)
            :base(origin:origin, width:width, height:height, hp:hp, jumpVelocity:1800,
                    xKnockDampen:_X_KNOCK_DAMPEN, yKnockDampen:_Y_KNOCK_DAMPEN)
        {
            _itemDrop = itemDrop;
            _maxRange = Width*2.2;
            _levelTimerSW = levelTimerSW;
            _activationDoor = activationDoor;
            endlagS = 2;

            setRunAnim(@"fillerAnim\");
            setIdleAnim(@"fillerPic\");
            _chargeAtk.Reset();
            _chargeAtk.Parent=this;
            _slamAtk.Reset();
            _slamAtk.Parent=this;
            _tripleAtk.Reset();
            _tripleAtk.Parent=this;
            _tripleAtk.SetEvent(doTripleAtk);
            _jumpAtk.Reset();
            _jumpAtk.Parent=this;
            _jumpAtk.SetEvent(doJumpAtk);
        }

        private void doTripleAtk()
        {
            _tripleAtkLeft = 3;
            _curState++;
        }

        private void spawnFallingBullet(PointF playerCenter)
        {
            _tripleDelay = _PROJ_DROP_INTERVAL;
            Projectile p = new(
                    origin: new PointF(playerCenter.X, playerCenter.Y - 400),
                    parent: this,
                    width: 80,
                    height: 150,
                    velocity: 1,
                    accel: 1500,
                    angle: Math.PI/2,
                    maxSpeed: 4000,
                    xDiff: 1,
                    yDiff: 1,
                    dmg: _tripleAtk.AtkDmg,
                    sprite: _projectileSprite);
        }

        private void doJumpAtk()
        {
            int projCount = 3;
            double angleOffset = Math.PI/12;
            for (int i=0; i<projCount; i++)
            {
                Projectile p = new Projectile(
                        origin: this.Center,
                        width: 60,
                        height: 60,
                        velocity: 1000,
                        angle: angleOffset*i-angleOffset,
                        xDiff: -_xDiffToPlayer,
                        yDiff: 1,
                        parent: this,
                        dmg: _jumpAtk.AtkDmg,
                        sprite: Global.GetSingleImage(@"fillerPic\"));
            }
            _toSlam = true;
            _curState--;
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
            Global.XDirections? direction = (xDistance > 0) ? Global.XDirections.left : Global.XDirections.right;

            if (_tripleAtkLeft > 0)
            {
                if (_tripleDelay <= 0)
                {
                    _tripleAtkLeft--;
                    spawnFallingBullet(playerCenter);
                    _tripleDelay = _PROJ_DROP_INTERVAL;
                }
                _tripleDelay -= dt;
            }
            else _tripleDelay = 0;

            if (endlagS > 0)
            {
                MoveCharacter(dt, null, scrollVelocity);
                FacingDirection = direction.Value;
                return;
            }

            if (_toSlam && IsOnFloor)
            {
                curAttack = _slamAtk;
                _toSlam = false;
                _curState++;
                return;
            }

            if (curAttack is null)
            {
                _chargeVelocity = 0;
                if (rnd.Next(0,2) == 0) 
                {
                    curAttack = _chargeAtk;
                    _chargeVelocity = (int)Math.CopySign(1000, -xDistance);
                }
                else if (xDistance <= _maxRange) 
                {
                    _xDiffToPlayer = xDistance;
                    curAttack = _jumpAtk;
                    DoJump();
                }
                else curAttack = _tripleAtk;
                _curState++;
            }
            else if (curAttack == _chargeAtk) chargeAtkLogic(dt);
            else if (curAttack == _tripleAtk) direction = null;


            MoveCharacter(dt, direction, 0);

            if (_curState == 10)
            {
                endlagS = _MOV_ENDLAG_S;
                _curState = 0;
            }
        }

        private void chargeAtkLogic(double dt)
        {
            xVelocity = _chargeVelocity;
            MoveCharacter(dt, null, 0);
            if (!MovingIntoWall) return;
            curAttack = null;
            endlagS = _chargeAtk.Endlag;
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
            return base.NextAnimFrame();
        }
    }
}


