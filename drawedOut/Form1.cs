using System.Diagnostics;
namespace drawedOut
{
    public partial class Form1 : Form
    {

        Character playerBox = new(
            origin: new Point(750, 250),
            width: 50,
            height: 50,
            LocatedLevel: 0,
            LocatedChunk: 0,
            yVelocity: -0.2);

        Platform box2 = new(
           origin: new Point(1, 650),
           width: 5400,
           height: 550,
           LocatedLevel: 0,
           LocatedChunk: 0);

        Platform box3 = new(
           origin: new Point(300, 200),
           width: 400,
           height: 175,
           LocatedLevel: 0,
           LocatedChunk: 1);

        Platform box4 = new(
           origin: new Point(1000, 400),
           width: 200,
           height: 300,
           LocatedLevel: 0,
           LocatedChunk: 1);

        Platform box5 = new(
           origin: new Point(1500, 400),
           width: 200,
           height: 300,
           LocatedLevel: 0,
           LocatedChunk: 2);

        Entity chunkLoader1 = new Entity(
            origin: new Point(1200, 0),
            width: 1,
            height: 1);


        Rectangle viewPort;
        string onWorldBoundary = "left";

        bool movingLeft = false;
        bool movingRight = false;
        bool jumping = false;

        bool scrollRight = false;
        bool scrollLeft = false;

        double xAccel = 10;

        int CurrentLevel;
        int[] LoadedChunks = new int[2];
        List<int> UnLoadedChunks;
        int TotalChunks;



        Brush playerBrush;

        // point where bullets spawn
        Entity bulletOrigin = new Entity (
            origin: new PointF(800, 250),
            width: 1,
            height: 1,
            chunk: 1);

        // threading 
        CancellationTokenSource threadTokenSrc = new CancellationTokenSource(); // used for closing the thread
        Thread hitboxThread = new Thread(() => { });
        Thread movementThread = new Thread(() => { });
        Thread deltaThread = new Thread(() => { });

        hpBarUI hpBar;

        Dictionary<Entity, PointF> zoomOrigins = new Dictionary<Entity, PointF>();  

        bool
            playerIsHit = false,

            isParrying = false,

            setFreeze = false,
            isPaused = false,
            slowedMov = false;


        int
            maxHp = 6,
            currentHp,
            targetFrameRate = 120,
            refreshRate;


        const float
            zoomFactor = 1.2F,
            slowFactor = 2.5F,
            slowDurationS = 0.35F,

            parryDurationS = 0.45F,
            perfectParryWindowS = 0.05F,
            parryEndlagS = 0.2F,

            bulletCooldownS = 0.5F,

            freezeDuratonS = 0.15F;

        float
            bulletInterval,

            parryWindow,
            endLagTime,

            curZoom = 1,

            slowFrame = 0,
            freezeFrame = 0;

        double
            prevTime = 0,
            motionDT = 0,
            deltaTime = 0;

        Stopwatch stopWatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // set height and width of window
            Width = 1660;
            Height = 770;

            // sets the refresh interval
            targetFrameRate += 10;
            refreshRate = (int)(1000.0F / targetFrameRate);
            timer1.Interval = refreshRate;
            timer1.Enabled = true;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            CurrentLevel = 0;
            LoadedChunks = [0, 1];
            UnLoadedChunks = [2];
            TotalChunks = LoadedChunks.Count() + UnLoadedChunks.Count();

            viewPort = new Rectangle(new Point(-5, 0), new Size(Width + 10, Height));

            bulletInterval = bulletCooldownS;


            hpBar = new hpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    iconCount: maxHp / 2
            );

            currentHp = maxHp;
            computeHP();

            playerBrush = Brushes.Blue;

            stopWatch.Start();
            fpsTimer.Start();


            CancellationToken threadCT = threadTokenSrc.Token;
            deltaThread = new Thread(() =>
            {
                while (true)
                {
                    if (threadCT.IsCancellationRequested)
                        return;
                    getDeltaTime();
                    Thread.Sleep(refreshRate / 4);
                }
            });

            hitboxThread = new Thread(() =>
            {
                while (true)
                {
                    if (threadCT.IsCancellationRequested)
                        return;
                    if (isPaused)
                        continue;
                    attackHandler();
                    Thread.Sleep(refreshRate / 4);
                }
            });

            movementThread = new Thread(() =>
            {
                while (true)
                {
                    if (threadCT.IsCancellationRequested)
                        return;
                    if (isPaused)
                        continue;
                    movementTick();
                    Thread.Sleep(refreshRate / 4);
                }
            });


            deltaThread.Start();
            hitboxThread.Start();
            movementThread.Start();

            togglePause(false);
        }


        private void getDeltaTime()
        {
            double currentTime = stopWatch.Elapsed.TotalSeconds;
            deltaTime = (currentTime - prevTime) * 10;
            motionDT = deltaTime;
            prevTime = currentTime;

            isPaused = false;
            if (freezeFrame > 0)
            {
                freezeFrame -= (float)deltaTime;
                isPaused = true;
            }
        }


        private void slowTime()
        {
            if (slowFrame > 0) // todo: functionize all the slow logic
            {
                slowedMov = true;

                if (zoomFactor <= 1)
                    throw new ArgumentException("zoomFactor must be bigger than 1");

                slowFrame -= (float)deltaTime;

                deltaTime /= slowFactor;
                motionDT = (zoomFactor * deltaTime);
            }
            else if (slowedMov) // keeps the player in motion when in slow 
            {
                movingLeft = false;
                movingRight = false;
                slowedMov = false;
            }
        }


        private void attackHandler()
        {
            playerIsHit = false;
            setFreeze = false;
            freezeFrame = 0;

            // checks if slowed
            slowTime();


            if (endLagTime > 0)
            {
                endLagTime -= (float)deltaTime;
                isParrying = false;
            }



            foreach (Projectile bullet in Projectile.ProjectileList)
            {
                bullet.moveProjectile(motionDT);
                PointF bLoc = bullet.getCenter();

                List<int> loadedChunks = LoadedChunks.ToList();
                foreach (int i in loadedChunks)
                { 
                    //List<Entity> list = Entity.EntityList[CurrentLevel][i];
                    List<Entity> list = Entity.EntityList[CurrentLevel][i].ToList();

                    foreach(Platform p in list.OfType<Platform>())
                    {
                        if ( !( p.getHitbox().IntersectsWith(bullet.getHitbox()) ) )
                            continue;

                        disposedProjectiles.Add(bullet); 
                        break; 

                    }
                    if (disposedProjectiles.Contains(bullet)) { break; }
                }

                if ((bLoc.X<0) || (bLoc.Y<0) || (bLoc.X > ClientSize.Width) || (bLoc.Y > ClientSize.Height))
                { disposedProjectiles.Add(bullet); continue; }

                if (playerBox.getHitbox().IntersectsWith(bullet.getHitbox()))
                {
                    if (isParrying)
                    {
                        bullet.rebound(playerBox.getCenter()); // required to prevent getting hit anyway when parrying

                        // if the current parry has lasted for at most the perfectParryWindow
                        if (parryWindow >= (parryDurationS - perfectParryWindowS) * 10)
                        {
                            //setFreeze = true;
                            slowFrame = slowDurationS * 10;
                            zoomScreen(zoomFactor);
                            isParrying = false;
                            endLagTime = 0;
                            continue; // so that the projectile is not disposed of when rebounded
                        }
                    }
                    else
                    {
                        playerIsHit = true;
                        doPlayerDamage(1);
                        setFreeze = true;
                    }

                    disposedProjectiles.Add(bullet);

                }
            }

            if (setFreeze)
                freezeFrame = freezeDuratonS * 10;

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            disposedProjectiles.Clear();


            if ((bulletInterval > 0) || (deltaTime == 0))
                bulletInterval -= (float)deltaTime;
            else
            {
                createBullet();
                bulletInterval = bulletCooldownS * 10;
            }


            // ticks down the parry window
            if (isParrying && parryWindow > 0)
                parryWindow -= (float)deltaTime;
            if (parryWindow < 0)
            {
                endLagTime = parryEndlagS * 10;
                parryWindow = 0;
            }
        }



        // spawn bullet about a point
        private void createBullet()
        {
            Projectile bullet = new Projectile
                (origin: bulletOrigin.getLocation(),
                  width: 30,
                  height: 10,
                  velocity: 50,
                  target: playerBox.getCenter());
            bullet.scaleHitbox(curZoom);
        }



        const int hpIconOffset = 50;

        private void computeHP()
        {
            float xOffset = 0;
            int tempHpStore = currentHp;

            for (int i = 0; i < hpBar.IconCount; i++)
            {
                PointF rectangleOrigin = new PointF(hpBar.Origin.X + xOffset, hpBar.Origin.Y);
                hpBar.HpRectangles[i] = new RectangleF(rectangleOrigin, hpBar.ElementSize);

                switch (tempHpStore)
                {
                    case >= 2:
                        hpBar.HpRecColours[i] = Brushes.Green;
                        break;
                    case 1:
                        hpBar.HpRecColours[i] = Brushes.Orange;
                        break;
                    default:
                        hpBar.HpRecColours[i] = Brushes.DimGray;
                        break;
                }

                xOffset += hpIconOffset * hpBar.ScaleF;
                tempHpStore -= 2;
            }
        }



        private void doPlayerDamage(int amt)
        {
            currentHp -= amt;
            computeHP();
            if (currentHp <= 0)
            {
                togglePause(true);
                MessageBox.Show("you are dead");
                Application.Exit();
            }
        }




        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // if Not parrying then resets parrywindow and sets to parrying
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                if (endLagTime <= 0)
                {
                    parryWindow = (parryDurationS * 10);
                    isParrying = true;
                }
            }
        }



        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            // stops parrying when mouseup but doesnt reset timer > only on mouse down 
            if (e.Button == MouseButtons.Right)
            {
                if (isParrying)
                {
                    endLagTime = parryEndlagS * 10;
                    isParrying = false;
                }
            }
        }



        // pause game
        private void togglePause(bool pause)
        {
            if (pause)
            {
                isPaused = true;
                timer1.Enabled = false;
            }
            else
            {
                isPaused = false;
                timer1.Enabled = true;
            }
        }



        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        List<Projectile> disposedProjectiles = new List<Projectile>();

        Stopwatch fpsTimer = new Stopwatch();
        double deltaFPSTime = 0;
        double prevFPSTime = 0;

        // rendering timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (slowFrame <= 0 && freezeFrame <= 0 && curZoom != 1)
            {
                unZoomScreen(zoomFactor);
                curZoom = 1;
            }


            // debugging/visual indicator for parry
            if (isParrying)
                playerBrush = Brushes.Gray;
            else if (playerIsHit)
                playerBrush = Brushes.Red; // visual hit indicator
            else
                playerBrush = Brushes.Blue;


            deltaFPSTime = 1/(fpsTimer.Elapsed.TotalSeconds - prevFPSTime);
            prevFPSTime = fpsTimer.Elapsed.TotalSeconds;
            label1.Text = deltaFPSTime.ToString("F0");

            Refresh();
            GC.Collect();
        }



        PointF mcPrevCenter; // previous center position of playerBox


        private void zoomScreen(float scaleF)
        {
            curZoom = scaleF;

            // gets center of screen
            float midX = ClientSize.Width / 2;
            float midY = ClientSize.Height / 2;

            mcPrevCenter = playerBox.getCenter();

            void zoomObj(Entity obj)
            {
                float XDiff = obj.getCenter().X - mcPrevCenter.X;
                float YDiff = obj.getCenter().Y - mcPrevCenter.Y;

                float newX = midX + XDiff * scaleF;
                float newY = midY + YDiff * scaleF;

                obj.scaleHitbox(scaleF);
                obj.updateCenter(newX, newY);
                this.Invalidate();
            }


            // calculates new position for each projectile based on distance from playerBox center and adjusts for Scale and the "screen" shifting to the center
            foreach (int chunk in LoadedChunks)
            {
                foreach (Entity e in Entity.EntityList[CurrentLevel][chunk])
                { 
                    if (e == playerBox ) { continue; }
                    zoomOrigins.Add(e,e.getCenter());
                    zoomObj(e); 
                }
            }

            playerBox.updateCenter(midX, midY);
            playerBox.scaleHitbox(scaleF);
        }



        private void unZoomScreen(float scaleF)
        {
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            void unZoomObj(Entity obj, PointF point)
            {
                obj.resetScale();
                obj.updateCenter(point.X, point.Y);
            }


            foreach (KeyValuePair<Entity,PointF> EntityPoints in zoomOrigins)
                unZoomObj(EntityPoints.Key, EntityPoints.Value);

            zoomOrigins.Clear();

            playerBox.updateCenter(mcPrevCenter.X, mcPrevCenter.Y);
            playerBox.resetScale();

            curZoom = 1; // screen is no longer scaled
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e) { threadTokenSrc.Cancel(); }



        private void movementTick()
        {
            if (!LoadedChunks.Contains(0))
                throw new ArgumentException("chunk zero not loaded");

            if (playerBox.IsOnFloor && jumping) { playerBox.doJump(); }
            if (movingLeft) { playerBox.xVelocity -= xAccel; }
            if (movingRight) { playerBox.xVelocity += xAccel; }

            if ((!movingLeft && !movingRight) || (playerBox.CurXColliderDirection != null))
                playerBox.IsMoving = false;
            else
                playerBox.IsMoving = true;


            foreach (int chunk1 in LoadedChunks)
            {
                foreach (Character chara in Character.CharacterList[CurrentLevel][chunk1])
                {
                    if (!playerBox.ShouldDoMove()) { break; }

                    if (playerBox.xVelocity != 0)
                    {
                        if (viewPort.Left < box2.getHitbox().Left) { onWorldBoundary = "left"; }
                        else if (viewPort.Right > box2.getHitbox().Right) { onWorldBoundary = "right"; }
                        else { onWorldBoundary = "null"; }

                        if ((playerBox.getCenter().X < 500) && (playerBox.xVelocity < 0))
                        { scrollLeft = true; }
                        else if ((playerBox.getCenter().X > 1300) && (playerBox.xVelocity > 0))
                        { scrollRight = true; }
                        else
                        {
                            scrollLeft = false;
                            scrollRight = false;
                        }


                        switch (onWorldBoundary)
                        {
                            case "left":
                                scrollLeft = false;
                                break;
                            case "right":
                                scrollRight = false;
                                break;
                        }
                    }

                    bool isScrolling = (scrollRight || scrollLeft);

                    if (isScrolling)
                        //ScrollPlatform(currentLevel: CurrentLevel, velocity: -chara.xVelocity, motionDT);
                        ScrollEntities(currentLevel: CurrentLevel, velocity: -chara.xVelocity, motionDT);

                    foreach (int chunk2 in LoadedChunks)
                    {
                        foreach (Platform plat in Entity.EntityList[CurrentLevel][chunk2].OfType<Platform>())
                            chara.CheckPlatformCollision(plat);
                    }

                    chara.MoveCharacter(
                        isScrolling: isScrolling,
                        dt: motionDT
                    );
                }
            }


            if (playerBox.getCenter().X < chunkLoader1.getCenter().X) { tryLoadChunk(1); }
            else { tryLoadChunk(2); }
        }


        public void tryLoadChunk(int chunk)
        {
            if (LoadedChunks.Contains(chunk)) { return; }

            UnLoadedChunks.Add(LoadedChunks[1]);
            LoadedChunks[1] = chunk;
            UnLoadedChunks.Remove(chunk);
        }




        public void ScrollPlatform(int currentLevel, double velocity, double deltaTime)
        {
            for (int i = 0; i < TotalChunks; i++)
            {
                foreach (Platform plat in Platform.PlatformList[currentLevel][i])
                {
                    plat.updateLocation(plat.getPoint().X + velocity * deltaTime, plat.getPoint().Y);
                }
            }
            chunkLoader1.updateLocation(chunkLoader1.getLocation().X + velocity * deltaTime, chunkLoader1.getLocation().Y);
        }



        public void ScrollEntities(int currentLevel, double velocity, double deltaTime)
        {
            //List<Entity>[][] tempEList = new List<Entity>(Entity.EntityList);
            //foreach (Entity e in Entity.EntityList[CurrentLevel][LoadedChunks[0]])
            

            for (int i = 0; i < TotalChunks; i++)
            {
                foreach( Entity e in Entity.EntityList[CurrentLevel][i])
                {
                    if (e == playerBox) { continue; }
                    e.updateLocation(e.getLocation().X + velocity * deltaTime);

                    //if (Entity.EntityList.Contains(e))
                    //{
                    //    int tempIndex = Entity.EntityList.IndexOf(e);
                    //    Entity.EntityList[tempIndex] = e;
                    //} 
                }
            }
        }



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            foreach (int chunk in LoadedChunks)
            {
                foreach (Character chara in Character.CharacterList[CurrentLevel][chunk])
                {
                    { e.Graphics.FillRectangle(playerBrush, chara.getHitbox()); }
                }
                foreach (Platform plat in Platform.PlatformList[CurrentLevel][chunk])
                {
                    using (Pen redPen = new Pen(Color.Red, 3))
                    { e.Graphics.DrawRectangle(redPen, plat.getHitbox()); }
                }
            }

            foreach (Projectile bullet in Projectile.ProjectileList)
                e.Graphics.FillRectangle(Brushes.Red, bullet.getHitbox());

            for (int i = 0; i < hpBar.IconCount; i++)
            {
                Brush colour = hpBar.HpRecColours[i];
                RectangleF rec = hpBar.HpRectangles[i];
                e.Graphics.FillRectangle(colour, rec);
            }
        }





        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            RectangleF playerBoxHitbox = playerBox.getHitbox();
            switch (e.KeyCode)
            {
                case Keys.A:
                    movingRight = false;
                    movingLeft = true;
                    break;

                case Keys.D:
                    movingLeft = false;
                    movingRight = true;
                    break;

                case Keys.W:
                    if (playerBox.IsOnFloor) { jumping = true; }
                    break;
            }
        }


        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (slowFrame > 0)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                    jumping = false;
                    if (playerBox.yVelocity < 0) { playerBox.yVelocity = 1; }
                    break;

                case Keys.A:
                    movingLeft = false;
                    break;

                case Keys.D:
                    movingRight = false;
                    break;
            }
        }
    }
}
