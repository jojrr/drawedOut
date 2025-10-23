namespace drawedOut
{
    internal class Attacks : Entity
    {
        public static List<Attacks> AttacksList = new List<Attacks>();

        public Attacks(Point origin, int width, int height)
            :base(origin: origin, width: width, height: height)
        {
            AttacksList.Add(this);
        }

        public void Dispose() => AttacksList.Remove(this);
    }
}



