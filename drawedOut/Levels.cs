namespace drawedOut
{
    internal partial class TutorialLevel : Level0
    {
        private const int _LEVELWIDTH = 8400;

        private MeleeEnemy _meleeOne;
        private FirstBoss _firstBoss;
        // private FlyingEnemy flyingOne;
        private Checkpoint _checkpointOne;
        private BgObj _movSign;

        private Platform 
            _floor,
            _roofTop,
            _firstTower;

        public TutorialLevel() : 
            base( 
                    levelNo: 0,
                    levelWidth: _LEVELWIDTH,
                    playerStartPos: new Point(450, 550)
        ) { }

        protected override void InitEnemies()
        {
            _meleeOne = new(origin:new Point(2850, -550));
            // flyingOne = new(origin:new Point(850, 100));
            _firstBoss = new(
                    activationDoor: ref roomDoor,
                    origin: new Point(8200, 100),
                    height: 250,
                    width: 250,
                    itemDrop: BossDeath,
                    levelTimerSW: ref levelTimerSW,
                    hp: 6);
        }

        protected override void InitPlatforms()
        {
            _floor = new(
                origin: new Point(1, 750),
                width: _LEVELWIDTH,
                height: 512,
                toggleable: true,
                defaultState: true);

            _roofTop = new(
               origin: new Point(400, 350),
               width: 400,
               height: 50);

            _firstTower = new(
               origin: new Point(1000, 550),
               width: 200,
               height: 250);

            base.InitPlatforms();
        }

        protected override void InitProps()
        { 
            _checkpointOne = new(origin: new Point(6200, 600)); 
        }
    }
}
