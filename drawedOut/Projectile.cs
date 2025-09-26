using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace drawedOut
{
    internal class Projectile : Entity
    {
        private float 
            xVelocity, 
            yVelocity;

        // debugging 
        // todo: set to private
        public float 
            velocityAngle, 
            yDiff, 
            xDiff;

        public float Velocity;

        private bool isRebound = false;

        private PointF Target;

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
            Velocity = velocity;
            float[] velocities = CalculateVelocities(target);

            Target = target;

            xVelocity = velocities[0]; 
            yVelocity = velocities[1]; 

            ProjectileList.Add(this);

            // debugging
            velocityAngle = velocities[2];
            yDiff = velocities[3];
            xDiff = velocities[4];
        }


        /// <summary>
        /// updates the projectile's location based on the velocity and angle (x and y velocities)
        /// adjusts for rebound
        /// </summary>
        public void moveProjectile(double deltaTime)
        {
            float xVelocity = (float)(this.xVelocity*deltaTime);
            float yVelocity = (float)(this.yVelocity*deltaTime);

            // moves backwards if rebounded
            if (isRebound)
            {
                this.updateLocation( Location.X - xVelocity, Location.Y - yVelocity);
                return;
            }

            this.updateLocation( Location.X + xVelocity, Location.Y + yVelocity);
        }



        // calculates the required y and x velocities for the projectile
        // angle and distances used for debugging
        public float[] CalculateVelocities(PointF target)
        {
            float xDiff = target.X - Location.X;
            float yDiff = target.Y - Location.Y;

            double velocityAngle = Math.Atan(yDiff/xDiff);

            float xVelocity = (float)Math.Abs(Math.Cos(velocityAngle)) * Velocity * Math.Sign(xDiff);
            float yVelocity = (float)Math.Abs(Math.Sin(velocityAngle)) * Velocity * Math.Sign(yDiff);

               

            return [xVelocity, yVelocity, (float)velocityAngle, yDiff, xDiff];
        }

        /// <summary>
        /// makes the projectile rebound off [target]
        /// </summary>
        /// <param name="target">rebound target</param>
        public void rebound(PointF target) // todo: use Entity or RectangleF - not PointF
        {
            isRebound = !isRebound; 

            // todo: redo positioning logic 
            // also use different variable names maybe
            xDiff = Center.X - target.X;
            yDiff = Center.Y - target.Y;

            this.updateLocation( Location.X + 2*xDiff, Location.Y + 2*yDiff);
        }
    }
}
