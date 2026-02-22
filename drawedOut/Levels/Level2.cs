namespace drawedOut
{
    internal class Level2 : Level0
    {
        private const int _LEVELWIDTH = 13000;

        private Platform 
            _door1,
            _door2,
            _door3,
            _door4,
            _door5;
        private ThirdBoss _Tommy;

        public Level2() : 
            base( 
                    levelNo: 2,
                    levelWidth: _LEVELWIDTH,
                    playerStartPos: new Point(450, 600)
                ) { }

        protected override void InitEnemies()
        {
            firstWave();
            secondWave();
            thirdWave();

            _Tommy = new(
                    activationDoor: ref roomDoor,
                    origin: new Point(_LEVELWIDTH-500, 100),
                    height: 220,
                    width: 220,
                    itemDrop: BossPickup,
                    levelTimerSW: ref levelTimerSW,
                    hp: 12);
        }

        protected override void InitPlatforms()
        {
            Platform floor;
            int floorY = 750;
            floor = new(
                    origin: new Point(1, floorY),
                    width: 7350,
                    height: 512,
                    toggleable: true,
                    defaultState: true);
            floor = new(
                    origin: new Point(8350, floorY),
                    width: _LEVELWIDTH-8350,
                    height: 512,
                    toggleable: true,
                    defaultState: true);

            makeDoors(floorY);

            Platform room1Plat;
            room1Plat = new(
                    origin: new Point(1200, floorY - Player.VisibleHeight),
                    width: 200,
                    height: 50);
            room1Plat = new(
                    origin: new Point(2700, floorY - Player.VisibleHeight),
                    width: 200,
                    height: 50);
            room1Plat = new(
                    origin: new Point(1200+(2900-1200)/2-400, floorY - Player.VisibleHeight*2),
                    width: 800,
                    height: 60);

            Platform room2Plat;
            CreateNewWall(floorY, 3400, 200, 180);
            room2Plat = new(
                    origin: new Point(3750, floorY - 350),
                    width: 200,
                    height: 40);
            room2Plat = new(
                    origin: new Point(4100, floorY - 550),
                    width: 700,
                    height: 60);

            Platform room3Plat;
            room3Plat = new(
                    origin: new Point(7350+100, floorY + 100),
                    width: 300,
                    height: 50);
            room3Plat = new(
                    origin: new Point(8350-400, floorY + 100),
                    width: 300,
                    height: 50);
            CreateNewWall(floorY, 8500, 100, 180);
            room3Plat = new(
                    origin: new Point(7850-400, floorY - 350),
                    width: 800,
                    height: 50);


            base.InitPlatforms();
        }

        private void makeDoors(int floorY)
        {
            _door1 = new(
                    origin: new Point(1000,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: false);
            _door2 = new(
                    origin: new Point(2900,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: true);
            _door3 = new(
                    origin: new Point(4800,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: true);
            _door4 = new(
                    origin: new Point(6800,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: false);
            _door5 = new(
                    origin: new Point(8700,0),
                    width: 200,
                    height: floorY,
                    toggleable: true,
                    defaultState: true);
        }

        protected override void InitProps()
        { 
            Checkpoint newCheckpoint;
            newCheckpoint = new(origin: new Point( (int)(_door4.LocationX - 800), 500 ));
            newCheckpoint = new(origin: new Point(_LEVELWIDTH-2300, 500)); 
        }

        protected override void otherLogic(double dt)
        {
            if (playerCharacter.LocationX > _door1.Hitbox.Right + 20) 
            {
                _door1.Activate();
                Enemy.ReActivate();
            }
            else if (playerCharacter.LocationX < _door1.Hitbox.Left) 
            {
                _door1.Deactivate();
                Enemy.DeactivateAll();
            }
            if (playerCharacter.LocationX > _door4.Hitbox.Right + 20) _door4.Activate();
            else if (playerCharacter.LocationX < _door4.Hitbox.Left) _door4.Deactivate();

            if (Enemy.ActiveEnemyList.Count<=1) _door2.Deactivate();
            else _door2.Activate();
            if (Enemy.ActiveEnemyList.Count==0) 
            {
                _door3.Deactivate();
                _door5.Deactivate();
            }
            else 
            {
                _door3.Activate();
                _door5.Activate();
            }
        }

        private void firstWave()
        {
            int startX = 1200; 
            int width = 1700;
            MeleeEnemy mEnemy;
            mEnemy = new( origin: new Point(startX + 200, 200) );
            mEnemy = new( origin: new Point(startX + width - 200, 600) );
            FlyingEnemy fEnemy;
            fEnemy = new( origin: new Point(startX + width/2 - 200, 200) );
            fEnemy = new( origin: new Point(startX + width/2 - 200, 600) );
        }
        private void secondWave()
        {
            int startX = 3100; 
            int width = 1700;
            MeleeEnemy mEnemy;
            mEnemy = new( origin: new Point(startX + width - 200, 100) );
            mEnemy = new( origin: new Point(startX + width - 200, 600) );
            FlyingEnemy fEnemy;
            fEnemy = new( origin: new Point(startX + width - 200, 100) );
            fEnemy = new( origin: new Point(startX + width - 200, 600) );
            fEnemy = new( origin: new Point(startX + width/2 - 200, 700) );
        }
        private void thirdWave()
        {
            int startX = 7000; 
            int width = 1700;
            MeleeEnemy mEnemy;
            mEnemy = new( origin: new Point(startX + width - 300, 200) );
            mEnemy = new( origin: new Point(startX + width/2, 100) );
            FlyingEnemy fEnemy;
            fEnemy = new( origin: new Point(startX+500, 50));
            fEnemy = new( origin: new Point(startX+width-100, 50));
            fEnemy = new( origin: new Point(startX+width/2 - 200, 600));
            fEnemy = new( origin: new Point(startX+width/2 + 200, 600));
        }

        private void BossPickup()
        {
            if (!Player.UnlockedMoves[2]) Player.MaxHp += 2;
            Player.UnlockedMoves[2] = true;
            FinishLevel();
        }
    }
}

