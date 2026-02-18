namespace drawedOut
{
    public class PlayerDataInstance
    {
        public byte MaxHp { get; init; }
        public UInt16 MaxEnergy { get; init; }
        public bool[] UnlockedMoves { get; init; }

        public PlayerDataInstance()
        {
            MaxHp = Player.MaxHp;
            MaxEnergy = Player.MaxEnergy;
            UnlockedMoves = Player.UnlockedMoves;
        }

        public static PlayerDataInstance Instance => new PlayerDataInstance();

        public static void LoadInstance(PlayerDataInstance? instance)
        {
            if (instance is null) return;
            Player.MaxHp = instance.MaxHp;
            Player.MaxEnergy = instance.MaxEnergy;
            Player.UnlockedMoves = instance.UnlockedMoves;
        }
    }

}
