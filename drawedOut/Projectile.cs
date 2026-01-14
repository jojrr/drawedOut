namespace drawedOut
{
    internal class Projectile : Entity
    {
        public static IReadOnlyCollection<Projectile> ProjectileList => _projectileList;
        public int Dmg { get => _dmg; }
        public bool IsLethal { get; private set; }

        private static HashSet<Projectile> _projectileList = new HashSet<Projectile>();
        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static HashSet<Projectile> _disposedProjectiles = new HashSet<Projectile>();
        private readonly float _velocity;
        private readonly int _dmg, _knockbackSpeed;
        private Entity _parent;
        private float 
            _xVelocity, 
            _yVelocity;

        /// <summary>
        /// creates a projectile with the following parameters
        /// </summary>
        /// <param name="origin">top-left of the projectile hitbox</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="velocity"></param>
        /// <param name="target"></param>
        public Projectile (PointF origin, int width, int height, float velocity, PointF target, Entity parent, 
                int dmg=1, int knockback=800, bool isLethal=false)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _parent = parent;
            _velocity = velocity;
            _knockbackSpeed = knockback;
            calculateVelocities(target);
            _projectileList.Add(this);
            IsLethal = isLethal;
            Center = origin;
        }

        public Projectile (PointF origin, int width, int height, float velocity, double angle, double xDiff, double yDiff, Entity parent,
                int dmg=1, int knockback=800, bool isLethal=false)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _parent = parent;
            _velocity = velocity;
            _knockbackSpeed = knockback;
            _xVelocity = (float)Math.Cos(angle) * _velocity * Math.Sign(xDiff);
            _yVelocity = (float)Math.Sin(angle) * _velocity * Math.Sign(yDiff);
            _projectileList.Add(this);
            IsLethal = isLethal;
            Center = origin;
        }

        public void ToggleLethal(bool? isLethal) => IsLethal = (isLethal is null) ? !IsLethal : isLethal.Value;

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
            float knockbackAngle = (float)(Math.Abs(Math.Atan(_yVelocity/_xVelocity) ));
            double xKnockback = Math.CopySign( Math.Cos(knockbackAngle)*_knockbackSpeed, _xVelocity );
            double yKnockback = Math.CopySign( Math.Sin(knockbackAngle)*_knockbackSpeed, _yVelocity );
            return [(int)xKnockback, (int)yKnockback];
        }

        /// <summary>
        /// makes the projectile rebound off [target]
        /// </summary>
        public void Rebound(double dt, Entity? target)
        {
            if (target is not null) _parent = target;
            _xVelocity *= -1;
            _yVelocity *= -1;
            this.Center = new PointF(Center.X + (float)(_xVelocity*dt), Center.Y + (float)(_yVelocity*dt));
        }

        public void Dipose() => _disposedProjectiles.Add(this); 

        public override void CheckActive() { if (this.DistToMid > Global.EntityLoadThreshold) Dipose(); }


        public new static void ClearAllLists()
        {
            _projectileList.Clear();
            _disposedProjectiles.Clear();
        }


        public static void CheckProjectileCollisions(double dt, Form form, Player playerBox, ParallelOptions threadSettings)
        {
            if (_projectileList.Count == 0) return;
            Parallel.ForEach(Projectile._projectileList, threadSettings, bullet =>
            {
                bullet.MoveProjectile(dt);
                PointF bLoc = bullet.Center;

                if (_disposedProjectiles.Contains(bullet)) return;

                foreach (Platform p in Platform.ActivePlatformList)
                {
                    if (!(p.Hitbox.IntersectsWith(bullet.Hitbox))) continue;

                    _disposedProjectiles.Add(bullet);
                    return;
                }

                if (!bullet.Hitbox.IntersectsWith(form.ClientRectangle))
                {
                    _disposedProjectiles.Add(bullet);
                    return;
                }

                if (bullet._parent is Player)
                {
                    foreach (Enemy e in Enemy.ActiveEnemyList)
                    {
                        if (!(e.Hitbox.IntersectsWith(bullet.Hitbox))) continue;

                        _disposedProjectiles.Add(bullet);
                        e.DoDamage(bullet);
                        return;
                    }
                }
                else
                {
                    // Return if bullet not touching player
                    if (!playerBox.Hitbox.IntersectsWith(bullet.Hitbox)) return;
                    if (playerBox.CheckParrying(bullet, dt)) _disposedProjectiles.Add(bullet);
                }
            });

            if (_disposedProjectiles.Count == 0) return;
            foreach (Projectile p in _disposedProjectiles)
            {
                _projectileList.Remove(p);
                p.Delete();
            }

            _disposedProjectiles.Clear();
        }
    }
}
