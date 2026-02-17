using System.Text.Json;
namespace drawedOut
{
    internal static class SaveData
    {
        private static readonly string 
            _saveFolder = Path.Combine(Global.GetProjFolder(), @"saveData\"),
            _timeFile = _saveFolder + "times.json",
            _keybindFile = _saveFolder + "keybinds.json",
            _settingsFile = _saveFolder + "settings.json",
            _playerDataFile = _saveFolder + "playerData.json";

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
            if (!File.Exists(_timeFile)) return;

            _levelTimes = RetriveJSONData<Dictionary<string, MaxHeap<float>>>(_timeFile); 
        }

        private static T RetriveJSONData<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath)) return null;

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

        public static float? GetFastestScore(byte levelNo) 
        { 
            string level = $"level{levelNo}";
            if (_levelTimes[level].Length == 0) return null;
            else return _levelTimes[level].FullArray[0];
        }

        public static void AddScore(byte levelNo, float timeS) 
        {
            _levelTimes[$"level{levelNo}"].Add(timeS);
            SaveObjectAsJson<Dictionary<string, MaxHeap<float>>>(_levelTimes, _timeFile);
        }

        // keybinds
        public static Dictionary<Keys, Keybinds.Actions>? GetKeybinds()
        { return RetriveJSONData<Dictionary<Keys, Keybinds.Actions>>(_keybindFile); }

        public static void SaveKeybinds(Dictionary<Keys, Keybinds.Actions> _keybinds)
        { SaveObjectAsJson<Dictionary<Keys, Keybinds.Actions>>( _keybinds, _keybindFile); }


        // settings
        public static Preferences.PreferencesInstance? GetSettings()
        { return RetriveJSONData<Preferences.PreferencesInstance>(_settingsFile); }

        public static void SaveSettings()
        { SaveObjectAsJson<Preferences.PreferencesInstance>(Preferences.Instance, _settingsFile); }


        // player data
        public static PlayerCharData.PlayerDataInstance? GetPlayerData()
        { return RetriveJSONData<PlayerCharData.PlayerDataInstance>(_playerDataFile); }

        public static void SavePlayerData()
        { SaveObjectAsJson<PlayerCharData.PlayerDataInstance>(PlayerCharData.Instance, _playerDataFile); }
    }
}
