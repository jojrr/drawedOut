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

        public static List<Entity> AttacksList = new List<Entity>();
        private List<Entity> _createdAttacks = new List<Entity>();

        public Attacks(Character parent, float xOffset, float yOffset, int width, int height)
        {
            _parent = parent;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _width = width;
            _height = height;
        }

        public void Dispose()
        {
            foreach (Entity e in _createdAttacks)
                AttacksList.Remove(e);
            _createdAttacks.Clear();
        }

        public void CreateHitbox() 
        {
            Entity hitbox = new Entity(
                    origin: _parent.Location,
                    width: _width,
                    height: _height);
            _createdAttacks.Add(hitbox);
            AttacksList.Add(hitbox);
        }
    }
}



