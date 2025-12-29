namespace drawedOut
{
    internal class Projectile : Entity
    {
        public static List<Projectile> ProjectileList = new List<Projectile>();
        public int KnockbackSpeed { get => _knockbackSpeed; }
        public int Dmg { get => _dmg; }

        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static List<Projectile> disposedProjectiles = new List<Projectile>();
        private Entity _parent;
        private float 
            _xVelocity, 
            _yVelocity;
        private readonly float _velocity;
        private readonly int _dmg, _knockbackSpeed;

        /// <summary>
        /// creates a projectile with the following parameters
        /// </summary>
        /// <param name="origin">top-left of the projectile hitbox</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="velocity"></param>
        /// <param name="target"></param>
        public Projectile (PointF origin, int width, int height, float velocity, PointF target, Entity parent, int dmg=1, int knockback=1000)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _parent = parent;
            _velocity = velocity;
            _knockbackSpeed = knockback;
            calculateVelocities(target);
            ProjectileList.Add(this);
        }

        public Projectile (PointF origin, int width, int height, float velocity, double angle, double xDiff, double yDiff, Entity parent, int dmg=1, int knockback=1000)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _parent = parent;
            _velocity = velocity;
            _knockbackSpeed = knockback;
            _xVelocity = (float)Math.Cos(angle) * _velocity * Math.Sign(xDiff);
            _yVelocity = (float)Math.Sin(angle) * _velocity * Math.Sign(yDiff);
            ProjectileList.Add(this);
        }


        /// <summary>
        /// updates the projectile's location based on the velocity and angle (x and y velocities)
        /// </summary>
        public void MoveProjectile(double deltaTime)
        {
            float xVelocity = (float)(_xVelocity*deltaTime);
            float yVelocity = (float)(_yVelocity*deltaTime);

            Location = new PointF(Location.X+xVelocity, Location.Y+yVelocity);
        }



        // calculates the required y and x velocities for the projectile
        // angle and distances used for debugging
        private void calculateVelocities(PointF target)
        {
            float xDiff = target.X - Location.X;
            float yDiff = target.Y - Location.Y;

            float velocityAngle = (float)Math.Abs(Math.Atan(yDiff/xDiff)); 

            _xVelocity = (float)Math.Cos(velocityAngle) * _velocity * Math.Sign(xDiff);
            _yVelocity = (float)Math.Sin(velocityAngle) * _velocity * Math.Sign(yDiff);
        }

        /// <summary>
        /// Calculates the x and y knockback velocities that should be applied by the projectile
        /// </summary>
        /// <param name="target"> the location of the target of the projectile</param>
        /// <returns> integer array [xKnockback, yKnockback] </returns>
        public int[] calculateKnockback(PointF target)
        {
            float xDiff = target.X - Center.X;
            float yDiff = target.Y - Center.Y;
            float knockbackAngle = (float)(Math.Abs(Math.Atan(yDiff/xDiff) + Math.PI/9));
            double xKnockback = Math.CopySign( Math.Cos(knockbackAngle)*_knockbackSpeed, xDiff );
            double yKnockback = -Math.Sin(knockbackAngle)*_knockbackSpeed;
            return [(int)xKnockback, (int)yKnockback];
        }

        /// <summary>
        /// makes the projectile rebound off [target]
        /// </summary>
        public void Rebound(Entity target, double dt)
        {
            _parent = target;
            _xVelocity = -_xVelocity;
            _yVelocity = -_yVelocity;
            this.Center = new PointF(Center.X + (float)(_xVelocity*dt), Center.Y + (float)(_yVelocity*dt));
        }

        public void Dipose() => disposedProjectiles.Add(this); 

        public override void CheckActive() { if (this.DistToMid > Global.EntityLoadThreshold) Dipose(); }

        public static void CheckProjectileCollisions(double dt, Form form, Player playerBox, ParallelOptions threadSettings)
        {
            if (ProjectileList.Count == 0) return;
            Parallel.ForEach(Projectile.ProjectileList, threadSettings, bullet =>
            {
                bullet.MoveProjectile(dt);
                PointF bLoc = bullet.Center;

                if (disposedProjectiles.Contains(bullet)) return;

                foreach (Platform p in Platform.ActivePlatformList)
                {
                    if (!(p.Hitbox.IntersectsWith(bullet.Hitbox))) continue;

                    disposedProjectiles.Add(bullet);
                    return;
                }
                foreach (Enemy e in Enemy.ActiveEnemyList)
                {
                    if (e == bullet._parent) continue;
                    if (!(e.Hitbox.IntersectsWith(bullet.Hitbox))) continue;

                    disposedProjectiles.Add(bullet);
                    e.DoDamage(bullet._dmg, bullet);
                    return;
                }

                if (!bullet.Hitbox.IntersectsWith(form.ClientRectangle))
                {
                    disposedProjectiles.Add(bullet);
                    return;
                }

                // Return if bullet not touching player
                if (!playerBox.Hitbox.IntersectsWith(bullet.Hitbox)) return;

                bool shouldDispose = playerBox.CheckParrying(bullet, dt);
                if (shouldDispose) disposedProjectiles.Add(bullet);
            });

            if (disposedProjectiles.Count == 0) return;
            foreach (Projectile p in disposedProjectiles)
            {
                ProjectileList.Remove(p);
                EntityList.Remove(p);
            }

            disposedProjectiles.Clear();
        }
    }
}
