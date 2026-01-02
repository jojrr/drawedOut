namespace drawedOut
{
    internal class ProjectileAttack : Attacks
    {
        private Action _launchProjectile;

        public ProjectileAttack(Character? parent, AnimationPlayer animation, float endlag, 
                Action? projectileEvent=null, int spawn=0, int despawn=-1, int dmg=1, bool isLethal=false)
            : base (parent:parent, width:0, height:0, animation:animation, endlag:endlag, spawn:spawn, despawn:despawn, dmg:dmg, isLethal:isLethal)
        {
            AttacksList.Remove(this);
            if (projectileEvent is not null) _launchProjectile = projectileEvent;
        }

        public void SetAction(Character parent, Action? projectileEvent)
        {
            if (Parent is not null) throw new Exception("Parent is trying to be overwritten");
            if (_launchProjectile is not null) throw new Exception("LaunchProjectile is trying to be overwritten");
            if (projectileEvent is null) throw new Exception ("no projectileEvent set and no target/veloicty set");

            _launchProjectile = projectileEvent;
            base.Parent = parent;
        }


        public new Bitmap NextAnimFrame(Global.XDirections facingDir = Global.XDirections.right)
        {
            if (Animation.CurFrame == spawnFrame) 
            {
                _launchProjectile();
                Parent.ApplyEndlag(endlagS);
            }
            return Animation.NextFrame();
        }
    }
}


