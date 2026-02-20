namespace drawedOut
{
    internal partial class TutorialLevel : Level0
    {
        private const int _LEVELWIDTH = 8400;

        private FlyingEnemy _flyingEnemy;
        private MeleeEnemy _meleeEnemy;
        private FirstBoss _firstBoss;

        private Checkpoint _newCheckpoint;
        private Platform _newPlat;
        private BgObj _newSign;
        private Platform 
            _floor1,
            _floor2;

        public TutorialLevel() : 
            base( 
                    levelNo: 0,
                    levelWidth: _LEVELWIDTH,
                    playerStartPos: new Point(450, 550)
        ) { }

        protected override void InitEnemies()
        {
            _meleeEnemy = new(origin:new Point(3500, -550));
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
            int floorY = 750;
            _floor1 = new(
                origin: new Point(1, floorY),
                width: 1700,
                height: 512,
                toggleable: true,
                defaultState: true);
            _floor2 = new(
                origin: new Point(1900, floorY),
                width: _LEVELWIDTH-1900,
                height: 512,
                toggleable: true,
                defaultState: true);

            _newPlat = new(
               origin: new Point(400, 350),
               width: 400,
               height: 50);

            CreateNewWall(floorY, 1000, 200, 200);

            CreateNewWall(floorY, 2600, 200, 200);
            CreateNewWall(floorY, 3000, 200, 300);

            base.InitPlatforms();
        }

        private void CreateNewWall(int floorY, int x, int pWidth, int pHeight)
        {
            _newPlat = new(
               origin: new Point(x, floorY-pHeight),
               width: pWidth,
               height: pHeight);
        }

        protected override void InitProps()
        { 
            _newCheckpoint = new(origin: new Point(6200, 600)); 
            _newSign = new(origin: new Point(1500, 500), sprite: Global.GetSingleImage(@"fillerAnim\"));
            _newSign = new(origin: new Point(2100, 500), sprite: Global.GetSingleImage(@"fillerAnim\"));
            _newSign = new(origin: new Point(2600, 100), sprite: Global.GetSingleImage(@"fillerAnim\"));
        }
    }
}
