namespace drawedOut
{
    internal class Character : Entity
    {
        // list of all characters - [int: level][list: chunk][Character]
        // used for gametick
        
        public static List<Character> ActiveCharacters = new List<Character>();
        public static List<Character> InactiveCharacters = new List<Character>();

        public enum YColliders : int { bottom, top }
        public enum XColliders : int { right, left }

        private double _xVelocity=0, _yVelocity=0;
        private bool 
            _isMoving = false,
            _isOnFloor,
            _hasGravity;

        public bool IsMoving { get => _isMoving; protected set => _isMoving = value; }
        public bool IsOnFloor { get => _isOnFloor; protected set => _isOnFloor = value; }

        private const int 
            TERMINAL_VELOCITY = 130,
            MAX_X_VELOCITY = 60,
            JUMP_VELOCITY = -150;

        private const double GRAVITY = 67.42;
        private double _coyoteTime;

        private RectangleF? _xStickTarget, _yStickTarget;
        private RectangleF _overShootRec;

        private Entity? _xStickEntity, _yStickEntity;

        /// <summary>
        /// Array that stores the current collision state of this character.
        /// format [X, Y]
        /// </summary>
        public YColliders? CurYColliderDirection = null;
        public XColliders? CurXColliderDirection = null;


        /// <summary>
        /// Initalises a "character" (entity with velocity and gravity)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="LocatedLevel">The level that the character is located in</param>
        /// <param name="LocatedChunk">The chunk that the character is located in</param>
        public Character(Point origin, int width, int height)
            : base(origin: origin, width: width, height: height )
        {
            SetOverShootRec();
            InactiveCharacters.Add(this);
        }



        /// <summary>
        /// Checks if the target's hitbox is colliding with this entity's hitbox. 
        /// Returned position is relative to this Entity.
        /// CollisionState[0] and [1] is the assigned Y and X collision value respectively.
        /// </summary>
        /// <param name="collisionTarget"></param>
        /// <returns>Rectangle: the collisionTarget's hitbox</returns>
        private RectangleF IsCollidingWith(Entity collisionTarget)
        {
            RectangleF targetHitbox = collisionTarget.Hitbox;
            PointF targetCenter = collisionTarget.Center;

            // sets collision to null if not longer colliding with the previously colliding hitbox
            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (collisionTarget == _xStickEntity)  SetXCollider(null, null, null); 
                if (collisionTarget == _yStickEntity)  SetYCollider(null, null, null); 
            }
            else
            {
                // if this' center is between the left and the right of the hitbox 
                if ((Center.X < targetHitbox.Right) && (Center.X > targetHitbox.Left))
                {
                    // Checks if there is a platform below - considers overshoot
                    if ((Center.Y <= targetHitbox.Y) || (_overShootRec.IntersectsWith(targetHitbox) && (_overShootRec.Top < targetHitbox.Top)))
                    {
                        // zeros the velocity if the player was previously not on the floor when landing (prevents fling)
                        if (!IsOnFloor) yVelocity = Math.Min(yVelocity, 0); 
                        SetYCollider(YColliders.bottom, targetHitbox, collisionTarget);
                    }
                    // Checks if there is a platform above the player
                    else if ((Center.Y >= targetCenter.Y + targetHitbox.Height / 2 - Height / 4) && (yVelocity < 0))
                    {
                        SetYCollider(YColliders.top, targetHitbox, collisionTarget);
                    }
                }

                if ((_xStickEntity == _yStickEntity) && IsOnFloor) // Stops the player from bugging on corners
                    SetXCollider(null, null, collisionTarget);
                else
                {
                    if (Center.X < targetHitbox.Left) // Checks if there is a platform to the left/right of the player
                    {
                        if ((_xStickEntity == null) && (Center.Y > targetHitbox.Y)) { _xVelocity = 0; }
                        SetXCollider(XColliders.right, targetHitbox, collisionTarget); // character is on the right of the hitbox
                    }
                    else if (Center.X > targetHitbox.Right)
                    {
                        if ((_xStickEntity == null) && (Center.Y > targetHitbox.Y)) { _xVelocity = 0; }
                        SetXCollider(XColliders.left, targetHitbox, collisionTarget); // character is on the left of the hitbox
                    }

                }
            }

            return targetHitbox;

        }


        /// <summary>
        /// Defines the current character's Y collider
        /// </summary>
        /// <param name="y">bottom, top or null </param>
        /// <param name="targetHitbox"></param>
        /// <param name="collisionTarget"></param>
        public void SetYCollider(YColliders? y, RectangleF? targetHitbox, Entity? collisionTarget)
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
        private void SetXCollider(XColliders? x, RectangleF? targetHitbox, Entity? collisionTarget)
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
            if (CurYColliderDirection != YColliders.bottom) return true;
            if ((yVelocity == 0) && (_xVelocity == 0)) return false; 
            return true;
        }


        public void CheckPlatformCollision(Entity target)
        {
            RectangleF targetHitbox = IsCollidingWith(target);

            if (_yStickTarget != null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if (CurYColliderDirection == YColliders.top)
                {
                    LocationY = _yStickTarget.Value.Bottom + 1;
                    yVelocity = 0;
                }

                // adds coyote time if there is a platform below the player, and sets the Y value of the player to the platform
                else if (CurYColliderDirection == YColliders.bottom)
                {
                    _coyoteTime = 10; // 100ms (on 10ms timer)
                    LocationY = _yStickTarget.Value.Y - Height + 1;
                    yVelocity = Math.Min(yVelocity, 0);
                }
            }



            if (_xStickTarget != null)
            {
                if (CurXColliderDirection == XColliders.right)
                {
                    LocationX = _xStickTarget.Value.Left - this.Width + 1;
                    _xVelocity = Math.Min(0, _xVelocity);
                }
                else if (CurXColliderDirection == XColliders.left)
                {
                    LocationX = _xStickTarget.Value.Right - 1;
                    _xVelocity = Math.Max(0, _xVelocity);
                }
            }
        }


        private void DoGravTick(double dt)
        {
            // if there is no floor beneath -> gravity occurs
            if (CurYColliderDirection != YColliders.bottom)
            {
                IsOnFloor = false;
                yVelocity += GRAVITY*dt;

                // Terminal velocity -> only applies downwards
                if (yVelocity > 0) yVelocity = Math.Min(yVelocity, TERMINAL_VELOCITY); 
            }

            // Coyote time ticks down 
            if (_coyoteTime > 0)
            {
                _coyoteTime -= dt*10;
                IsOnFloor = true; // allows for more responsive jumping
            }
        }


        public void DoJump() => yVelocity = JUMP_VELOCITY; 


        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter(double acceleration, double dt, bool isScrolling = false)
        {
            SetOverShootRec();

            if (_hasGravity) DoGravTick(dt);

            // stops the player going above the screen
            if (Location.Y < 0)  yVelocity = -JUMP_VELOCITY/6; 

            _xVelocity += accleration;

            if (isScrolling)
            {
                if (_yStickEntity != null)  CheckPlatformCollision(_yStickEntity); 
                Location = new PointF(Location.X, Location.Y + (float)(yVelocity * dt));
            }
            else Location = new PointF(Location.X + (float)(_xVelocity * dt), Location.Y + (float)(yVelocity * dt)); 

            if (_xVelocity == 0) return;

            _xVelocity = Math.Min(Math.Abs(_xVelocity), MAX_X_VELOCITY) * Math.Sign(_xVelocity); // stops the player from achieving lightspeed

            if (accleration == 0) // if not moving horizontally -> gradually decrease horizontal velocity
            {
                if (Math.Abs(_xVelocity) > 0.01)  _xVelocity *= 0.85; 
                else _xVelocity = 0; 
            }
        }

        /// <summary>
        /// creates a new rectangle to detect for overshoot above the player's current location.
        /// Rectangle is the size of the player (effectively doubling the player's height)
        /// Only used to detect overshoot incase the player clips into the ground.
        /// </summary>
        private void SetOverShootRec() => _overShootRec = new RectangleF(Location.X, Location.Y - Height, Width, Height); 

    }
}
