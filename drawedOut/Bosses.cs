namespace drawedOut
{
    internal class FirstBoss : Enemy
    {
        private const int 
            ATK_ENDLAG_S = 1,
            ATK_X_OFFSET = 100;
        private readonly double 
            _maxRange,
            _jumpRange;
        private static Bitmap _downedSprite;
        private readonly Attacks _attackOne;
        private readonly Attacks _attackTwo;
        private double _movementTimer;

        public FirstBoss(Point origin, int width, int height, int hp=3)
            :base(origin:origin, width:width, height:height, hp:hp)
        {
            Size atkSize = new Size(380,520);

            _maxRange = Width+(atkSize.Width * Global.BaseScale);
            _jumpRange = 1.5*Height;

            _attackOne = new Attacks(
                    parent: this,
                    size: atkSize,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: ATK_X_OFFSET,
                    spawn: 7,
                    despawn: 11,
                    endlag: ATK_ENDLAG_S);
            _attackTwo = new ProjectileAttack(
                    parent:this,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    endlag: ATK_ENDLAG_S,
                    spawn: 5,
                    projectileEvent: DoAttack);

            string downedSpriteFolder = @"seven\";
            _downedSprite = Global.GetSingleImage(downedSpriteFolder);
            setRunAnim(@"fillerAnim\");
            setIdleAnim(@"fillerPic\");
        }


        public void DoAttack()
        {
            Size projectileSize = new Size(160,160);

            Projectile flyingEnemyProj = new Projectile(
                    origin: this.Center,
                    width: projectileSize.Width,
                    height: projectileSize.Height,
                    velocity: 1000,

                    parent: this);
        }
    }
}


