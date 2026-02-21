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
        // private ThirdBoss _Tommy;

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

            Platform room3Plat;
            room3Plat = new(
                    origin: new Point(7350+200, floorY + 100),
                    width: 300,
                    height: 50);
            room3Plat = new(
                    origin: new Point(8350-400, floorY + 100),
                    width: 300,
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
            newCheckpoint = new(origin: new Point(_LEVELWIDTH-2300, 500)); 
        }

        protected override void otherLogic(double dt)
        {
            if (playerCharacter.LocationX > _door1.Hitbox.Right + 20) _door1.Activate();
            else _door1.Deactivate();
            if (playerCharacter.LocationX > _door4.Hitbox.Right + 20) _door4.Activate();
            else _door4.Deactivate();
            if (Enemy.ActiveEnemyList.Count==0) 
            {
                _door2.Deactivate();
                _door3.Deactivate();
                _door5.Deactivate();
            }
            else 
            {
                _door2.Deactivate();
                _door3.Deactivate();
                _door5.Deactivate();
            }
        }

        private void firstWave(){}
        private void secondWave(){}
        private void thirdWave() {}

        private void BossPickup()
        {
            if (!Player.UnlockedMoves[2]) Player.MaxHp += 2;
            Player.UnlockedMoves[2] = true;
            FinishLevel();
        }
    }
}

