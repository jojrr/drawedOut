namespace drawedOut
{
    internal static class Preferences
    {
        public static byte FPS = 60;
        public static bool ShowBG = true;
        public static bool ShowTime = false;
        public static Global.Resolutions Resolution = Global.Resolutions.p1080;

        public class PreferencesInstance
        {
            public byte FPS { get; init; }
            public bool ShowBG { get; init; }
            public bool ShowTime { get; init; }
            public Global.Resolutions Resolution { get; init; }
            
            public PreferencesInstance()
            {
                FPS = Preferences.FPS;
                ShowBG = Preferences.ShowBG;
                ShowTime = Preferences.ShowTime;
                Resolution = Preferences.Resolution;
            }
        }

        public static PreferencesInstance Instance => new PreferencesInstance();

        public static void LoadInstance(PreferencesInstance? instance)
        {
            if (instance is null) return;
            FPS = instance.FPS;
            ShowBG = instance.ShowBG;
            ShowTime = instance.ShowTime;
            Resolution = instance.Resolution;
            Global.LevelResolution = Resolution;
            Global.GameTickFreq = FPS;
        }
    }
}
