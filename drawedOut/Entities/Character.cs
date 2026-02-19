namespace drawedOut
{
    internal abstract class Character : Entity
    {
        public Global.XDirections FacingDirection { get; protected set; }
        public bool IsOnFloor { get; private set; }
        public bool MovingIntoPlatform 
        { get => (MovingIntoWall || (yVelocity < 0 && _curYColliderDirection == Global.YDirections.top)); }

        public bool MovingIntoWall 
        {
            get 
            {
                if (_lastXDirection is null || _curXColliderDirection is null) return false;
                if (_curXColliderDirection == _lastXDirection) return true;
                return false;
            }
        }

        protected AnimationPlayer? idleAnim { get => _idleAnim; private set => _idleAnim = value; }
        protected AnimationPlayer? runAnim { get => _runAnim; private set => _runAnim = value; }
        protected bool knockedBack { get => _knockedBack; set => _knockedBack = value; }
        protected int knockBackVelocity { get => _xKnockbackVelocity; }
        protected int maxVelocity { get => _maxXVelocity; }
        protected int curXAccel { get => _curXAccel; }
        protected int accel { get => _xAccel; }
        protected Attacks? curAttack;
        protected double
            iFrames,
            xVelocity, yVelocity,
            movementEndlagS=0, endlagS=0;

        /// <summary> the horizontal direction of the platfrom from the character that is colliding </summary>
        private Global.XDirections? _curXColliderDirection = null;
        /// <summary> the vertical direction of the platfrom from the character that is colliding </summary>
        private Global.YDirections? _curYColliderDirection = null;

        private static readonly int _GRAVITY = Global.Gravity;
        private int _maxHp, _hp, _curXAccel, _xKnockbackVelocity, _yKnockbackVelocity;
        private AnimationPlayer? _idleAnim, _runAnim;
        private Entity? _xStickEntity, _yStickEntity;
        private RectangleF? _xStickTarget, _yStickTarget;
        private Global.XDirections? _lastXDirection;
        private bool _knockedBack = false;
        private double _coyoteTimeS;
        private const int 
            _TERMINAL_VELOCITY=3000,
            _JUMP_VELOCITY=1100,
            FRICTION=2000;
        private readonly int
            _maxYVelocity,
            _maxXVelocity,
            _xAccel;

        /// <summary>
        /// Initalises a "character" (entity with velocity and gravity)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hp"> 
        ///
        protected Character(Point origin, int width, int height, int hp, int xAccel, int maxXVelocity, 
            int maxYVelocity=_TERMINAL_VELOCITY)
            : base(origin: origin, width: width, height: height)
        {
            IsOnFloor = false;
            xVelocity = 0;
            yVelocity = 0;
            _hp = hp;
            _maxHp = hp;
            _xAccel = xAccel;
            _maxXVelocity = maxXVelocity;
            _maxYVelocity = maxYVelocity;
        }

        protected void setIdleAnim(string filePath)
        {
            if (_idleAnim is not null)
                throw new Exception("cannot write to _idleAnim when already not null");
            _idleAnim = new AnimationPlayer(filePath);
        }
        protected void setRunAnim(string filePath)
        {
            if (_runAnim is not null)
                throw new Exception("cannot write to _runAnim when already not null");
            _runAnim = new AnimationPlayer(filePath);
        }

        protected void TickEndlag(double dt) => endlagS = Math.Max(endlagS-dt,0); 
        protected void TickIFrames(double dt) => iFrames = Math.Max(iFrames-dt,0); 
        protected void TickMovFrames(double dt) => movementEndlagS = Math.Max(movementEndlagS-dt, 0); 
        protected void TickAllCounters(double dt)
        { TickIFrames(dt); TickEndlag(dt); TickMovFrames(dt); }

        public int Hp 
        { 
            get => _hp;
            protected set => _hp = (value > _maxHp) ? _maxHp : value; 
        }

        public int MaxHp 
        { 
            get => _maxHp; 
            private set
            {
                if (value <= 0) throw new Exception("MaxHp must be > 0");
                _maxHp = value; 
            }
        }

        public RectangleF AnimRect 
        {
            get 
            {
                float sqrSize = Math.Max(Hitbox.Width, Hitbox.Height);
                if (this is Player) sqrSize *= 1.3F;
                else sqrSize *= 1.1F;

                PointF p = new PointF(Center.X - sqrSize/2, Hitbox.Bottom - sqrSize);
                SizeF s = new SizeF(sqrSize, sqrSize);
                return new RectangleF(p,s);
            }
        }

        public void ApplyEndlag(double endlag, bool overrideCurrent=false)
        {
            if (endlagS > 0 && !overrideCurrent) return;
            endlagS = endlag;
        }

        /// <summary>
        /// checks the Y direction for collision with platforms
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity"/> that is being checked </param>
        private void checkYCollider(RectangleF targetHitbox, Entity collisionTarget)
        {
            // Checks if there is a platform below
            if (Hitbox.Bottom <= targetHitbox.Bottom && Hitbox.Top < targetHitbox.Top)
                SetYCollider(Global.YDirections.bottom, targetHitbox, collisionTarget);
            
            // Checks if there is a platform above the character
            else if (targetHitbox.Top < Hitbox.Top && Hitbox.Bottom > targetHitbox.Bottom)
                SetYCollider(Global.YDirections.top, targetHitbox, collisionTarget);
        }

        /// <summary>
        /// checks the X direction for collision with entities (mostly platforms)
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity"/> that is being checked </param>
        private void checkXCollider(RectangleF targetHitbox, Entity collisionTarget, double dt)
        {
            if (xVelocity == 0) return;
            float centerX = Center.X;
            float finalX = centerX + (float)(xVelocity*dt);
            float left = (float)(Math.Min(finalX, centerX)*0.98);
            float right = (float)(Math.Max(finalX, centerX)*1.02);
            if (left <= targetHitbox.Right && right >= targetHitbox.Left)
            {
                Global.XDirections collidingDirection;

                if (centerX - collisionTarget.Center.X < 0) collidingDirection = Global.XDirections.right; // platform is on right of character
                else collidingDirection = Global.XDirections.left; // platform is on left of character

                if (_xStickEntity is null && LocationY > targetHitbox.Y) xVelocity = 0; 

                SetXCollider(collidingDirection, targetHitbox, collisionTarget); 
            }
        }

        /// <summary>
        /// Checks if the target's hitbox is colliding with this entity's hitbox.<br/>
        /// Returned position is relative to this Entity.<br/>
        /// </summary>
        /// <param name="collisionTarget">The target to check for collision with</param>
        /// <returns> True if a collider is found </returns>
        private bool IsCollidingWith(Entity collisionTarget, double dt)
        {
            RectangleF targetHitbox = collisionTarget.Hitbox;

            // sets collision to null if not longer colliding with the previously colliding hitbox
            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (collisionTarget == _xStickEntity)  SetXCollider(null, null, null); 
                if (collisionTarget == _yStickEntity)  SetYCollider(null, null, null); 
                return false;
            }

            // if this' center is between the left and the right of the hitbox 
            if (targetHitbox.Right > Center.X && Center.X > targetHitbox.Left) 
                checkYCollider(targetHitbox, collisionTarget);

            if (_yStickEntity == collisionTarget) return true;

            if ((_xStickEntity == _yStickEntity) && IsOnFloor) // Stops the character from bugging on corners
                SetXCollider(null, null, collisionTarget);
            else
                checkXCollider(targetHitbox, collisionTarget, dt);

            return true;
        }


        /// <summary>
        /// Defines the current character's Y collider
        /// </summary>
        /// <param name="y">bottom, top or null </param>
        /// <param name="targetHitbox"></param>
        /// <param name="collisionTarget"></param>
        private void SetYCollider(Global.YDirections? y, RectangleF? targetHitbox, Entity? collisionTarget)
        {
            _curYColliderDirection = y;
            _yStickTarget = targetHitbox;
            _yStickEntity = collisionTarget;
        }


        /// <summary>
        /// Defines the current character's horizontal collider
        /// </summary>
        /// <param name="x">right, left or null</param>
        /// <param name="targetHitbox">The hitbox of the X collider</param>
        /// <param name="collisionTarget">The reference to the entity that the character is colliding with horizontally</param>
        private void SetXCollider(Global.XDirections? x, RectangleF? targetHitbox, Entity? collisionTarget)
        {
            _curXColliderDirection = x;
            _xStickTarget = targetHitbox;
            _xStickEntity = collisionTarget;
        }


        /// <summary>
        /// Returns a boolean which is used to tell the code whether or not to have the character check for collision
        /// </summary>
        /// <returns>boolean: default true</returns>
        public bool ShouldDoMove()
        {
            if (_yStickEntity is not null || _xStickEntity is not null) return true;
            if ((yVelocity == 0) && (xVelocity == 0)) return false; 
            return true;
        }


        /// <summary>
        /// set the Y velocity to go 1 if character is still in the middle of a jump and stop jumping
        /// </summary>
        public void StopJump() => yVelocity = (yVelocity < 0) ? 1 : yVelocity; 


        public void CheckPlatformCollision(Entity target, double dt)
        {
            if (!ShouldDoMove()) return;

            bool colliderFound = IsCollidingWith(target, dt);
            if (!colliderFound)return;

            if (_yStickTarget is not null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if (_curYColliderDirection == Global.YDirections.top)
                {
                    LocationY = _yStickTarget.Value.Bottom + 1;
                    yVelocity = Math.Max(yVelocity,0);
                }

                // adds coyote time if there is a platform below the character, and sets the Y value of the character to the platform
                else if (_curYColliderDirection == Global.YDirections.bottom)
                {
                    _coyoteTimeS = 0.05;
                    LocationY = _yStickTarget.Value.Y - Height + 1;
                    yVelocity = Math.Min(yVelocity, 0);
                }
            }


            if (_xStickTarget is not null)
            {
                if (_curXColliderDirection == Global.XDirections.right)
                {
                    LocationX = _xStickTarget.Value.Left - this.Width;
                    xVelocity = Math.Min(0, xVelocity);
                }
                else if (_curXColliderDirection == Global.XDirections.left)
                {
                    LocationX = _xStickTarget.Value.Right;
                    xVelocity = Math.Max(0, xVelocity);
                }
            }
        }


        private void DoGravTick(double dt)
        {
            // if there is no floor beneath -> gravity occurs
            if (_curYColliderDirection != Global.YDirections.bottom)
            {
                IsOnFloor = false;
                yVelocity += _GRAVITY*dt;
            }

            // Coyote time ticks down 
            if (_coyoteTimeS > 0)
            {
                _coyoteTimeS -= dt;
                IsOnFloor = true; // allows for more responsive jumping
            }
        }


        public void DoJump() => yVelocity = -_JUMP_VELOCITY; 


        /// <summary>
        /// Moves the character according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        internal void MoveCharacter(double dt, Global.XDirections? direction, double scrollVelocity)
        {
            DoGravTick(dt);
            checkInBoundary();
            _lastXDirection = direction;
            if (movementEndlagS > 0) direction = null;

            _curXAccel=0;

            if (direction is not null)
            {
                FacingDirection = direction.Value;
                if (!_knockedBack)
                {
                    if (direction == Global.XDirections.left) _curXAccel = -_xAccel;
                    if (direction == Global.XDirections.right) _curXAccel = _xAccel;
                }
            }

            xVelocity += _curXAccel;

            CheckAllPlatformCollision(dt); // check collider after xVelocity as been decided as checkXCollider uses xVelocity

            if (Math.Abs(scrollVelocity) > 0) ScrollChar(dt, scrollVelocity);
            else Location = new PointF( 
                    Location.X + (float)(xVelocity * dt * Global.BaseScale), 
                    Location.Y + (float)(yVelocity * dt * Global.BaseScale)); 

            if (Math.Abs(xVelocity) <= _maxXVelocity) _knockedBack = false;
            if (xVelocity == 0) return;

            if (!_knockedBack) clampSpeed(_maxXVelocity, _TERMINAL_VELOCITY);
            else clampSpeed(_xKnockbackVelocity, _yKnockbackVelocity);

            if (_curXAccel == 0 || _knockedBack) decelerate(dt);
        }

        // stops the character going above the screen and kills when going below screen.
        protected bool checkInBoundary() 
        {
             if (Location.Y <= 0)
             {
                 LocationY = 1;
                 yVelocity = Math.Max(yVelocity, 0);
             }
             else if (Location.Y > Global.LevelSize.Height) return false;
             return true;
        }

        protected void clampSpeed(int maxXSpeed, int maxYSpeed) 
        {
            xVelocity = Math.Min( Math.Abs(xVelocity), maxXSpeed) * Math.Sign(xVelocity);
            yVelocity = Math.Min( Math.Abs(yVelocity), maxYSpeed) * Math.Sign(yVelocity);
        }

        private void decelerate(double dt)
        {
            //double deceleration = (_knockedBack) ? (FRICTION*0.9): FRICTION;
            double deceleration = FRICTION*dt;
            double xSpeed = Math.Abs(xVelocity);
            if (xSpeed > deceleration)  xVelocity = Math.CopySign(xSpeed-deceleration, xVelocity);
            else xVelocity = 0; 
        }

        public void ScrollChar(double dt, double scrollVelocity)
        {
            if (_yStickEntity is not null)  CheckPlatformCollision(_yStickEntity, dt);
            float _baseScale = Global.BaseScale;

            if (this is Player) 
            {
                Location = new PointF(
                        Location.X,
                        Location.Y + (float)(yVelocity * dt * _baseScale)
                        );
                return;
            }

            if (_xStickEntity is not null) CheckPlatformCollision(_xStickEntity, dt);

            Location = new PointF(
                    Location.X + (float)(xVelocity * dt) * _baseScale, 
                    Location.Y + (float)(yVelocity * dt) * _baseScale
                    ); 
        }

        
        /*    
        public string CollisionDebugX()
        {
            string s = "null";
            if (_curXColliderDirection == Global.XDirections.left) s= "left ";
            else if (_curXColliderDirection == Global.XDirections.right) s="right ";

            if (_xStickEntity is not null) s += _xStickEntity.ToString();
            if (_xStickTarget is not null) s += _xStickTarget.ToString();
            
            return s;
        }
        public string CollisionDebugY()
        {
            if (_curYColliderDirection == Global.YDirections.top) return "top";
            else if (_curYColliderDirection == Global.YDirections.bottom) return "bottom";
            return "null";
        }
        */


        protected virtual void DoDamage(Attacks atk, int xVel, int yVel, int xDampen=0, int yDampen=0)
        {
            if (iFrames > 0) return;
            _hp -= atk.AtkDmg;
            PointF sourceCenter = atk.Parent.Center;
            if (sourceCenter.X - this.Center.X > 0) xVel *= -1;
            if (sourceCenter.Y - this.Center.Y > 0) yVel *= -1;
            ApplyKnockBack(xVel, yVel, xDampen, yDampen); 
        }

        protected virtual void DoDamage(Projectile sourceProjectile, int xDampen=0, int yDampen=0)
        {
            if (iFrames>0) return;
            _hp -= sourceProjectile.Dmg;
            int[] knockBackVelocites = sourceProjectile.calculateKnockback(this.Center);
            ApplyKnockBack(knockBackVelocites[0], knockBackVelocites[1], xDampen, yDampen); 
        }

        public void ApplyKnockBack(int xVel, int yVel, int xDampen, int yDampen)
        {
            xVelocity = Math.CopySign(Math.Max(0, Math.Abs(xVel)-xDampen), xVel);
            yVelocity = Math.CopySign(Math.Max(0, Math.Abs(yVel)-yDampen), yVel);
            _xKnockbackVelocity = Math.Max(Math.Abs(xVel), _maxXVelocity);
            _yKnockbackVelocity = Math.Max(Math.Abs(yVel), _maxYVelocity);
            _knockedBack = true;
        }

        public void CheckAllPlatformCollision(double dt)
        { foreach (Platform p in Platform.ActivePlatformList) CheckPlatformCollision(p, dt); }

        public override void Reset()
        {
            base.Reset();
            xVelocity = 0;
            yVelocity = 0;
            endlagS = 0;
            iFrames = 0;
            Hp = MaxHp;
        }

        public virtual Bitmap NextAnimFrame()
        {
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

            if (yVelocity == 0 || IsOnFloor)
            {
                if (curXAccel == 0) return idleAnim.NextFrame(FacingDirection);
                return runAnim.NextFrame(FacingDirection);
            }
            return idleAnim.NextFrame(FacingDirection);
        }

        public override void CheckActive() => throw new Exception("CheckActive not implemented in a character");
    }
}
