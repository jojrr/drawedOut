namespace drawedOut
{
    internal static class Preferences
    {
        public static bool ShowBG = true;
        public static bool ShowTime = false;
        public static Global.Resolutions Resolution = Global.Resolutions.p1080;
        public static UInt16 FPS = 60;

        public class PreferencesInstance
        {
            public bool ShowBG { get; init; }
            public bool ShowTime { get; init; }
            public Global.Resolutions Resolution { get; init; }
            public UInt16 FPS { get; init; }
            
            public PreferencesInstance()
            {
                ShowBG = Preferences.ShowBG;
                ShowTime = Preferences.ShowTime;
                Resolution = Preferences.Resolution;
                FPS = Preferences.FPS;
            }
        }

        public static PreferencesInstance Instance => new PreferencesInstance();

        public static void LoadInstance(PreferencesInstance? instance)
        {
            if (instance is null) return;
            ShowBG = instance.ShowBG;
            ShowTime = instance.ShowTime;
            Resolution = instance.Resolution;
            FPS = instance.FPS;
            Global.LevelResolution = Resolution;
            Global.GameTickFreq = FPS;
        }
    }
}
