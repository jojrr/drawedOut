namespace drawedOut
{
    internal class Projectile : Entity
    {
        private float 
            _xVelocity, 
            _yVelocity;
        private bool _isRebound = false;
        private readonly float _velocity;

        public static List<Projectile> ProjectileList = new List<Projectile>();

        /// <summary>
        /// creates a projectile with the following parameters
        /// </summary>
        /// <param name="origin">top-left of the projectile hitbox</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="velocity"></param>
        /// <param name="target"></param>
        public Projectile (PointF origin, int width, int height, float velocity, PointF target)
            : base(origin: origin, width: width, height: height)
        {
            _velocity = velocity;
            calculateVelocities(target);
            ProjectileList.Add(this);
        }

        public Projectile (PointF origin, int width, int height, float velocity, double angle, double xDiff, double yDiff)
            : base(origin: origin, width: width, height: height)
        {
            _velocity = velocity;
            _xVelocity = (float)Math.Cos(angle) * _velocity * Math.Sign(xDiff);
            _yVelocity = (float)Math.Sin(angle) * _velocity * Math.Sign(yDiff);
            ProjectileList.Add(this);
        }


        /// <summary>
        /// updates the projectile's location based on the velocity and angle (x and y velocities)
        /// adjusts for rebound
        /// </summary>
        public void MoveProjectile(double deltaTime)
        {
            float xVelocity = (float)(_xVelocity*deltaTime);
            float yVelocity = (float)(_yVelocity*deltaTime);

            // moves backwards if rebounded
            if (_isRebound)
            {
                Location = new PointF(Location.X-xVelocity, Location.Y-yVelocity);
                return;
            }
            Location = new PointF(Location.X+xVelocity, Location.Y+yVelocity);
        }



        // calculates the required y and x velocities for the projectile
        // angle and distances used for debugging
        private void calculateVelocities(PointF target)
        {
            float xDiff = target.X - Location.X;
            float yDiff = target.Y - Location.Y;

            // HACK: changed this to do less calculations before was doing Math.Abs during velocityCalculations instead of angleCalc.
            float velocityAngle = (float)Math.Abs(Math.Atan(yDiff/xDiff)); 

            _xVelocity = (float)Math.Cos(velocityAngle) * _velocity * Math.Sign(xDiff);
            _yVelocity = (float)Math.Sin(velocityAngle) * _velocity * Math.Sign(yDiff);
        }

        /// <summary>
        /// makes the projectile rebound off [target]
        /// </summary>
        /// <param name="target">rebound target</param>
        public void rebound(Entity target) 
        {
            _isRebound = !_isRebound; 
            PointF targetCenter = target.Center;

            float xDiff = Center.X - targetCenter.X;
            float yDiff = Center.Y - targetCenter.Y;

            float newX = (Width/2 + target.Width/2 + _xVelocity)*Math.Sign(xDiff);
            float newY = (Height/2 + target.Height/2 + _yVelocity)*Math.Sign(yDiff);

            Center = new PointF( targetCenter.X + newX, targetCenter.Y + newY );
        }

        public void Dipose() => ProjectileList.Remove(this);

        public override void CheckActive() 
        {
            if (this.DistToMid > Global.EntityLoadThreshold) return;
            Dipose();
        }
    }
}
