namespace drawedOut
{
    internal class Character : Entity
    {
        public Global.XDirections FacingDirection { get; protected set; }
        public bool IsOnFloor { get; protected set; }
        public bool IsHit { get; protected set; }

        protected AnimationPlayer? idleAnim { get => _idleAnim; private set => _idleAnim = value; }
        protected AnimationPlayer? runAnim { get => _runAnim; private set => _runAnim = value; }
        protected double xVelocity { get; private set; }
        protected double yVelocity { get; private set; }
        protected int curXAccel { get => _curXAccel; }
        protected Attacks? curAttack;
        protected double endlagS = 0;

        private const int GRAVITY = 4000, FRICTION = 84;
        private Global.XDirections? _curXColliderDirection = null;
        private Global.YDirections? _curYColliderDirection = null;
        private AnimationPlayer? _idleAnim, _runAnim;
        private RectangleF? _xStickTarget, _yStickTarget;
        private Entity? _xStickEntity, _yStickEntity;
        private int _maxHp, _hp, _curXAccel, _xKnockbackVelocity;
        private bool _knockedBack = false;
        private double _coyoteTimeS;
        private readonly int
            _terminalVelocity = 2300,
            _jumpVelocity = 1500,
            _maxXVelocity,
            _gravity,
            _xAccel;

        /// <summary>
        /// Initalises a "character" (entity with velocity and gravity)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hp"> 
        ///
        protected Character(Point origin, int width, int height, int hp, int xAccel, int maxXVelocity)
            : base(origin: origin, width: width, height: height)
        {
            IsOnFloor = false;
            xVelocity = 0;
            yVelocity = 0;
            _hp = hp;
            _maxHp = hp;
            _xAccel = (int)(xAccel * Global.BaseScale);
            _gravity = (int)(Global.BaseScale * GRAVITY);
            _maxXVelocity = (int)(Global.BaseScale * maxXVelocity);
            _jumpVelocity = (int)(Global.BaseScale * _jumpVelocity);
            _terminalVelocity = (int)(Global.BaseScale * _terminalVelocity);
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

        public static void TickEndlags(double dt)
        {
            Player.TickEndlagS(dt);
            foreach (Enemy e in Enemy.ActiveEnemyList) 
            {
                if (e.endlagS <= 0) continue;
                e.endlagS -= dt; 
                e.IsHit = false;
            }
        }

        /// <summary>
        /// checks the Y direction for collision with platforms
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity"/> that is being checked </param>
        private void checkYCollider(RectangleF targetHitbox, Entity collisionTarget)
        {
            // Checks if there is a platform below
            if (Hitbox.Bottom <= targetHitbox.Bottom)
            {
                // zeros the velocity if the player was previously not on the floor when landing (prevents fling)
                if (!IsOnFloor) yVelocity = Math.Min(yVelocity, 0); 
                SetYCollider(Global.YDirections.bottom, targetHitbox, collisionTarget);
            }
            // Checks if there is a platform above the player
            else if (targetHitbox.Top < Hitbox.Top)
                SetYCollider(Global.YDirections.top, targetHitbox, collisionTarget);
        }

        /// <summary>
        /// checks the X direction for collision with entities (mostly platforms)
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity"/> that is being checked </param>
        private void checkXCollider(RectangleF targetHitbox, Entity collisionTarget)
        {
            if (Center.X < targetHitbox.Left)
            {
                if (_xStickEntity is null && Center.Y > targetHitbox.Y) { xVelocity = 0; }
                // character is on the right of the hitbox
                SetXCollider(Global.XDirections.right, targetHitbox, collisionTarget); 
            }
            else if (Center.X > targetHitbox.Right)
            {
                if (_xStickEntity is null && Center.Y > targetHitbox.Y) { xVelocity = 0; }
                // character is on the left of the hitbox
                SetXCollider(Global.XDirections.left, targetHitbox, collisionTarget); 
            }
        }

        /// <summary>
        /// Checks if the target's hitbox is colliding with this entity's hitbox.<br/>
        /// Returned position is relative to this Entity.<br/>
        /// </summary>
        /// <param name="collisionTarget">The target to check for collision with</param>
        /// <returns><see cref="Rectangle"/>: the collisionTarget's hitbox</returns>
        private RectangleF? IsCollidingWith(Entity collisionTarget)
        {
            RectangleF targetHitbox = collisionTarget.Hitbox;

            // sets collision to null if not longer colliding with the previously colliding hitbox
            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (collisionTarget == _xStickEntity)  SetXCollider(null, null, null); 
                if (collisionTarget == _yStickEntity)  SetYCollider(null, null, null); 
                return null;
            }

            // if this' center is between the left and the right of the hitbox 
            if (targetHitbox.Right > Center.X && Center.X > targetHitbox.Left)
                checkYCollider(targetHitbox, collisionTarget);

            if ((_xStickEntity == _yStickEntity) && IsOnFloor) // Stops the player from bugging on corners
                SetXCollider(null, null, collisionTarget);
            else
                checkXCollider(targetHitbox, collisionTarget);


            return targetHitbox;
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
        /// <param name="collisionTarget">The reference to the entity that the player is colliding with horizontally</param>
        private void SetXCollider(Global.XDirections? x, RectangleF? targetHitbox, Entity? collisionTarget)
        {
            _curXColliderDirection = x;
            _xStickTarget = targetHitbox;
            _xStickEntity = collisionTarget;
        }


        /// <summary>
        /// Returns a boolean which is used to tell the code whether or not to have the player check for collision
        /// </summary>
        /// <returns>boolean: default true</returns>
        public bool ShouldDoMove()
        {
            if (_yStickEntity is not null || _xStickEntity is not null) return true;
            if ((yVelocity == 0) && (xVelocity == 0)) return false; 
            return true;
        }


        /// <summary>
        /// set the Y velocity to go 1 if character is still in the middle of a jumo and stop jumping
        /// </summary>
        public void StopJump() => yVelocity = (yVelocity < 0) ? 1 : yVelocity; 


        public void CheckPlatformCollision(Entity target)
        {
            if (!ShouldDoMove()) return;

            RectangleF? targetHitbox = IsCollidingWith(target);

            if (targetHitbox is null) return;

            if (_yStickTarget is not null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if (_curYColliderDirection == Global.YDirections.top)
                {
                    LocationY = _yStickTarget.Value.Bottom + 1;
                    yVelocity = 0;
                }

                // adds coyote time if there is a platform below the player, and sets the Y value of the player to the platform
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
                    LocationX = _xStickTarget.Value.Left - this.Width - 1;
                    xVelocity = Math.Min(0, xVelocity);
                }
                else if (_curXColliderDirection == Global.XDirections.left)
                {
                    LocationX = _xStickTarget.Value.Right + 1;
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
                yVelocity += _gravity*dt;

                // Terminal velocity -> only applies downwards
                if (yVelocity > 0) yVelocity = Math.Min(yVelocity, _terminalVelocity); 
            }

            // Coyote time ticks down 
            if (_coyoteTimeS > 0)
            {
                _coyoteTimeS -= dt;
                IsOnFloor = true; // allows for more responsive jumping
            }
        }


        public void DoJump() => yVelocity = -_jumpVelocity; 


        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter(double dt, Global.XDirections? direction, double scrollVelocity)
        {
            DoGravTick(dt);

            // stops the player going above the screen
            if (Location.Y <= 0 && yVelocity < 0)
            {
                LocationY = 1;
                yVelocity = 0;
            }
            else if (Location.Y > Global.LevelSize.Height) this.Hp = 0;

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
            if (Math.Abs(scrollVelocity) > 0) ScrollChar(dt, scrollVelocity);
            else Location = new PointF(
                    Location.X + (float)(xVelocity * dt), Location.Y + (float)(yVelocity * dt)
                    ); 

            if (xVelocity == 0) return;

            if (!_knockedBack) clampSpeed(_maxXVelocity);
            else clampSpeed(_xKnockbackVelocity);

            if (_curXAccel == 0 || _knockedBack) decelerate(dt);
            if (Math.Abs(xVelocity) <= _maxXVelocity) _knockedBack = false;
        }

        private void clampSpeed(int maxSpeed) => xVelocity = Math.Min(
                Math.Abs(xVelocity), maxSpeed) * Math.Sign(xVelocity);

        private void decelerate(double dt)
        {
            double deceleration = (_knockedBack) ? (FRICTION/2): FRICTION;
            deceleration = Math.Max(1.05, deceleration*dt);
            if (Math.Abs(xVelocity) > deceleration)  xVelocity /= deceleration;
            else xVelocity = 0; 
        }

        public void ScrollChar(double dt, double scrollVelocity)
        {
                if (_yStickEntity is not null)  CheckPlatformCollision(_yStickEntity); 

                if (this is Player) 
                {
                    Location = new PointF(
                            Location.X, 
                            Location.Y + (float)(yVelocity * dt)
                            );
                    return;
                }

                if (_xStickEntity is not null) CheckPlatformCollision(_xStickEntity);

                Location = new PointF(
                        Location.X + (float)(xVelocity * dt), 
                        Location.Y + (float)(yVelocity * dt)
                        ); 
        }

        /*
        public string CollisionDebugX()
        {
            if (_curXColliderDirection == Global.XDirections.left) return ($"left {(_xStickEntity==_yStickEntity).ToString()}");
            else if (_curXColliderDirection == Global.XDirections.right) return ($"right {(_xStickEntity==_yStickEntity).ToString()}");
            return "null";
        }
            
        public string CollisionDebugY()
        {
            if (_curYColliderDirection == Global.YDirections.top) return "top";
            else if (_curYColliderDirection == Global.YDirections.bottom) return "bottom";
            return "null";
        }
        */


        public void DoDamage(int dmg, Entity source)
        {
            if (this is Player) throw new Exception(
                    "Player should call DoDamage that takes hpBarUI as param"
                    );
            IsHit = true;
            _hp -= dmg;
            ApplyKnockBack(source); 
        }

        public void ApplyKnockBack(Entity source, int xSpeed = 1500, int ySpeed = 500)
        {
            xVelocity = (source.Center.X - this.Center.X < 0) ? xSpeed : -xSpeed;
            yVelocity = (source.Center.Y - this.Center.Y < 0) ? ySpeed : -ySpeed;
            _xKnockbackVelocity = xSpeed;
            _knockedBack = true;
        }


        public override void CheckActive(){}

        public virtual Bitmap? NextAnimFrame()
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
    }
}
