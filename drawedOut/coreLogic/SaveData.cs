using System.Text.Json;
namespace drawedOut
{
    internal static class SaveData
    {
        private static readonly string _saveFolder = Path.Combine(Global.GetProjFolder(), @"playerData\");
        private static readonly string 
            _timeFile = _saveFolder + "times.json",
            _keybindFile = _saveFolder + "keybinds.json",
            _settingsFile = _saveFolder + "settings.json";

        private static Dictionary<string, MaxHeap<float>> _levelTimes = 
            new Dictionary<string, MaxHeap<float>> { ["level0"] = new MaxHeap<float>(), };

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

        public static void SaveDictAsJson<T>(T dict, string filePath)
        {
            string dictAsJson = JsonSerializer.Serialize(dict, _jsonOptions);
            using ( StreamWriter sw = new StreamWriter(filePath, false) )
            { sw.Write(dictAsJson); }
        }

        public static float GetFastestScore(UInt16 levelNo) 
        { return _levelTimes[$"level{levelNo}"].FullArray[0]; }

        public static void AddScore(UInt16 levelNo, float timeS) 
        {
            _levelTimes[$"level{levelNo}"].Add(timeS);
            SaveDictAsJson<Dictionary<string, MaxHeap<float>>>(_levelTimes, _timeFile);
        }

        public static Dictionary<Keys, Keybinds.Actions>? GetKeybinds()
        {
            if (File.Exists(_keybindFile)) 
            { return RetriveJSONData<Dictionary<Keys, Keybinds.Actions>>(_keybindFile); }
            else return null;
        }
        public static void SaveKeybinds(Dictionary<Keys, Keybinds.Actions> keyActDict)
        { SaveData.SaveDictAsJson<Dictionary<Keys, Keybinds.Actions>>(keyActDict, _keybindFile); }

    }
}
