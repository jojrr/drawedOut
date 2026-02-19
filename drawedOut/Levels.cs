namespace drawedOut
{
    internal partial class TutorialLevel : Level0
    {
        private const int LEVELWIDTH = 8400;

        private MeleeEnemy meleeOne;
        private FirstBoss firstBoss;
        private FlyingEnemy flyingOne;
        private Checkpoint checkpointOne;
        private Checkpoint checkpointOn;

        private Platform 
            floor,
            box3,
            box4,
            box5;

        public TutorialLevel() : 
            base( 
                    levelNo: 0,
                    levelWidth: LEVELWIDTH,
                    playerStartPos: new Point(850, 550)
        ) { }

        protected override void InitEnemies()
        {
            meleeOne = new(origin:new Point(2850, -550));
            flyingOne = new(origin:new Point(850, 100));
            firstBoss = new(
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
            floor = new(
                origin: new Point(1, 750),
                width: LEVELWIDTH,
                height: 512,
                toggleable: true,
                defaultState: true);

            box3 = new(
               origin: new Point(300, 250),
               width: 400,
               height: 175);

            box4 = new(
               origin: new Point(1000, 550),
               width: 200,
               height: 250);

            box5 = new(
               origin: new Point(1500, 550),
               width: 200,
               height: 250);

            base.InitPlatforms();
        }

        protected override void InitCheckpoints()
        { 
            checkpointOne = new(origin: new Point(6200, 600)); 
        }
    }
}
