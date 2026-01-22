using System.Text.Json;
using System.IO;
namespace drawedOut
{
    internal static class SaveData
    {
        private static readonly string _saveFolder = Path.Combine(Global.GetProjFolder(), @"playerData\");
        private static readonly string _timeFile = _saveFolder + "times.txt";
        private static Dictionary<string, MinHeap<float>> _levelTimes = new Dictionary<string, MinHeap<float>>
        {
            ["level0"] = new MinHeap<float>(0),
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
            if (File.Exists(_timeFile)) RetriveTimeData();
        }
        
        private static void RetriveTimeData()
        {
            string jsonTxt = "";
            using ( StreamReader sr = new StreamReader(_timeFile) )
            { jsonTxt= sr.ReadToEnd(); }
            var tempLevelTimes = 
                JsonSerializer.Deserialize<Dictionary<string, MinHeap<float>>>
                (jsonTxt,_jsonOptions);
            if (tempLevelTimes is not null) _levelTimes = tempLevelTimes;
        }

        public static float GetFastestScore(UInt16 levelNo) => _levelTimes[$"level{levelNo}"].Min;
        public static void AddScore(UInt16 levelNo, float timeS) 
        {
            _levelTimes[$"level{levelNo}"].Add(timeS);
            string timesAsJson = JsonSerializer.Serialize(_levelTimes, _jsonOptions);
            using ( StreamWriter sw = new StreamWriter(_timeFile, false) )
            { sw.Write(timesAsJson); }
        }
    }
}
