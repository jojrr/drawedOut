using System.Text.Json;
using System.IO;
namespace drawedOut
{
    internal static class SaveData
    {
        private static readonly string _saveFolder = Path.Combine(Global.GetProjFolder(), @"playerData\");
        private static readonly string _timeFile = _saveFolder + "times.txt";

        static SaveData()
        {
            Directory.CreateDirectory(_saveFolder);
            string jsonTxt = "";
            using ( StreamReader sr = new StreamReader(_timeFile) )
            { 
                string line;
                do 
                {
                    line = sr.ReadLine();
                    jsonTxt += line;
                } while (line is not null);
            }
            Dictionary<string, MinHeap<float>>? tempLevelTimes = 
                JsonSerializer.Deserialize<Dictionary<string, MinHeap<float>>>(jsonTxt);
            if (tempLevelTimes is not null) _levelTimes = tempLevelTimes;
        }

        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true
        };

        private static Dictionary<string, MinHeap<float>> _levelTimes = new Dictionary<string, MinHeap<float>>
        {
            ["level0"] = new MinHeap<float>(0),
        };

        public static float GetFastestScore(UInt16 levelNo) => _levelTimes[$"level{levelNo}"].Root;
        public static void AddScore(UInt16 levelNo, float timeS) 
        {
            _levelTimes[$"level{levelNo}"].Add(timeS);
            string timesAsJson = JsonSerializer.Serialize(_levelTimes, _jsonOptions);
            using ( StreamWriter sw = new StreamWriter(_timeFile, false) )
            { sw.Write(timesAsJson); }
        }
    }
}
