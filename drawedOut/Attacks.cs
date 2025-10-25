namespace drawedOut
{
    internal class Attacks
    {
        Character _parent;
        private float 
            _xOffset,
            _yOffset;
        private int
            _width,
            _height;
        private double _durationS;
        private Entity? _createdAttack;

        public static List<Entity> AttacksList = new List<Entity>();

        public Attacks(Character parent, float xOffset, float yOffset, int width, int height, double durationS)
        {
            _parent = parent;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _width = width;
            _height = height;
            _durationS = durationS;
        }

        public void Dispose()
        {
            if (_createdAttack is null) return;
            AttacksList.Remove(_createdAttack);
            _createdAttack = null;
        }

        public void CreateHitbox() 
        {
            Entity hitbox = new Entity(
                    origin: _parent.Location,
                    width: _width,
                    height: _height);
            _createdAttack = hitbox;
            AttacksList.Add(hitbox);
        }

        public void UpdateHitbox(double dt)
        {
            if (_createdAttack is null) return;
             if (_durationS <= 0) this.Dispose();
            _createdAttack.Center = new PointF(_parent.Center.X + _xOffset, _parent.Center.Y + _yOffset);
            _durationS -= dt;

        }

    }
}



