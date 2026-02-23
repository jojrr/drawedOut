namespace drawedOut
{
    internal class Projectile : Entity
    {
        public static IReadOnlyCollection<Projectile> ProjectileList => _projectileList;
        private static readonly Bitmap _enemyProjectileSprite = Global.GetSingleImage(
                @"projectiles\", "enemyBullet.png");
        public int Dmg { get => _dmg; }
        public int MaxSpeed { get => _maxSpeed; }
        public bool IsLethal { get; private set; }
        public Entity Parent { get => _parent; }
        public int Velocity { get => (int)Math.Sqrt(
                _xVelocity*_xVelocity + _yVelocity*_yVelocity); }

        private static HashSet<Projectile> _projectileList = new HashSet<Projectile>();
        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static HashSet<Projectile> _disposedProjectiles = new HashSet<Projectile>();
        private static readonly double _sqrtBaseScale = Math.Sqrt(Global.BaseScale);
        private readonly float _velocity, _accel;
        private readonly int _dmg, _knockbackSpeed;
        private readonly Bitmap _sprite;
        private Entity _parent;

        private bool _bouncy;
        private int 
            _maxSpeed,
            _deflect = 0,
            _bounceCount = 0;
        private float 
            _cosAngle,
            _sinAngle,
            _xVelocity, 
            _yVelocity;

        public RectangleF AnimRect 
        {
            get 
            {
                if (_parent is Player) return Hitbox; 

                float scaleFactor = 1.76f;
                SizeF drawSize = new SizeF(Size.Width * scaleFactor, Size.Height*scaleFactor);
                PointF p = new PointF(Center.X - drawSize.Width/2, Center.Y - drawSize.Height/2);
                return new RectangleF(p,drawSize);
            }
        }

        /// <summary>
        /// creates a projectile with the following parameters
        /// </summary>
        /// <param name="origin">top-left of the projectile hitbox</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="velocity"></param>
        /// <param name="target"></param>
        public Projectile (PointF origin, int width, int height, int velocity, PointF target, Entity parent, 
                Bitmap sprite,
                float accel=0, int dmg=1, int knockback=800, bool isLethal=true, int? maxSpeed=null, 
                bool bouncy=false)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _sprite = (Bitmap)(sprite.Clone());
            _parent = parent;
            _bouncy = bouncy;
            _accel = accel;
            _velocity = velocity;
            _maxSpeed = maxSpeed ?? velocity;
            _knockbackSpeed = knockback;
            _projectileList.Add(this);
            IsLethal = isLethal;
            Center = origin;

            _maxSpeed = maxSpeed ?? velocity;
            _maxSpeed = (int)(_maxSpeed*Global.BaseScale);

            calculateVelocities(target);
        }

        public Projectile (PointF origin, int width, int height, int velocity, double angle, double xDiff, 
                double yDiff, Entity parent, Bitmap sprite,
                float accel=0, int dmg=1, int knockback=800, bool isLethal=true, int? maxSpeed=null,
                bool bouncy=false)
            : base(origin: origin, width: width, height: height)
        {
            _dmg = dmg;
            _sprite = (Bitmap)(sprite.Clone());
            _parent = parent;
            _bouncy = bouncy;
            _accel = accel;
            _velocity = velocity;
            _knockbackSpeed = knockback;
            _projectileList.Add(this);
            IsLethal = isLethal;
            Center = origin;

            _maxSpeed = maxSpeed ?? velocity;
            _maxSpeed = (int)(_maxSpeed*Global.BaseScale);

            _cosAngle = (float)Math.Cos(angle);
            _sinAngle = (float)Math.Sin(angle);
            _xVelocity = (float)(_cosAngle * _velocity * _sqrtBaseScale * Math.Sign(xDiff));
            _yVelocity = (float)(_sinAngle * _velocity * _sqrtBaseScale * Math.Sign(yDiff));
        }

        public void ToggleLethal(bool? isLethal) => IsLethal = (isLethal is null) ? !IsLethal : isLethal.Value;

        /// <summary>
        /// updates the projectile's location based on the velocity and angle (x and y velocities)
        /// </summary>
        public void MoveProjectile(double deltaTime)
        {
            _xVelocity += (float)(_accel*deltaTime*_cosAngle)*Math.Sign(_xVelocity);
            _yVelocity += (float)(_accel*deltaTime*_sinAngle)*Math.Sign(_yVelocity);

            _xVelocity = Math.Min(Math.Abs(_xVelocity), _maxSpeed)*Math.Sign(_xVelocity);
            _yVelocity = Math.Min(Math.Abs(_yVelocity), _maxSpeed)*Math.Sign(_yVelocity);

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
            _cosAngle = (float)Math.Cos(velocityAngle);
            _sinAngle = (float)Math.Sin(velocityAngle);

            _xVelocity = (float)(_cosAngle * _velocity * _sqrtBaseScale * Math.Sign(xDiff));
            _yVelocity = (float)(_sinAngle * _velocity * _sqrtBaseScale * Math.Sign(yDiff));
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

        private void Bounce(double dt, bool vertical)
        {
            if (_bounceCount++ > 3) 
            {
                Dispose();
                return;
            }
            if (vertical) _yVelocity *= -1;
            else _xVelocity *= -1;
            this.Center = new PointF(Center.X + (float)(_xVelocity*dt), Center.Y + (float)(_yVelocity*dt));
        }

        public void Dispose() => _disposedProjectiles.Add(this); 
        public void DisposeSprite() => _sprite.Dispose();

        public override void CheckActive() { if (this.DistToMid > Global.EntityLoadThreshold) Dispose(); }


        public new static void ClearAllLists()
        {
            _projectileList.Clear();
            _disposedProjectiles.Clear();
        }

        private void drawSprite(Graphics g) => g.DrawImage(_sprite, AnimRect);
        
        public static void DrawAll(Graphics g, Rectangle clientRect)
        {
            ReadOnlySpan<Projectile> _toDraw = _projectileList.ToArray();
            foreach (Projectile p in _toDraw)
            { if (p.Hitbox.IntersectsWith(clientRect)) p.drawSprite(g); }
        }

        public virtual void CheckCollision(double dt, Form form, Player playerBox)
        {
                MoveProjectile(dt);
                PointF bLoc = Center;

                if (_disposedProjectiles.Contains(this)) return;
                if (_bouncy && LocationY < 0) 
                {
                    LocationY = 0;
                    Bounce(dt, true);
                }

                foreach (Platform p in Platform.ActivePlatformList)
                {
                    if (!(p.Hitbox.IntersectsWith(this.Hitbox))) continue;
                    if (_bouncy)
                    {
                        bool vert = false;
                        RectangleF plat = p.Hitbox;
                        if ( plat.Right > Center.X && Center.X > plat.Left) vert=true;
                        Bounce(dt, vert);
                        return;
                    }
                    _disposedProjectiles.Add(this);
                    return;
                }

                if (!Hitbox.IntersectsWith(form.ClientRectangle))
                {
                    _disposedProjectiles.Add(this);
                    return;
                }

                if (_parent is Player)
                {
                    foreach (Enemy e in Enemy.ActiveEnemyList)
                    {
                        if (!(e.Hitbox.IntersectsWith(Hitbox))) continue;

                        _disposedProjectiles.Add(this);
                        e.DoDamage(this);
                        return;
                    }
                }
                else
                {
                    // Return if bullet not touching player
                    if (!playerBox.Hitbox.IntersectsWith(Hitbox)) return;
                    if (!playerBox.CheckParrying(this, dt)) DoDeflect(playerBox, this);
                    _disposedProjectiles.Add(this);
                }

        }

        private void DoDeflect(Player playerBox, Projectile p)
        {
            Projectile newProj = new(
                    origin: playerBox.Center,
                    width: (int)(p.AnimRect.Width*1.5),
                    height: (int)(p.AnimRect.Height*1.5),
                    velocity: p.Velocity,
                    parent: playerBox,
                    sprite: _enemyProjectileSprite,
                    target: p.Parent.Center,
                    dmg: p.Dmg,
                    maxSpeed: p.MaxSpeed);
        }


        public static void CheckProjectileCollisions(double dt, Form form, Player playerBox)
        {
            if (_projectileList.Count == 0) return;
            ReadOnlySpan<Projectile> _toCheck = _projectileList.ToArray();
            foreach (Projectile p in _toCheck)
            { p.CheckCollision(dt, form, playerBox); }

            if (_disposedProjectiles.Count == 0) return;

            foreach (Projectile p in _disposedProjectiles)
            {
                _projectileList.Remove(p);
                p.DisposeSprite();
                p.Delete();
            }

            _disposedProjectiles.Clear();
        }
    }
}
