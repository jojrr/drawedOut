namespace drawedOut
{
    internal class Player : Character
    {
        public int Energy { get => _energy; }
        public double XVelocity { get => xVelocity; }
        public static bool IsParrying { get => _isParrying; }

        private static int _energy;
        private static bool _isParrying = false;
        private static new double _endlagS;
        private const double 
            PARRY_ENDLAG_S = 0.2,
            PARRY_DURATION_S = 0.45,
            PERFECT_PARRY_WINDOW_S = 0.25;
        private static double 
            _parryTimeS = 0,
            _parryEndlagS = 0;

        private static readonly Attacks 
            _basic1 = new Attacks(
                    parent: null,
                    width: 380,
                    height: 220,
                    animation: new AnimationPlayer(@"fillerAnim\"),
                    xOffset: 100,
                    spawn: 2,
                    despawn: 14), 
            _basic2; 

        private static Dictionary<string, bool> _unlockedMoves = new Dictionary<string, bool>();

        static Player()
        {
            _unlockedMoves.Add("move1", false);
            _unlockedMoves.Add("move2", false);
            _unlockedMoves.Add("move3", false);
        }

        public static void UnlockMoves(){}

        public Player(Point origin, int width, int height, int attackPower, int energy, int hp, 
                int xAccel=100, int maxXVelocity=600)
            :base(origin: origin, width: width, height: height, hp: hp, xAccel: xAccel, maxXVelocity: maxXVelocity)
        {
            _energy = energy;
            IsActive = true;
            _basic1.Parent = this;
            setIdleAnim(@"playerChar\idle\");
            setRunAnim(@"playerChar\run\");
        }

        public void DoBasicAttack()
        {
            if (Player._endlagS > 0) return;
            curAttack = _basic1;
            Player._endlagS = 1;
        }

        public void DoDamage(int dmg, ref HpBarUI hpBar)
        {
            IsHit = true;
            Hp -= dmg;
            hpBar.ComputeHP(Hp);
        }

        public void DoParry() { if (!_isParrying && _parryEndlagS <= 0) _isParrying = true; }

        public static void StopParry()
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
            StopParry();
            _parryEndlagS = 0;
        }

        public bool CheckParrying(Attacks atk)
        {
            if (!Hitbox.IntersectsWith(atk.AtkHitbox.Hitbox)) return false;
            if (!IsParrying) return false;
            if (_parryTimeS <= PERFECT_PARRY_WINDOW_S) PerfectParry();
            return true;
        }

        public bool CheckParrying(Projectile proj)
        {
            if (!Hitbox.IntersectsWith(proj.Hitbox)) return false;
            if (!IsParrying) return false;

            if (_parryTimeS <= PERFECT_PARRY_WINDOW_S)
            {
                PerfectParry();
                return false; // returns false as projectile should not be disposed when perfect parried
            }
            return true;
        }


        public void HealPlayer(int heal) => Hp += heal; 

        ///<summary>
        ///reduces endlag by <paramref name="dt"/>
        ///</summary>
        ///<param name="dt"> delta time </param>
        public static void TickEndlagS(double dt) 
        { 
            if (Player._endlagS > 0) Player._endlagS -= dt; 
            if (Player.IsParrying) Player._parryTimeS += dt;
            if (Player._parryEndlagS > 0) Player._parryEndlagS -= dt;
            if (Player._parryTimeS >= PARRY_DURATION_S) Player.StopParry();
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

            if (0 < baseBox.Hitbox.Left) onWorldBoundary = Global.XDirections.left; 
            else if (Global.LevelSize.Width > baseBox.Hitbox.Right) onWorldBoundary = Global.XDirections.right;

            if (Center.X < Global.LeftScrollBound && xVelocity < 0)
                scrollDirection = Global.XDirections.left; 
            else if (Center.X > Global.RightScrollBound && xVelocity > 0)
                scrollDirection = Global.XDirections.right;

            if (onWorldBoundary == scrollDirection) return false;
            if (scrollDirection is null) return false;
            return true;
        }

        public override void CheckActive() { }
    }
}

