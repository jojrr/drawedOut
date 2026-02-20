
namespace drawedOut
{
    internal class PlayerUltProjectile:Projectile
    {
        private static HashSet<Enemy> _toDamage = new HashSet<Enemy>();
        private int _heal; 

        public PlayerUltProjectile(PointF origin, int width, int height, float velocity, double angle, 
                Entity parent, Bitmap sprite, int dmg, int accel, int maxSpeed, int heal)
            : base(origin:origin, width:width, height:height, velocity:velocity, angle:angle, xDiff:1, yDiff:1,
                    parent:parent, sprite:sprite, isLethal:true, dmg:dmg, accel:accel, maxSpeed:maxSpeed)
            { _heal = heal; }

        public override void CheckCollision(double dt, Form form, Player playerBox)
        {
            MoveProjectile(Level0.AbsDeltaTime);
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
                    if (e.Hp <= 0) playerBox.HealPlayer(_heal);
                }
                _toDamage.Clear();
                Dispose();
            }
        }
    }
}
