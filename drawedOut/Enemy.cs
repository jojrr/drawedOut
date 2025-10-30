namespace drawedOut
{
    internal class Enemy : Character
    {
        public static List<Enemy> ActiveEnemyList = new List<Enemy>();
        public static List<Enemy> InactiveEnemyList = new List<Enemy>();

        public Enemy(Point origin, int width, int height, int hp, int xAccel)
            : base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel)
        {
            InactiveEnemyList.Add(this);
        }

        public virtual void DoMove(double dt, bool doScroll) => 
            throw new Exception($"DoMove is not implemented in {this.GetType()}");

        public override void CheckActive()
        {
            if (DistToMid > Global.EntityLoadThreshold)
            {
                if (!IsActive) return;

                IsActive = false;
                InactiveEnemyList.Add(this);
                ActiveEnemyList.Remove(this);
                return;
            }

            if (IsActive) return;
                
            IsActive = true;
            ActiveEnemyList.Add(this);
            InactiveEnemyList.Remove(this);
        }
    }
}

