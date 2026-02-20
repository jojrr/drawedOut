namespace drawedOut
{
    internal class Preferences
    {
        public byte FPS { get; init; }
        public bool ShowHitboxes { get; init; }
        public bool ShowTime { get; init; }
        public Global.Resolutions Resolution { get; init; }
        
        public Preferences()
        {
            FPS = Global.GameTickFreq;
            ShowHitboxes = Global.ShowHitboxes;
            ShowTime = Global.ShowTime;
            Resolution = Global.LevelResolution;
        }

        public static Preferences Instance => new Preferences();

        public static void LoadInstance(Preferences? instance)
        {
            if (instance is null) return;
            Global.ShowHitboxes = instance.ShowHitboxes;
            Global.ShowTime = instance.ShowTime;
            Global.GameTickFreq = instance.FPS;
            Global.LevelResolution = instance.Resolution;
        } 
    }
}
