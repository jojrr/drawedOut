namespace drawedOut
{
    internal partial class TutorialLevel : Level0
    {
        private const int _LEVELWIDTH = 11200;

        private FirstBoss _firstBoss;

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
            MeleeEnemy meleeEnemy;
            meleeEnemy = new(origin:new Point(3500, 250));

            meleeEnemy = new(origin:new Point(6200, 740));
            meleeEnemy = new(origin:new Point(7000, 740));

            FlyingEnemy flyingEnemy;
            flyingEnemy = new(origin:new Point(6000, 100));
            flyingEnemy = new(origin:new Point(6800, 100));

            _firstBoss = new(
                    activationDoor: ref roomDoor,
                    origin: new Point(_LEVELWIDTH-500, 100),
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

            Platform newPlat;
            newPlat = new(
               origin: new Point(400, 350),
               width: 400,
               height: 50);

            CreateNewWall(floorY, 1000, 200, 200);

            CreateNewWall(floorY, 2600, 200, 200);
            CreateNewWall(floorY, 3000, 200, 300);

            CreateNewWall(floorY, 5000, 100, 150);
            CreateNewWall(floorY, 5100, 100, 250);
            newPlat = new(
                    origin: new Point(5300,floorY - 350),
                    width: 1000,
                    height: 50);
            newPlat = new(
                    origin: new Point(6300,floorY - 350),
                    width: 1000,
                    height: 50);

            base.InitPlatforms();
        }

        private void CreateNewWall(int floorY, int x, int pWidth, int pHeight)
        {
            Platform newWall;
            newWall = new(
               origin: new Point(x, floorY-pHeight),
               width: pWidth,
               height: pHeight);
        }

        protected override void InitProps()
        { 
            Checkpoint newCheckpoint;
            newCheckpoint = new(origin: new Point(4600, 600)); 
            newCheckpoint = new(origin: new Point(_LEVELWIDTH-2300, 500)); 

            BgObj newSign;
            newSign = new(origin: new Point(1500, 500), sprite: Global.GetSingleImage(@"fillerAnim\"));
            newSign = new(origin: new Point(2100, 500), sprite: Global.GetSingleImage(@"fillerAnim\"));
            newSign = new(origin: new Point(2600, 100), sprite: Global.GetSingleImage(@"fillerAnim\"));
            newSign = new(origin: new Point(4500, 500), sprite: Global.GetSingleImage(@"fillerAnim\"));
        }
    }
}
