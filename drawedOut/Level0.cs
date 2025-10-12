using System.Diagnostics;
namespace drawedOut
{
    public partial class Level0 : Form
    {

        Character playerBox = new(
            origin: new Point(750, 250),
            width: 50,
            height: 50,
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

        const double xAccel = 100; // TODO: move to player/character

        // TODO: replace chunk logic with "Load if on screen" logic
        int CurrentLevel;
        int[] LoadedChunks = new int[2];
        List<int> UnLoadedChunks;
        int TotalChunks;



        Brush playerBrush; // TODO: remove when player sprites is added

        // point where bullets spawn
        // TODO: remove when enemies are added
        Entity bulletOrigin = new Entity (
            origin: new PointF(800, 250),
            width: 1,
            height: 1,
            chunk: 1);

        // threading 
        CancellationTokenSource threadTokenSrc = new CancellationTokenSource(); // used for closing the thread
        Thread gameTickThread = new Thread(() => { });

        HpBarUI hpBar = new HpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    maxHp: 6
            );


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
            targetFrameRate = 60,
            refreshDelay;


        const float
            zoomFactor = 1.2F,
            slowFactor = 1.5F,
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
            motionDT = 0,
            deltaTime = 0;

        ParallelOptions threadSettings = new ParallelOptions();
        Stopwatch deltaTimeSW = new Stopwatch();

        public Level0()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            threadSettings.MaxDegreeOfParallelism = 4;

            // set height and width of window
            Width = 1860;
            Height = 770;

            // sets the refresh interval
            refreshDelay = (int)(1000.0F / targetFrameRate);
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            CurrentLevel = 0;
            LoadedChunks = [0, 1];
            UnLoadedChunks = [2];
            TotalChunks = LoadedChunks.Count() + UnLoadedChunks.Count();

            viewPort = new Rectangle(new Point(-5, 0), new Size(Width + 10, Height));

            bulletInterval = bulletCooldownS;

            currentHp = maxHp;
            hpBar.UpdateMaxHp(maxHp);

            playerBrush = Brushes.Blue;

            deltaTimeSW.Start();
            fpsTimer.Start();
            Stopwatch threadDelaySW = Stopwatch.StartNew();

            CancellationToken threadCT = threadTokenSrc.Token;

            gameTickThread = new Thread(() =>
            {
                while (true)
                {
                    if (threadCT.IsCancellationRequested)
                        return;

                    if (threadDelaySW.Elapsed.TotalMilliseconds >= refreshDelay)
                    {
                        getDeltaTime();
                        threadDelaySW.Restart();

                        if (isPaused) continue;

                        Task.Run(movementTick)
                        .ContinueWith(_ => Task.Run(attackHandler))
                        .ContinueWith(_ => Task.Run(renderGraphics));
                    }
                }
            });

            gameTickThread.Start();

            togglePause(false);
        }


        private void getDeltaTime()
        {
            double deltaTime = deltaTimeSW.Elapsed.TotalSeconds * 10;
            motionDT = deltaTime;
            deltaTimeSW.Restart();

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


            if (endLagTime > 0)
            {
                endLagTime -= (float)deltaTime;
                isParrying = false;
            }


            Parallel.ForEach(Projectile.ProjectileList, threadSettings, bullet =>
            {
                bullet.moveProjectile(motionDT);
                PointF bLoc = bullet.Center;

                foreach(Platform p in Platform.ActivePlatformList)
                {
                    if (disposedProjectiles.Contains(bullet)) break;

                    if ( !( p.GetHitbox().IntersectsWith(bullet.GetHitbox()) ) )
                        continue;

                    disposedProjectiles.Add(bullet); 
                    break; 

                }


                if ((bLoc.X<0) || (bLoc.Y<0) || (bLoc.X > ClientSize.Width) || (bLoc.Y > ClientSize.Height))
                { 
                    disposedProjectiles.Add(bullet); 
                    return; 
                }

                bool hitPlayer = playerBox.GetHitbox().IntersectsWith(bullet.GetHitbox());

                if (!hitPlayer) return;

                if (!isParrying)
                {
                    setFreeze = true;
                    disposedProjectiles.Add(bullet);
                    doPlayerDamage(1);
                    return;
                }

                bullet.rebound(playerBox.Center); // required to prevent getting hit anyway when parrying

                // if the current parry has lasted for at most the perfectParryWindow
                if (parryWindow >= (parryDurationS - perfectParryWindowS) * 10)
                {
                    //setFreeze = true;
                    slowFrame = slowDurationS * 10;
                    zoomScreen(zoomFactor);
                    isParrying = false;
                    endLagTime = 0;
                }
                else disposedProjectiles.Add(bullet);

            });

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            if (setFreeze)
                freezeFrame = freezeDuratonS * 10;


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
                (origin: bulletOrigin.Location,
                  width: 30,
                  height: 10,
                  velocity: 50,
                  target: playerBox.Center);
            bullet.ScaleHitbox(curZoom);
        }

        private void doPlayerDamage(int amt)
        {
            playerIsHit = true;
            currentHp -= amt;
            hpBar.ComputeHP(currentHp);
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
        private void togglePause(bool pause) => isPaused = !isPaused; 



        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        List<Projectile> disposedProjectiles = new List<Projectile>();

        Stopwatch fpsTimer = new Stopwatch();
        double deltaFPSTime = 0;
        double prevFPSTime = 0;

        // rendering graphics method
        private void renderGraphics()
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
        }



        PointF mcPrevCenter; // previous center position of playerBox


        private void zoomScreen(float scaleF)
        {
            curZoom = scaleF;

            // gets center of screen
            float midX = ClientSize.Width / 2;
            float midY = ClientSize.Height / 2;

            mcPrevCenter = playerBox.Center;

            void zoomObj(Entity obj)
            {
                float XDiff = obj.Center.X - mcPrevCenter.X;
                float YDiff = obj.Center.Y - mcPrevCenter.Y;

                float newX = midX + XDiff * scaleF;
                float newY = midY + YDiff * scaleF;

                obj.ScaleHitbox(scaleF);
                obj.UpdateCenter(newX, newY);
                this.Invalidate();
            }


            // calculates new position for each projectile based on distance from playerBox center and adjusts for Scale and the "screen" shifting to the center
            foreach (Entity e in Entity.EntityList)
            { 
                if (e == playerBox ) { continue; }
                zoomOrigins.Add(e,e.Center);
                zoomObj(e); 
            }


            playerBox.UpdateCenter(midX, midY);
            playerBox.ScaleHitbox(scaleF);
        }



        private void unZoomScreen(float scaleF)
        {
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            void unZoomObj(Entity obj, PointF point)
            {
                obj.ResetScale();
                obj.UpdateCenter(point.X, point.Y);
            }


            foreach (KeyValuePair<Entity,PointF> EntityPoints in zoomOrigins)
                unZoomObj(EntityPoints.Key, EntityPoints.Value);

            zoomOrigins.Clear();

            playerBox.UpdateCenter(mcPrevCenter.X, mcPrevCenter.Y);
            playerBox.ResetScale();

            curZoom = 1; // screen is no longer scaled
        }


        private void movementTick()
        {
            // checks if slowed
            slowTime();

            // TODO: functionize and optimise
            
            if (!LoadedChunks.Contains(0)) // NOTE: to be removed when chunk loading is replaced    
                throw new ArgumentException("chunk zero not loaded");

            if (playerBox.IsOnFloor && jumping) { playerBox.doJump(); }
            if (movingLeft) { playerBox.xVelocity -= xAccel*motionDT; }
            if (movingRight) { playerBox.xVelocity += xAccel*motionDT; }

            if ((!movingLeft && !movingRight) || (playerBox.CurXColliderDirection != null))
                playerBox.IsMoving = false;
            else
                playerBox.IsMoving = true;


            foreach (int chunk1 in LoadedChunks) // NOTE: try parallel foreach
            {
                foreach (Character chara in Character.ActiveCharacters)
                {
                    if (!playerBox.ShouldDoMove()) { break; }

                    if (playerBox.xVelocity != 0)
                    {
                        if (viewPort.Left < box2.GetHitbox().Left) { onWorldBoundary = "left"; }
                        else if (viewPort.Right > box2.GetHitbox().Right) { onWorldBoundary = "right"; }
                        else { onWorldBoundary = "null"; }

                        if ((playerBox.Center.X < 500) && (playerBox.xVelocity < 0))
                        { scrollLeft = true; }
                        else if ((playerBox.Center.X > 1300) && (playerBox.xVelocity > 0))
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
                        ScrollEntities(currentLevel: CurrentLevel, velocity: -chara.xVelocity, motionDT);

                    bool colliding = false; // HACK: temporary solution - should remove when on-screen loading is implemented
                    foreach (int chunk2 in LoadedChunks)
                    {
                        foreach (Platform plat in Platform.ActivePlatformList)
                        {
                            chara.CheckPlatformCollision(plat);
                            if (chara.GetHitbox().IntersectsWith(plat.GetHitbox())) colliding = true;
                        }
                    }
                    if (!colliding) chara.SetYCollider(null, null, null);

                    chara.MoveCharacter(
                        isScrolling: isScrolling,
                        dt: motionDT
                    );
                }
            }

            if (playerBox.Center.X < chunkLoader1.Center.X) { tryLoadChunk(1); } // NOTE: chunk loading to be removed
            else { tryLoadChunk(2); }
        }


        public void tryLoadChunk(int chunk) // NOTE: chunk loading to be removed
        {
            if (LoadedChunks.Contains(chunk)) { return; }

            UnLoadedChunks.Add(LoadedChunks[1]);
            LoadedChunks[1] = chunk;
            UnLoadedChunks.Remove(chunk);
        }



        public void ScrollEntities(int currentLevel, double velocity, double deltaTime)
        {
            foreach( Entity e in Entity.EntityList)
            {
                if (e == playerBox) { continue; }
                e.UpdateLocation(e.Location.X + velocity * deltaTime);
            }
        }


        // NOTE: chunk loading to be removed
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            foreach (int chunk in LoadedChunks)
            {
                foreach (Character chara in Character.ActiveCharacters)
                {
                    { e.Graphics.FillRectangle(playerBrush, chara.GetHitbox()); }
                }
                foreach (Platform plat in Platform.ActivePlatformList)
                {
                    using (Pen redPen = new Pen(Color.Red, 3))
                    { e.Graphics.DrawRectangle(redPen, plat.GetHitbox()); }
                }
            }

            foreach (Projectile bullet in Projectile.ProjectileList)
                e.Graphics.FillRectangle(Brushes.Red, bullet.GetHitbox());

            for (int i = 0; i < hpBar.IconCount; i++)
            {
                e.Graphics.FillRectangle(hpBar.HpRecColours[i], hpBar.HpRectangles[i]);
            }
        }



        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            RectangleF playerBoxHitbox = playerBox.GetHitbox();
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

                case Keys.S:
                    createBullet();
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) { threadTokenSrc.Cancel(); }

    }
}
