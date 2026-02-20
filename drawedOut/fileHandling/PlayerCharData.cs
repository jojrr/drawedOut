namespace drawedOut
{
    public class PlayerDataInstance
    {
        public byte MaxHp { get; init; }
        public bool[] UnlockedMoves { get; init; }

        public PlayerDataInstance()
        {
            MaxHp = Player.MaxHp;
            UnlockedMoves = Player.UnlockedMoves;
        }

        public static PlayerDataInstance Instance => new PlayerDataInstance();

        public static void LoadInstance(PlayerDataInstance? instance)
        {
            if (instance is null) return;
            Player.MaxHp = instance.MaxHp;
            Player.UnlockedMoves = instance.UnlockedMoves;
        }
    }

}
