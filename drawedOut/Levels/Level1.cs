namespace drawedOut
{
    internal class Level1 : Level0
    {
        private const int _LEVELWIDTH = 12000;

        private Platform _lockedStep;
        // private SecondBoss _Oscar;

        public Level1() : 
            base( 
                    levelNo: 1,
                    levelWidth: _LEVELWIDTH,
                    playerStartPos: new Point(450, 600)
                ) { }

        protected override void InitEnemies()
        {
            startSpawn();
            spawnTop();
            spawnBottom();

            // _firstBoss = new(
            //         activationDoor: ref roomDoor,
            //         origin: new Point(_LEVELWIDTH-500, 100),
            //         height: 250,
            //         width: 250,
            //         itemDrop: BossPickup,
            //         levelTimerSW: ref levelTimerSW,
            //         hp: 6);
        }

        protected override void InitPlatforms()
        {
            Platform floor;
            int floorY = 750;
            floor = new(
                    origin: new Point(1, floorY),
                    width: 4400,
                    height: 512,
                    toggleable: true,
                    defaultState: true);
            floor = new(
                    origin: new Point(7300, floorY),
                    width: _LEVELWIDTH-7300,
                    height: 512,
                    toggleable: true,
                    defaultState: true);
            floor = new(
                    origin: new Point(4800, 300),
                    width: 7300-4800,
                    height: 70,
                    toggleable: true,
                    defaultState: true);

            _lockedStep = new(
                    origin: new Point(2000,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: true);

            Platform newPlat;
            newPlat = new(
                    origin: new Point(4450, floorY - Player.VisibleHeight/2 - 50),
                    width: 100,
                    height: 20);
            newPlat = new(
                    origin: new Point(4600, floorY - Player.VisibleHeight/2 - 180),
                    width: 100,
                    height: 20);

            for (int i = 0; i<3; i++)
            {
                int offset = 450;
                newPlat = new(
                        origin: new Point(4700+(i*offset), 800),
                        width: 250,
                        height: 80);
            }

            newPlat = new(
                    origin: new Point(6200, 800),
                    width: 200,
                    height: 50);
            newPlat = new(
                    origin: new Point(6800, 800),
                    width: 120,
                    height: 20);


            base.InitPlatforms();
        }

        protected override void InitProps()
        { 
            Checkpoint newCheckpoint;
            newCheckpoint = new(origin: new Point(4000, 500)); 
            newCheckpoint = new(origin: new Point(_LEVELWIDTH-2300, 500)); 
        }

        protected override void otherLogic(double dt)
        {
            _lockedStep.Activate();
            if (playerRelX < 2300 && Enemy.ActiveEnemyList.Count < 1) _lockedStep.Deactivate(); 
        }

        private void spawnBottom()
        {
            FlyingEnemy flyingEnemy = new(new Point(6000, 500));
        }

        private void startSpawn()
        {
            MeleeEnemy meleeEnemy;
            meleeEnemy = new(new Point(2000, 300));
            meleeEnemy = new(new Point(3200, 300));
            FlyingEnemy flyingEnemy;
            flyingEnemy = new(new Point(700, 200));
            flyingEnemy = new(new Point(1300, 200));
        }

        private void spawnTop()
        {
            MeleeEnemy meleeEnemy = new(new Point(5400, -10000));
            FlyingEnemy flyingEnemy;
            flyingEnemy = new(new Point(5400, 100));
            flyingEnemy = new(new Point(6000, 100));
        }

        private void BossPickup()
        {
            if (SaveData.GetFastestScore(1) is null) Player.MaxHp += 4;
            FinishLevel();
        }

    }
}
