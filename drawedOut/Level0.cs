using System.Diagnostics;

// TODO: move all const values into global
namespace drawedOut
{
    public partial class Level0 : Form
    {

        private static Character playerBox = new(
            origin: new Point(750, 250),
            width: 50,
            height: 50,
            yVelocity: -0.2);

        private static Platform box2 = new(
           origin: new Point(1, 650),
           width: 5400,
           height: 550,
           LocatedLevel: 0,
           LocatedChunk: 0);

         private static Platform box3 = new(
           origin: new Point(300, 200),
           width: 400,
           height: 175,
           LocatedLevel: 0,
           LocatedChunk: 1);

         private static Platform box4 = new(
           origin: new Point(1000, 400),
           width: 200,
           height: 300,
           LocatedLevel: 0,
           LocatedChunk: 1);

        private static Platform box5 = new(
           origin: new Point(1500, 400),
           width: 200,
           height: 300,
           LocatedLevel: 0,
           LocatedChunk: 2);

        private static Entity chunkLoader1 = new Entity(
            origin: new Point(1200, 0),
            width: 1,
            height: 1);


        private static Global.XDirections? onWorldBoundary = Global.XDirections.left;
        private static Global.XDirections? scrollDirection = null;

        private const double xAccel = 100; // TODO: move to player/character

        private static Brush playerBrush; // TODO: remove when player sprites is added

        // point where bullets spawn
        // TODO: remove when enemies are added
        private static Entity bulletOrigin = new Entity(
            origin: new PointF(800, 250),
            width: 1,
            height: 1);


        private static HpBarUI hpBar = new HpBarUI(
                    origin: new PointF(70, 50),
                    barWidth: 20,
                    barHeight: 40,
                    maxHp: 6
            );


        private static Dictionary<Entity, PointF> zoomOrigins = new Dictionary<Entity, PointF>();

        private static bool
            gameTickEnabled = true,
            showHitbox = true,

            movingLeft = false,
            movingRight = false,
            jumping = false,

            playerIsHit = false,
            isParrying = false,

            isPaused = false,
            slowedMov = false;


        private static int
            maxHp = 6,
            currentHp,
            gameTickFreq = 60,
            gameTickInterval;


        private const float 
            ZOOM_FACTOR = 1.2F,
            SLOW_FACTOR = 1.5F,
            SLOW_DURATION_S = 0.35F,

            FREEZE_DURATION_S = 0.15F,

            PARRY_DURATION_S = 0.45F,
            PERFECT_PARRY_WINDOW_S = 0.05F,
            PARRY_ENDLAG_S = 0.2F;

            //bulletCooldownS = 0.5F, // NOTE: remove when enemy code added

        private static float
            bulletInterval, // TODO: move into enemy class

            parryWindowS,
            endLagTime,

            curZoom = 1,
            MidX,
            MidY,
            // TODO: redo freeze and slow logic
            slowTimeS = 0,
            freezeTimeS = 0;

        private static double motionDT = 0; // TODO: get rid of this bruhv lowk useless like wth

        // threading 
        // used for closing the thread
        private static CancellationTokenSource cancelTokenSrc = new CancellationTokenSource(); 
        private static Thread gameTickThread;
        private static ParallelOptions threadSettings = new ParallelOptions();
        private static Stopwatch deltaTimeSW = new Stopwatch();

        public Level0()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            threadSettings.MaxDegreeOfParallelism = 4;
            threadSettings.CancellationToken = cancelTokenSrc.Token;

            // set height and width of window
            Width = (int)(Global.BaseSize.Width*Global.BaseScale);
            Height = (int)(Global.BaseSize.Height*Global.BaseScale);

            // sets the refresh interval
            gameTickInterval = (int)(1000.0F / gameTickFreq);

            Stopwatch threadDelaySW = Stopwatch.StartNew();
            CancellationToken threadCT = cancelTokenSrc.Token;

            gameTickThread = new Thread(() =>
            {
                while (gameTickEnabled)
                {
                    if (threadCT.IsCancellationRequested)
                        return;

                    if (threadDelaySW.Elapsed.TotalMilliseconds >= gameTickInterval)
                    {
                        double deltaTime = getDeltaTime();
                        threadDelaySW.Restart();

                        if (isPaused) continue;

                        // TODO: review delta time logic
                        movementTick(deltaTime);
                        attackHandler(deltaTime); 

                        try { Invoke(renderGraphics); } 
                        catch (ObjectDisposedException) { return; }
                    }
                }
            });

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            bulletInterval = bulletCooldownS;

            currentHp = maxHp;
            hpBar.UpdateMaxHp(maxHp);

            playerBrush = Brushes.Blue;

            deltaTimeSW.Start();

            if (gameTickThread is null) throw new Exception("gameTickThread not initialsed");
            gameTickThread.Start();

            fpsTimer.Start();

            togglePause(false);
        }


        private double getDeltaTime()
        {
            double deltaTime = deltaTimeSW.Elapsed.TotalSeconds;
            deltaTimeSW.Restart();

            isPaused = false;
            if (freezeTimeS > 0)
            {
                freezeTimeS -= (float)deltaTime;
                isPaused = true;
            }
            return deltaTime;
        }


        private void slowTime(double deltaTime)
        {
            if (slowTimeS > 0)
            {
                slowedMov = true;

                if (ZOOM_FACTOR <= 1)
                    throw new ArgumentException("ZOOM_FACTOR must be bigger than 1");

                slowTimeS -= (float)deltaTime;

                deltaTime /= SLOW_FACTOR;
                motionDT = (ZOOM_FACTOR * deltaTime);
            }
            else 
            {
                slowTimeS = 0;
                if (slowedMov) // keeps the player in motion when in slow 
                {
                    movingLeft = false;
                    movingRight = false;
                    slowedMov = false;
                }
            }
        }


        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        private static List<Projectile> disposedProjectiles = new List<Projectile>();

        private void attackHandler(double deltaTime)
        {
            playerIsHit = false;
            freezeTimeS = 0;

            if (endLagTime > 0)
            {
                endLagTime -= (float)deltaTime;
                isParrying = false;
            }

            try // TODO: remove the 999999 nested br
            {
                Parallel.ForEach(Projectile.ProjectileList, threadSettings, bullet =>
                {
                    bullet.moveProjectile(motionDT);
                    PointF bLoc = bullet.Center;

                    foreach (Platform p in Platform.ActivePlatformList)
                    {
                        if (disposedProjectiles.Contains(bullet)) break;

                        if (!(p.Hitbox.IntersectsWith(bullet.Hitbox)))
                            continue;

                        disposedProjectiles.Add(bullet);
                        break;

                    }


                    if ((bLoc.X < 0) || (bLoc.Y < 0) || (bLoc.X > ClientSize.Width) || (bLoc.Y > ClientSize.Height))
                    {
                        disposedProjectiles.Add(bullet);
                        return;
                    }

                    if (!playerBox.Hitbox.IntersectsWith(bullet.Hitbox))
                        return;

                    if (!isParrying)
                    {
                        freezeTimeS = FREEZE_DURATION_S * 10;
                        disposedProjectiles.Add(bullet);
                        doPlayerDamage(1);
                        return;
                    }

                    bullet.rebound(playerBox.Center); // required to prevent getting hit anyway when parrying

                    // if the current parry has lasted for at most the perfectParryWindow
                    if (parryWindowS >= (PARRY_DURATION_S - PERFECT_PARRY_WINDOW_S) * 10)
                    {
                        slowTimeS = SLOW_DURATION_S * 10;
                        zoomScreen(ZOOM_FACTOR);
                        isParrying = false;
                        endLagTime = 0;
                    }
                    else disposedProjectiles.Add(bullet);

                });
            }
            catch (OperationCanceledException) { return; }

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
            if (isParrying && parryWindowS > 0)
                parryWindowS -= (float)deltaTime;
            if (parryWindowS < 0)
            {
                endLagTime = PARRY_ENDLAG_S * 10;
                parryWindowS = 0;
            }

            if (currentHp > maxHp) currentHp = maxHp;
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
                    parryWindowS = (PARRY_DURATION_S * 10);
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
                    endLagTime = PARRY_ENDLAG_S * 10;
                    isParrying = false;
                }
            }
        }


        // pause game
        private void togglePause(bool pause) => isPaused = !isPaused; 


        private static Stopwatch fpsTimer = new Stopwatch();

        // rendering graphics method
        private void renderGraphics()
        {
            if (playerBrush is null) throw new Exception("playerBrush is not defined");

            if (slowTimeS <= 0 && freezeTimeS <= 0 && curZoom != 1)
            {
                unZoomScreen();
                curZoom = 1;
            }

            // debugging/visual indicator for parry
            if (isParrying)
                playerBrush = Brushes.Gray;
            else if (playerIsHit)
                playerBrush = Brushes.Red; // visual hit indicator
            else
                playerBrush = Brushes.Blue;


            float deltaFPSTime = Convert.ToSingle(1/(fpsTimer.Elapsed.TotalSeconds));
            fpsTimer.Restart();
            label1.Text = deltaFPSTime.ToString("F0");

            Refresh();
        }



        private void zoomScreen(float scaleF)
        {
            curZoom = scaleF;

            float playerX = playerBox.Center.X;
            float playerY = playerBox.Center.Y;

            void zoomObj(Entity obj)
            {
                float _xDiff = obj.Center.X - playerX;
                float _yDiff = obj.Center.Y - playerY;

                float newX = MidX + _xDiff * scaleF;
                float newY = MidY + _yDiff * scaleF;

                obj.ScaleHitbox(scaleF);
                obj.Center = new PointF (newX, newY);
                this.Invalidate();
            }


            // calculates new position for each projectile based on distance from playerBox center and adjusts for Scale and the "screen" shifting to the center
            foreach (Entity e in Entity.EntityList)
            {
                zoomOrigins.Add(e,e.Center);
                if (e == playerBox)
                {
                    playerBox.Center = new PointF (MidX, MidY);
                    playerBox.ScaleHitbox(scaleF);
                    continue;
                }
                zoomObj(e); 
            }
        }



        private void unZoomScreen()
        {
            foreach (KeyValuePair<Entity, PointF> EntityPoints in zoomOrigins)
            { 
                Entity obj = EntityPoints.Key;
                PointF point = EntityPoints.Value;
                obj.ResetScale();
                obj.Center = point;
            }

            playerBox.ResetScale();
            zoomOrigins.Clear();
            curZoom = 1; // screen is no longer scaled
        }


        private void movementTick(double deltaTime)
        {
            slowTime(deltaTime);

            // TODO: functionize and optimise
            
            if (playerBox.IsOnFloor && jumping) { playerBox.DoJump(); }
            if (movingLeft) { playerBox.xVelocity -= xAccel*deltaTime; }
            if (movingRight) { playerBox.xVelocity += xAccel*deltaTime; }



            foreach (Character chara in Character.ActiveCharacters) // NOTE: try parallel foreach
            {
                // TODO: move all the player dependent code into player class
                if (!playerBox.ShouldDoMove()) { break; }

                if (playerBox.xVelocity != 0)
                {
                    if (0 < box2.Hitbox.Left) onWorldBoundary = Global.XDirections.left; 
                    else if (Width > box2.Hitbox.Right) onWorldBoundary = Global.XDirections.right;
                    else onWorldBoundary = null;

                    if ((playerBox.Center.X < 500) && (playerBox.xVelocity < 0)) scrollDirection = Global.XDirections.left;
                    else if ((playerBox.Center.X > 1300) && (playerBox.xVelocity > 0)) scrollDirection = Global.XDirections.right;
                    else scrollDirection = null;

                    if (onWorldBoundary == scrollDirection) scrollDirection = null;
                }

                bool isScrolling = (scrollDirection is not null);

                if (isScrolling) ScrollEntities( velocity: -chara.xVelocity, deltaTime);

                bool colliding = false; // HACK: temporary solution - should remove when on-screen loading is implemented

                foreach (Platform plat in Platform.ActivePlatformList)
                {
                    chara.CheckPlatformCollision(plat);
                    if (chara.Hitbox.IntersectsWith(plat.Hitbox)) colliding = true;
                }

                if (!colliding) chara.SetYCollider(null, null, null);

                chara.MoveCharacter(
                    isScrolling: isScrolling,
                    dt: deltaTime
                );
            }
        }




        public void ScrollEntities(double velocity, double deltaTime)
        {
            foreach( Entity e in Entity.EntityList)
            {
                if (e == playerBox) { continue; }
                e.UpdateX(velocity * deltaTime);
            }
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (showHitbox)
            {
                foreach (Character chara in Character.ActiveCharacters)
                    e.Graphics.FillRectangle(playerBrush, chara.Hitbox);

                foreach (Platform plat in Platform.ActivePlatformList)
                {
                    using (Pen redPen = new Pen(Color.Red, 3))
                    { e.Graphics.DrawRectangle(redPen, plat.Hitbox); }
                }

                foreach (Projectile bullet in Projectile.ProjectileList)
                    e.Graphics.FillRectangle(Brushes.Red, bullet.Hitbox);

                for (int i = 0; i < hpBar.IconCount; i++)
                    e.Graphics.FillRectangle(hpBar.HpRecColours[i], hpBar.HpRectangles[i]);
            }
        }



        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
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
            if (slowTimeS > 0)
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            gameTickEnabled = false;    
            cancelTokenSrc.Cancel();
        }
    }
}
