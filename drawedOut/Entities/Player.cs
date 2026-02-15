namespace drawedOut
{
    internal class Player : Character
    {
        public double Energy { get => _energy; }
        public double MaxEnergy { get => _maxEnergy; }
        public double XVelocity { get => xVelocity; }
        public bool IsParrying { get => _isParrying; }

        private static HpBarUI _hpBar;
        private const int 
            PASSIVE_ENERGY_GAIN_S = 6,
            PASSIVE_GAIN_LIMIT = 50,
            PARRY_ENERGY_GAIN = 10;
        private const double 
            _HIT_IFRAMES_S = 0.5,
            PARRY_ENDLAG_S = 0.2,
            PARRY_DURATION_S = 0.65,
            PERFECT_PARRY_WINDOW_S = 0.25;
        private double 
            _parryTimeS = 0,
            _parryEndlagS = 0;
        public bool IsHit { get; private set; }
        private bool _isParrying = false;
        private double _energy, _maxEnergy;

        private readonly Attacks 
            _basic1 = new Attacks(
                    parent: null,
                    width: 180,
                    height: 180,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 100,
                    spawn: 2,
                    despawn: 6,
                    endlag: 1),
            _basic2 = new Attacks(
                    parent: null,
                    width: 240,
                    height: 180,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 100,
                    spawn: 2,
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

        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();

        static Player()
        {
            _unlockedMoves.Add("move1", true);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);
        }

        public static void UnlockMoves(){}

        public Player(Point origin, int width, int height, int attackPower, int energy, int hp, 
                int xAccel=100, int maxXVelocity=600)
            :base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        {
            _energy = 0;
            _maxEnergy = 100;
            IsActive = true;
            setIdleAnim(@"playerChar\idle\");
            setRunAnim(@"playerChar\run\");

            _basic1.Parent = this;
            _basic2.Parent = this;
            _special1.Parent = this;
        }

        public static void LinkHpBar(ref HpBarUI hpBar) => _hpBar = hpBar;

        public void DoSpecial1()
        {
            const int energyCost = 30;
            if (!_unlockedMoves["move1"]) return;
            if (_energy < energyCost) return;
            _energy -= energyCost;
            curAttack = _special1;
        }

        public void DoBasicAttack()
        {
            if (endlagS > 0) 
            {
                if (curAttack == _basic1 && !curAttack.IsActive) DoBasicAttack2();
                return;
            }
            curAttack = _basic1;
        }
        public void DoBasicAttack2() => curAttack = _basic2;

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
            Level0.SlowTime();
            Level0.ZoomScreen();
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

        public void UpdateEnergy(double energy) => _energy = Math.Min(energy, _maxEnergy);
        public void HealPlayer(int heal) => Hp += heal; 

        private new void TickAllCounters(double dt)
        { 
            base.TickAllCounters(dt); 
            _parryEndlagS = Math.Max((_parryEndlagS-dt),0); 
        }

        ///<summary>
        ///reduces endlag by <paramref name="dt"/>
        ///</summary>
        ///<param name="dt"> delta time </param>
        public void TickCounters(double dt) 
        { 
            IsHit = false;
            TickAllCounters(dt);
            if (IsParrying) _parryTimeS += dt;
            if (_parryTimeS >= PARRY_DURATION_S) StopParry();
            if (_energy < PASSIVE_GAIN_LIMIT) UpdateEnergy(_energy+(dt*PASSIVE_ENERGY_GAIN_S));
        }


        public override Bitmap NextAnimFrame()
        {
            if (runAnim is null || idleAnim is null) throw new Exception("Player runAnim or idle null");
            if (curAttack is null)
            {
                if (yVelocity == 0)
                {
                    if (curXAccel == 0) return idleAnim.NextFrame(FacingDirection);
                    return runAnim.NextFrame(FacingDirection);
                }
                /*
                else if (yVelocity > 0)
                {
                    return fallAnim.NextFrame(FacingDirection);
                }
                else 
                {
                    return jumpAnim.NextFrame(FacingDirection);
                }
                */
                return idleAnim.NextFrame(FacingDirection);
            }

            if (curAttack.Animation.CurFrame == curAttack.Animation.LastFrame)
            {
                Bitmap atkAnim = curAttack.NextAnimFrame(FacingDirection);
                curAttack = null;
                return atkAnim;
            }

            return curAttack.NextAnimFrame(FacingDirection);
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

