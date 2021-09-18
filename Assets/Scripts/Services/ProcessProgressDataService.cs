using System.Collections.Generic;
using System.IO;
using System.Linq;
using Storage.Levels.Params;
using UnityEngine;

namespace Services
{
    public class ProcessProgressDataService : IProcessProgressDataService
    {
        private const string SaveFilePath = "/Saves/LevelsProgress.txt";

        public void SaveProgress(List<LevelParams> levelsParams)
        {
            var jsonLevelsProgressData = "";
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                jsonLevelsProgressData += JsonUtility.ToJson(levelParams);
                
                if (index < levelsParams.Count - 1)
                {
                    jsonLevelsProgressData += "\n";
                }
            });
            File.WriteAllText (Application.dataPath + SaveFilePath, jsonLevelsProgressData);
        }

        public List<LevelParams> LoadProgress()
        {
            if (!File.Exists(Application.dataPath + SaveFilePath))
            {
                return null;
            }

            var rawTotalProgressData = File.ReadAllText(Application.dataPath + SaveFilePath);

            if (rawTotalProgressData == "")
            {
                return null;
            }
            
            var rawProgressDataArray = rawTotalProgressData.Split('\n');
            var levelParamsList = rawProgressDataArray.Select(JsonUtility.FromJson<LevelParams>).ToList();
            return levelParamsList.Where(level => level != null).ToList();
        }
    }
}