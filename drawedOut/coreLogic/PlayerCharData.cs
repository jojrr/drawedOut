namespace drawedOut
{
    internal static class PlayerCharData
    {
        public static byte MaxHp = 6;
        public static UInt16 MaxEnergy = 6;
        public static bool[] UnlockedMoves = new bool[3] { true, false, false };

        public class PlayerDataInstance
        {
            public byte MaxHp { get; init; }
            public UInt16 MaxEnergy { get; init; }
            public bool[] UnlockedMoves { get; init; }

            public PlayerDataInstance()
            {
                MaxHp = PlayerCharData.MaxHp;
                MaxEnergy = PlayerCharData.MaxEnergy;
                UnlockedMoves = PlayerCharData.UnlockedMoves;
            }
        }

        public static PlayerDataInstance Instance => new PlayerDataInstance();

        public static void LoadInstance(PlayerDataInstance? instance)
        {
            if (instance is null) return;
            MaxHp = instance.MaxHp;
            MaxEnergy = instance.MaxEnergy;
            UnlockedMoves = instance.UnlockedMoves;
        }
    }
}
