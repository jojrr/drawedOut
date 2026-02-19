namespace drawedOut
{
    internal class Preferences
    {
        public byte FPS { get; init; }
        public bool ShowBG { get; init; }
        public bool ShowTime { get; init; }
        public Global.Resolutions Resolution { get; init; }
        
        public Preferences()
        {
            FPS = Global.GameTickFreq;
            ShowBG = Global.ShowBG;
            ShowTime = Global.ShowTime;
            Resolution = Global.LevelResolution;
        }

        public static Preferences Instance => new Preferences();

        public static void LoadInstance(Preferences? instance)
        {
            if (instance is null) return;
            Global.ShowBG = instance.ShowBG;
            Global.ShowTime = instance.ShowTime;
            Global.GameTickFreq = instance.FPS;
            Global.LevelResolution = instance.Resolution;
        } 
    }
}
