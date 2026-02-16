using System.Text.Json;
namespace drawedOut
{
    internal static class SaveData
    {
        private static readonly string 
            _saveFolder = Path.Combine(Global.GetProjFolder(), @"playerData\"),
            _timeFile = _saveFolder + "times.json",
            _keybindFile = _saveFolder + "keybinds.json",
            _settingsFile = _saveFolder + "settings.json";

        private static Dictionary<string, MaxHeap<float>> _levelTimes = 
            new Dictionary<string, MaxHeap<float>> { 
                ["level0"] = new MaxHeap<float>(), 
                ["level1"] = new MaxHeap<float>(), 
                ["level2"] = new MaxHeap<float>(), 
            };

        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true
        };

        static SaveData()
        {
            Directory.CreateDirectory(_saveFolder);

            if (File.Exists(_timeFile)) 
            { _levelTimes = RetriveJSONData<Dictionary<string, MaxHeap<float>>>(_timeFile); }
        }

        private static T RetriveJSONData<T>(string filePath)
        {
            string jsonTxt = "";

            using ( StreamReader sr = new StreamReader(filePath) )
            { jsonTxt= sr.ReadToEnd(); }

            T outVar = 
                JsonSerializer.Deserialize<T>
                (jsonTxt,_jsonOptions);

            if (outVar is not null) return outVar;
            else throw new Exception("file is null");
        }

        private static void SaveObjectAsJson<T>(T obj, string filePath, bool append=false)
        {
            string objAsJson = JsonSerializer.Serialize(obj, _jsonOptions);
            using ( StreamWriter sw = new StreamWriter(filePath, append) )
            { sw.Write(objAsJson); }
        }

        public static float? GetFastestScore(UInt16 levelNo) 
        { 
            string level = $"level{levelNo}";
            if (_levelTimes[level].Length == 0) return null;
            else return _levelTimes[level].FullArray[0];
        }

        public static void AddScore(UInt16 levelNo, float timeS) 
        {
            _levelTimes[$"level{levelNo}"].Add(timeS);
            SaveObjectAsJson<Dictionary<string, MaxHeap<float>>>(_levelTimes, _timeFile);
        }

        public static Dictionary<Keys, Keybinds.Actions>? GetKeybinds()
        {
            if (File.Exists(_keybindFile)) 
            { return RetriveJSONData<Dictionary<Keys, Keybinds.Actions>>(_keybindFile); }
            else return null;
        }

        public static void SaveKeybinds(Dictionary<Keys, Keybinds.Actions> keyActDict)
        { SaveObjectAsJson<Dictionary<Keys, Keybinds.Actions>>(keyActDict, _keybindFile); }

        public static Preferences.PreferencesInstance? GetSettings()
        {
            if (File.Exists(_settingsFile)) 
            { return RetriveJSONData<Preferences.PreferencesInstance>(_settingsFile); }
            else return null;
        }

        public static void SaveSettings()
        { 
            Preferences.Resolution = Global.LevelResolution;
            Preferences.FPS = Global.GameTickFreq;
            SaveObjectAsJson<Preferences.PreferencesInstance>(Preferences.Instance, _settingsFile);
        }

    }
}
