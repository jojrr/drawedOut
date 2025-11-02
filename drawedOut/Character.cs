namespace drawedOut
{
    internal class Character : Entity
    {
        private int _xAccel;
        protected double xVelocity { get; private set; }
        protected double yVelocity { get; private set; }

        public bool IsMoving { get; protected set; }
        public bool IsOnFloor { get; protected set; }
        public Global.XDirections FacingDirection { get; private set; }

        public RectangleF AnimRect 
        {
            get 
            {
                float sqrSize = Math.Max(Hitbox.Width*1.1F, Hitbox.Height*1.1F);
                SizeF s = new SizeF(sqrSize, sqrSize);
                PointF p = new PointF(Center.X - s.Width/2, Hitbox.Bottom - s.Height);
                return new RectangleF(p,s);
            }
        }

        private readonly int
            _maxXVelocity = 600,
            _terminalVelocity = 2300,
            _gravity = 4000,
            _jumpVelocity = 1500;

        private int _hp;
        public int Hp 
        { 
            get => _hp;
            protected set
            {
                if (value > _maxHp) 
                    _hp = _maxHp;
                else
                    _hp = value;
            }
        }
        private int _maxHp;
        public int MaxHp 
        { 
            get => _maxHp; 
            private set
            {
                if (value <= 0) throw new Exception("MaxHp must be > 0");
                _maxHp = value; 
            }
        }

        private double _coyoteTimeS;
        protected double endlagS = 0;

        private RectangleF? _xStickTarget, _yStickTarget;

        private Entity? _xStickEntity, _yStickEntity;

        public Global.YDirections? CurYColliderDirection = null;
        public Global.XDirections? CurXColliderDirection = null;

        /// <summary>
        /// Initalises a "character" (entity with velocity and gravity)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hp"> 
        ///
        protected Character(Point origin, int width, int height, int hp, int xAccel=100) 
            : base(origin: origin, width: width, height: height)
        {
            xVelocity = 0;
            yVelocity = 0;
            MaxHp = hp;
            Hp = hp;
            IsMoving = false;
            IsOnFloor = false;
            _terminalVelocity = (int)Global.BaseScale * _terminalVelocity;
            _maxXVelocity = (int)Global.BaseScale * _maxXVelocity;
            _jumpVelocity = (int)Global.BaseScale * _jumpVelocity;
            _gravity = (int)Global.BaseScale * _gravity;
            _xAccel = xAccel * (int)Global.BaseScale;
        }


        /// <summary>
        /// checks the Y direction for collision with platforms
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity" that is being checked </param>
        private void checkYCollider(RectangleF targetHitbox, Entity collisionTarget)
        {
            // Checks if there is a platform below
            if (Center.Y <= collisionTarget.Center.Y)
            {
                // zeros the velocity if the player was previously not on the floor when landing (prevents fling)
                if (!IsOnFloor) yVelocity = Math.Min(yVelocity, 0); 
                SetYCollider(Global.YDirections.bottom, targetHitbox, collisionTarget);
            }
            // Checks if there is a platform above the player
            //else if ((Center.Y >= collisionTarget.Center.Y + targetHitbox.Height / 2 - Height / 4) && (yVelocity < 0))
            else if (collisionTarget.Center.Y < Hitbox.Top)
                SetYCollider(Global.YDirections.top, targetHitbox, collisionTarget);
        }

        /// <summary>
        /// checks the X direction for collision with entities (mostly platforms)
        /// </summary>
        /// <param name="collisionTarget"> the <see cref="Entity"/> that is being checked </param>
        private void checkXCollider(RectangleF targetHitbox, Entity collisionTarget)
        {
            if (Center.X < targetHitbox.Left)
                // Checks if there is a platform to the left/right of the player
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
            CurYColliderDirection = y;
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
            CurXColliderDirection = x;
            _xStickTarget = targetHitbox;
            _xStickEntity = collisionTarget;
        }


        /// <summary>
        /// Returns a boolean which is used to tell the code whether or not to have the player check for collision
        /// </summary>
        /// <returns>boolean: default true</returns>
        public bool ShouldDoMove()
        {
            if (CurYColliderDirection != Global.YDirections.bottom) return true;
            if ((yVelocity == 0) && (xVelocity == 0)) return false; 
            return true;
        }


        /// <summary>
        /// set the Y velocity to go 1 if character is still in the middle of a jumo and stop jumping
        /// </summary>
        public void StopJump() => yVelocity = (yVelocity < 0) ? 1 : yVelocity; 


        public void CheckPlatformCollision(Entity target)
        {
            RectangleF? targetHitbox = IsCollidingWith(target);

            if (targetHitbox is null) return;

            if (_yStickTarget is not null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if (CurYColliderDirection == Global.YDirections.top)
                {
                    LocationY = _yStickTarget.Value.Bottom + 1;
                    yVelocity = 0;
                }

                // adds coyote time if there is a platform below the player, and sets the Y value of the player to the platform
                else if (CurYColliderDirection == Global.YDirections.bottom)
                {
                    _coyoteTimeS = 0.05;
                    LocationY = _yStickTarget.Value.Y - Height + 1;
                    yVelocity = Math.Min(yVelocity, 0);
                }
            }


            if (_xStickTarget is not null)
            {
                if (CurXColliderDirection == Global.XDirections.right)
                {
                    LocationX = _xStickTarget.Value.Left - this.Width - 1;
                    xVelocity = Math.Min(0, xVelocity);
                }
                else if (CurXColliderDirection == Global.XDirections.left)
                {
                    LocationX = _xStickTarget.Value.Right + 1;
                    xVelocity = Math.Max(0, xVelocity);
                }
            }
        }


        private void DoGravTick(double dt)
        {
            // if there is no floor beneath -> gravity occurs
            if (CurYColliderDirection != Global.YDirections.bottom)
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
        public void MoveCharacter(double dt, Global.XDirections? direction, bool doScroll)
        {
            DoGravTick(dt);

            // stops the player going above the screen
            if (Location.Y <= 0)  
            {
                LocationY = 1;
                yVelocity = 0;
            }

            double xAccel=0;

            if (direction == Global.XDirections.left) xAccel = -_xAccel;
            if (direction == Global.XDirections.right) xAccel = _xAccel;

            xVelocity += xAccel;

            if (doScroll)
            {
                if (_yStickEntity != null)  CheckPlatformCollision(_yStickEntity); 
                Location = new PointF(Location.X, Location.Y + (float)(yVelocity * dt));
            }
            else Location = new PointF(Location.X + (float)(xVelocity * dt), Location.Y + (float)(yVelocity * dt)); 

            if (xVelocity == 0) return;

            xVelocity = Math.Min(Math.Abs(xVelocity), _maxXVelocity) * Math.Sign(xVelocity); // clamp player speed

            if (xVelocity > 0) FacingDirection = Global.XDirections.right;
            if (xVelocity < 0) FacingDirection = Global.XDirections.left;
            // if not moving horizontally -> gradually decrease horizontal velocity
            if (xAccel == 0) 
            {
                if (Math.Abs(xVelocity) > 1)  xVelocity -= xVelocity * (15*dt); 
                else xVelocity = 0; 
            }
        }
        
        public string CollisionDebugX()
        {
            if (CurXColliderDirection == Global.XDirections.left) return ($"left {(_xStickEntity==_yStickEntity).ToString()}");
            else if (CurXColliderDirection == Global.XDirections.right) return ($"right {(_xStickEntity==_yStickEntity).ToString()}");
            return "null";
        }
            
        public string CollisionDebugY()
        {
            if (CurYColliderDirection == Global.YDirections.top) return "top";
            else if (CurYColliderDirection == Global.YDirections.bottom) return "bottom";
            return "null";
        }


        public void DoDamage(int dmg)
        {
            if (this is Player) { throw new Exception("Player should call DoDamage that takes hpBarUI as param"); }
            Hp -= dmg;
        }

        public override void CheckActive(){}

        public virtual Bitmap? NextAnimFrame() => null;
    }
}
