namespace drawedOut
{
    internal class Item:Entity
    {
        private static HashSet<Item> _itemList = new HashSet<Item>();
        private readonly Action _doAction;
        private bool _isOnFloor = false;
        private Entity? _yStickEntity;
        private float? _yStickLevel;
        private int _yVelocity = 1;
        private Bitmap _sprite;

        private static readonly int 
            _TERMINAL_VELOCITY = (int)(Global.BaseScale * 10000),
            _COLLISION_RANGE = (int)(Global.BaseScale * 400),
            _GRAVITY = Global.Gravity*2;
        private readonly bool _hasGravity;

        /// <summary> Constructor </summary>
        /// <param name="origin"> The center of the item </param>
        /// <param name="hasGravity"> 
        /// If the item is affected by gravity <br/>
        /// Default: true
        /// </param>
        public Item(PointF origin, int width, int height, Bitmap sprite, Action action,
                bool hasGravity=true)
            : base(origin: origin, width: width, height: height)
        { 
            Center = origin;
            _sprite = sprite;
            _doAction = action;
            _hasGravity = hasGravity; 
            _itemList.Add(this);
        }

        public void CheckPlatformCollision(Entity target, double dt)
        {
            if (_yVelocity == 0) return;
            float finalY = Hitbox.Bottom + (float)(_yVelocity*dt*Global.BaseScale);
            RectangleF targetHitbox = target.Hitbox;
            if (finalY > target.LocationY && Center.X > targetHitbox.Left && Center.X < targetHitbox.Right)
            {
                SetYCollider(true, targetHitbox.Y, target);
                _yVelocity = 0;
            }
            else if (target == _yStickEntity) SetYCollider(false, null, null);
        }

        /// <summary>
        /// Defines the current character's Y collider
        /// </summary>
        /// <param name="y">bottom, top or null </param>
        /// <param name="targetHitbox"></param>
        /// <param name="collisionTarget"></param>
        private void SetYCollider(bool floorFound, float? targetY, Entity? collisionTarget)
        {
            _yStickEntity = collisionTarget;
            if (targetY is not null) LocationY = targetY.Value - Hitbox.Height;
        }

        private void SetInactive()
        {
            IsActive = false;
            _itemList.Remove(this);
        }

        public void DoGravTick(double dt)
        {
            if (!_hasGravity || _isOnFloor || !IsActive) return;
            _yVelocity = Math.Min(_yVelocity+(int)(_GRAVITY*dt), _TERMINAL_VELOCITY); 
            foreach (Platform p in Platform.ActivePlatformList) CheckPlatformCollision(p, dt);
            LocationY += (int)(_yVelocity*dt*Global.BaseScale);
        }

        public static void DoAllGravTick(double dt)
        { foreach (Item item in Item._itemList) item.DoGravTick(dt); }

        public void Pickup()
        {
            _doAction();
            Reset();
        }

        public override void Reset() 
        {
            SetInactive();
            Delete();
        }

        public static void CheckPlayerCollisions(RectangleF playerHitbox) 
        { 
            foreach (Item item in Item._itemList) 
            { 
                if (Math.Abs(item.LocationX - playerHitbox.X) > _COLLISION_RANGE) continue;
                if (item.Hitbox.IntersectsWith(playerHitbox)) item.Pickup(); 
            }
        }

        public static void Draw(Graphics g)
        { foreach (Item item in Item._itemList) g.DrawImage(item._sprite, item.Hitbox); }

        public override void CheckActive()
        {
            if (Location.Y > Global.LevelSize.Height) 
            {
                Reset(); 
                return;
            }
            if (this.DistToMid > Global.EntityLoadThreshold) 
            {
                if (!IsActive) return;
                SetInactive();
            }
            else if (!IsActive) 
            {
                IsActive = true;
                _itemList.Add(this);
            }
        }
    }
}

