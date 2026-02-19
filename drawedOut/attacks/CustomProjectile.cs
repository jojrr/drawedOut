
namespace drawedOut
{
    internal class PlayerUltProjectile:Projectile
    {
        private static HashSet<Enemy> _toDamage = new HashSet<Enemy>();
        private static float _slowFactor;

        public PlayerUltProjectile(PointF origin, int width, int height, float velocity, double angle, 
                Entity parent, Bitmap sprite, int dmg, int accel, int maxSpeed, float slowFactor)
            : base(origin:origin, width:width, height:height, velocity:velocity, angle:angle, xDiff:1, yDiff:1,
                    parent:parent, sprite:sprite, isLethal:true, dmg:dmg, accel:accel, maxSpeed:maxSpeed)
            { 
                _slowFactor = slowFactor;
            }

        public override void CheckCollision(double dt, Form form, Player playerBox)
        {
            MoveProjectile(dt/_slowFactor);
            PointF bLoc = Center;

            foreach (Enemy e in Enemy.ActiveEnemyList)
            {
                if (!(e.Hitbox.IntersectsWith(Hitbox))) continue;
                _toDamage.Add(e);
            }

            if (this.LocationY > 2100) 
            {
                foreach (Enemy e in _toDamage)
                {
                    e.DoDamage(this);
                }
                _toDamage.Clear();
                Dispose();
            }
        }
    }
}
