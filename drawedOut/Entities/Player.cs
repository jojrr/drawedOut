namespace drawedOut
{
    internal class Player : Character
    {
        public new static byte MaxHp = 6;
        public static UInt16 MaxEnergy = 100;
        public static bool[] UnlockedMoves;

        public bool IsHit { get; private set; }
        public double Energy { get => _energy; }
        public double XVelocity { get => xVelocity; }
        public bool IsParrying { get => _isParrying; }
        public static readonly int[] SpecialEnergyCosts = new int[3] { 30, 10, 10 };

        private static HpBarUI _hpBar;
        private static Action? _queueAtk;
        private static AnimationPlayer _jumpAnim, _fallAnim;
        private static readonly Bitmap _projectileSprite, _ultSprite;
        private const int 
            PASSIVE_ENERGY_GAIN_S = 6,
            PASSIVE_GAIN_LIMIT = 50,
            PARRY_ENERGY_GAIN = 10;
        private const double 
            _HIT_IFRAMES_S = 0.5,
            PARRY_ENDLAG_S = 0.2,
            PARRY_DURATION_S = 0.65,
            PERFECT_PARRY_WINDOW_S = 0.25;

        private bool _isParrying = false;
        private Level0 _curLvl;
        private double 
            _energy,
            _parryTimeS = 0,
            _parryEndlagS = 0,
            _movementEndlagS = 0;

        private static readonly Attacks 
            _basic1 = new Attacks(
                    parent: null,
                    width: 180,
                    height: 180,
                    animation: new AnimationPlayer(@"playerChar\basic1\"),
                    xOffset: 100,
                    spawn: 3,
                    despawn: 7,
                    endlag: 0.3f),
            _basic2 = new Attacks(
                    parent: null,
                    width: 240,
                    height: 180,
                    animation: new AnimationPlayer(@"playerChar\basic2\"),
                    xOffset: 100,
                    spawn: 3,
                    despawn: 8,
                    endlag: 1.5F),
            _special1 = new Attacks(
                    parent: null,
                    width: 500,
                    height: 100,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    spawn: 4,
                    xOffset: 100,
                    despawn: 12,
                    endlag: 0.5F,
                    isLethal: true);
        private static readonly ProjectileAttack 
            _special2 = new ProjectileAttack(
                parent: null,
                animation: new AnimationPlayer(@"fillerAnim\"),
                endlag: 1.5f,
                spawn: 1,
                projectileEvent: ()=>{}
            ),
            _special3 = new ProjectileAttack(
                parent: null,
                animation: new AnimationPlayer(@"fillerAnim\"),
                endlag: 2f,
                spawn: 10,
                projectileEvent: ()=>{}
            );


        static Player()
        {
            UnlockedMoves = new bool[3] { true, false, false };
            _projectileSprite = Global.GetSingleImage(@"fillerAnim\");
            _ultSprite = Global.GetSingleImage(@"fillerAnim\");
            _jumpAnim = new AnimationPlayer(@"playerChar\jumpAnim\");
            _fallAnim = new AnimationPlayer(@"playerChar\fallAnim\");
        }


        public Player(Point origin, int width, int height, int attackPower, int energy, Level0 curLevel,
                int xAccel=100, int maxXVelocity=600)
            :base(origin: origin, width: width, height: height, xAccel: xAccel, maxXVelocity: maxXVelocity,
                 hp: MaxHp)
        {
            _energy = 0;
            IsActive = true;
            _curLvl = curLevel;

            setIdleAnim(@"playerChar\idle\");
            setRunAnim(@"playerChar\run\");

            _fallAnim.ResetAnimation();
            _jumpAnim.ResetAnimation();

            _basic1.Reset();
            _basic2.Reset();
            _special1.Reset();
            _special2.Reset();
            _special3.Reset();

            _basic1.Parent = this;
            _basic2.Parent = this;
            _special1.Parent = this;
            _special2.Parent = this;
            _special2.SetEvent(fireSpecial2);
            _special3.Parent = this;
            _special3.SetEvent(fireSpecial3);
        }

        public static void LinkHpBar(ref HpBarUI hpBar) => _hpBar = hpBar;

# region player attacks
        public void DoSpecial(byte moveNo)
        {
            int energyCost = SpecialEnergyCosts[moveNo];
            if (!UnlockedMoves[moveNo]) return;
            if (_energy < energyCost) return;
            _energy -= energyCost;

            switch (moveNo)
            {
                case 0:
                    curAttack = _special1;
                    break;
                case 1:
                    curAttack = _special2;
                    break;
                case 2:
                    curAttack = _special3;
                    break;
            }
        }

        public void DoBasicAttack()
        {
            if (curAttack == _basic1) 
            {
                _queueAtk = DoBasicAttack2;
                return;
            }
            if (curAttack is null && endlagS == 0) curAttack = _basic1;
        }
        private void DoBasicAttack2() 
        {
            if (curAttack != _basic1 || endlagS <= 0) return;
            _basic1.Reset();
            _basic2.Reset();
            curAttack = _basic2;
        }
        private void fireSpecial2()
        {
            Projectile special2Proj = new Projectile(
                origin: this.Center,
                width: 100,
                height: 100,
                velocity: 1400,
                maxSpeed: 5000,
                angle: 0,
                xDiff: (FacingDirection == Global.XDirections.right) ? 1: -1,
                yDiff: 1,
                accel: 3000,
                sprite: _projectileSprite,
                parent: this
            );
        }
        private void fireSpecial3()
        {
            iFrames = 1;
            int xOffset = (FacingDirection == Global.XDirections.right) ? 367 : -367;
            PlayerUltProjectile special3Proj = new PlayerUltProjectile(
                origin: new PointF(this.Center.X + xOffset, -6400),
                width: 500,
                accel: 3000,
                height: 6400,
                velocity: 2200,
                maxSpeed: 10000,
                angle: Math.PI/2,
                dmg:3,
                sprite: _ultSprite,
                parent: this
                );
        }
# endregion

        public void DoDamage(Projectile sourceProjectile)
        {
            if (iFrames>0) return;
            IsHit = true;
            Hp -= sourceProjectile.Dmg;
            _hpBar.ComputeHP(Hp);
            int[] knockBackVelocites = sourceProjectile.calculateKnockback(this.Center);
            ApplyKnockBack(knockBackVelocites[0], knockBackVelocites[1], 0, 0);
            iFrames += _HIT_IFRAMES_S;
        }


        public void DoDamage(Attacks sourceAttack, int xSpeed=1000, int ySpeed=500)
        {
            if (iFrames>0) return;
            IsHit = true;
            Hp -= sourceAttack.AtkDmg;
            _hpBar.ComputeHP(Hp);
            PointF sourceCenter = sourceAttack.Parent.Center;
            if (sourceCenter.X - this.Center.X > 0) xSpeed *= -1;
            if (sourceCenter.Y - this.Center.Y > 0) ySpeed *= -1;
            ApplyKnockBack(xSpeed, ySpeed, 0, 0); 
            iFrames += _HIT_IFRAMES_S;
        }

        ///<summary>
        ///Reset the player to the state that the player was initialised in
        ///</summary>
        public override void Reset()
        {
            base.Reset();
            _energy = 0;
        }

        public void DoParry() { if (!_isParrying && _parryEndlagS <= 0) _isParrying = true; }

        public void StopParry()
        {
            if (!_isParrying) return;
            _parryEndlagS = PARRY_ENDLAG_S;
            _isParrying = false;
            _parryTimeS = 0;
        }

        private void PerfectParry()
        {
            _curLvl.SlowTime();
            _curLvl.ZoomScreen();
            _energy += (int)(PARRY_ENERGY_GAIN*0.5);
            StopParry();
            _parryEndlagS = 0;
        }

        public bool CheckParrying(Attacks atk)
        {
            if (!Hitbox.IntersectsWith(atk.AtkHitbox.Hitbox)) return false;
            if (!IsParrying) return false;
            if (_parryTimeS <= PERFECT_PARRY_WINDOW_S) PerfectParry();
            _energy += PARRY_ENERGY_GAIN;
            UpdateEnergy(_energy);
            return true;
        }

        /// <summary>
        /// check for parry state against a projectile
        /// </summary>
        /// <param name="projectile"> the projectile to check for collision with </param>
        /// <returns> boolean true = the projectile should be destroyed </returns>
        public bool CheckParrying(Projectile projectile, double dt)
        {
            if (!Hitbox.IntersectsWith(projectile.Hitbox)) return true;
            if (!IsParrying)
            {
                int[] knockBackVelocites = projectile.calculateKnockback(this.Center);
                DoDamage(projectile);
                return true;
            }
            if (_parryTimeS <= PERFECT_PARRY_WINDOW_S) 
            {
                projectile.Rebound(dt, this);
                PerfectParry(); 
                return false;
            }
            _energy += PARRY_ENERGY_GAIN;
            return true;
        }

        public void UpdateEnergy(double energy) => _energy = Math.Min(energy, MaxEnergy);
        public void HealPlayer(int heal) => Hp += heal; 

        ///<summary>
        ///reduces endlag by <paramref name="dt"/>
        ///</summary>
        ///<param name="dt"> delta time </param>
        public new void TickAllCounters(double dt)
        { 
            IsHit = false;
            base.TickAllCounters(dt); 
            _parryEndlagS = Math.Max((_parryEndlagS-dt),0); 
            if (IsParrying) _parryTimeS += dt;
            if (_parryTimeS >= PARRY_DURATION_S) StopParry();
            if (_energy < PASSIVE_GAIN_LIMIT) UpdateEnergy(_energy+(dt*PASSIVE_ENERGY_GAIN_S));
        }

        public bool TryQueuedAttack()
        {
            if (_queueAtk is null) return false;
            _queueAtk();
            _queueAtk = null;
            return true;
        }


        public override Bitmap NextAnimFrame()
        {
            if (runAnim is null || idleAnim is null) throw new Exception("Player runAnim or idle null");

            if (curAttack is not null)
            {
                if (!curAttack.IsActive) TryQueuedAttack();
                if (curAttack.Animation.CurFrame == curAttack.Animation.LastFrame)
                {
                    Bitmap atkAnim = curAttack.NextAnimFrame(FacingDirection);
                    curAttack.Reset();
                    curAttack = null;
                    return atkAnim;
                }

                return curAttack.NextAnimFrame(FacingDirection);
            }
            else if (TryQueuedAttack()) NextAnimFrame(); 

            if (yVelocity > 0) return _fallAnim.NextFrame(FacingDirection); 
            else if (yVelocity < 0) return _jumpAnim.NextFrame(FacingDirection);

            if (curXAccel == 0) return idleAnim.NextFrame(FacingDirection);
            return runAnim.NextFrame(FacingDirection);
        }


        /// <summary>
        /// Check if the level should scroll
        /// </summary>
        /// <param name="baseBox"> The base rectangle that defines the bounds of the level. </param>
        public bool CheckScrolling(Platform baseBox)
        {
            Global.XDirections? onWorldBoundary = null;
            Global.XDirections? scrollDirection = null;

            if (!ShouldDoMove())  return false;
            if (Global.LeftScrollBound<=Center.X && Center.X<=Global.RightScrollBound) return false;

            if (0 <= baseBox.Hitbox.X) onWorldBoundary = Global.XDirections.left; 
            else if (Global.LevelSize.Width >= baseBox.Hitbox.Right) onWorldBoundary = Global.XDirections.right;

            if (Center.X < Global.LeftScrollBound && xVelocity < 0)
                scrollDirection = Global.XDirections.left; 
            else if (Center.X > Global.RightScrollBound && xVelocity > 0)
                scrollDirection = Global.XDirections.right;

            if (onWorldBoundary == scrollDirection) return false;
            if (onWorldBoundary == Global.XDirections.right) return false;
            if (scrollDirection is null) return false;
            return true;
        }

        public override void CheckActive() 
        { if (!checkInBoundary()) Hp = 0; }
    }
}
