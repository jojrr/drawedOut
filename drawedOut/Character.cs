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

        public double xVelocity;
        public double yVelocity;
        public bool IsMoving = false;
        public bool IsOnFloor = false;
        public bool HasGravity;

        private const int TerminalVelocity = 130;
        private const int MaxXVelocity = 60;
        private const int jumpVelocity = -150;
        private double CoyoteTime;
        private const double Gravity = 67.42;

        private RectangleF? xStickTarget; 
        private RectangleF? yStickTarget;
        private Entity? xStickEntity;
        private Entity? yStickEntity;

        private RectangleF OverShootRec;

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
        /// <param name="xVelocity">default = 0</param>
        /// <param name="yVelocity">default = 0</param>
        /// <param name="flying">default = false</param>
        public Character(Point origin, int width, int height, double xVelocity = 0, double yVelocity = 0, bool flying = false)
            : base(origin: origin, width: width, height: height )
        {
            this.xVelocity = xVelocity;
            this.yVelocity = yVelocity;
            HasGravity = !flying;
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
            RectangleF targetHitbox = collisionTarget.GetHitbox();
            PointF targetCenter = collisionTarget.Center;

            // sets collision to null if not longer colliding with the previously colliding hitbox
            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (collisionTarget == xStickEntity) { SetXCollider(null, null, null); }
                if (collisionTarget == yStickEntity) { SetYCollider(null, null, null); }
            }
            else
            {
                // if this' center is between the left and the right of the hitbox 
                if ((Center.X < targetHitbox.Right) && (Center.X > targetHitbox.Left))
                {
                    // Checks if there is a platform below - considers overshoot
                    if ((Center.Y <= targetHitbox.Y) || (OverShootRec.IntersectsWith(targetHitbox) && (OverShootRec.Top < targetHitbox.Top)))
                    {
                        // zeros the velocity if the player was previously not on the floor when landing (prevents fling)
                        if (!IsOnFloor) { yVelocity = Math.Min(yVelocity, 0); }
                        SetYCollider(YColliders.bottom, targetHitbox, collisionTarget);
                    }
                    // Checks if there is a platform above the player
                    else if ((Center.Y >= targetCenter.Y + targetHitbox.Height / 2 - Height / 4) && (yVelocity < 0))
                    {
                        SetYCollider(YColliders.top, targetHitbox, collisionTarget);
                    }
                }

                if ((xStickEntity == yStickEntity) && IsOnFloor) // Stops the player from bugging on corners
                {
                    SetXCollider(null, null, collisionTarget);
                }
                else
                {

                    if (Center.X < targetHitbox.Left) // Checks if there is a platform to the left/right of the player
                    {
                        if ((xStickEntity == null) && (Center.Y > targetHitbox.Y)) { xVelocity = 0; }
                        SetXCollider(XColliders.right, targetHitbox, collisionTarget); // character is on the right of the hitbox
                    }
                    else if (Center.X > targetHitbox.Right)
                    {
                        if ((xStickEntity == null) && (Center.Y > targetHitbox.Y)) { xVelocity = 0; }
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
            yStickTarget = targetHitbox;
            yStickEntity = collisionTarget;
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
            xStickTarget = targetHitbox;
            xStickEntity = collisionTarget;
        }


        /// <summary>
        /// Returns a boolean which is used to tell the code whether or not to have the player check for collision
        /// </summary>
        /// <returns>boolean: default true</returns>
        public bool ShouldDoMove()
        {
            if (CurYColliderDirection == YColliders.bottom)
            {
                if ((yVelocity == 0) && (xVelocity == 0)) { return false; }
            }

            return true;
        }


        public void CheckPlatformCollision(Entity target)
        {
            RectangleF targetHitbox = IsCollidingWith(target);

            if (yStickTarget != null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if (CurYColliderDirection == YColliders.top)
                {
                    location.Y = yStickTarget.Value.Bottom + 1;
                    yVelocity = 0;
                }

                // adds coyote time if there is a platform below the player, and sets the Y value of the player to the platform
                else if (CurYColliderDirection == YColliders.bottom)
                {
                    CoyoteTime = 10; // 100ms (on 10ms timer)
                    location.Y = yStickTarget.Value.Y - Height + 1;
                    yVelocity = Math.Min(yVelocity, 0);
                }
            }



            if (xStickTarget != null)
            {
                if (CurXColliderDirection == XColliders.right)
                {
                    location.X = xStickTarget.Value.Left - this.Width + 1;
                    xVelocity = Math.Min(0, xVelocity);
                }
                else if (CurXColliderDirection == XColliders.left)
                {
                    location.X = xStickTarget.Value.Right - 1;
                    xVelocity = Math.Max(0, xVelocity);
                }
            }
        }


        private void doGravTick(double dt)
        {
            // if there is no floor beneath -> gravity occurs
            if (CurYColliderDirection != YColliders.bottom)
            {
                IsOnFloor = false;
                yVelocity += Gravity*dt;

                // Terminal velocity -> only applies downwards
                if (yVelocity > 0) { yVelocity = Math.Min(yVelocity, TerminalVelocity); }
            }

            // Coyote time ticks down 
            if (CoyoteTime > 0)
            {
                CoyoteTime -= dt*10;
                IsOnFloor = true; // allows for more responsive jumping
            }
        }


        public void doJump() { yVelocity = jumpVelocity; }


        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter(double dt, bool isScrolling = false)
        {
            SetOverShootRec();

            if (HasGravity) { doGravTick(dt); }

            // stops the player going above the screen
            if (Location.Y < 0) { yVelocity = -jumpVelocity/6; }

            if (isScrolling)
            {
                if (yStickEntity != null) { CheckPlatformCollision(yStickEntity); }
                UpdateLocation(Location.X, Location.Y + (int)yVelocity*dt);
            }
            else { UpdateLocation(Location.X + (int)xVelocity*dt, Location.Y + (int)yVelocity*dt); }


            if (xVelocity == 0)
                return;


            xVelocity = Math.Min(Math.Abs(xVelocity), MaxXVelocity) * Math.Sign(xVelocity); // stops the player from achieving lightspeed

            if (!IsMoving) // if not moving horizontally -> gradually decrease horizontal velocity
            {
                if (Math.Abs(xVelocity) > 0.01) { xVelocity *= 0.85; }
                else { xVelocity = 0; }
            }
        }

        /// <summary>
        /// creates a new rectangle to detect for overshoot above the player's current location.
        /// Rectangle is the size of the player (effectively doubling the player's height)
        /// Only used to detect overshoot incase the player clips into the ground.
        /// </summary>
        private void SetOverShootRec() { OverShootRec = new RectangleF(Location.X, Location.Y - Height, Width, Height); }

    }
}
