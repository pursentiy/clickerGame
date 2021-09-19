using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Storage.Levels.Params;
using UnityEngine;

namespace Services
{
    [UsedImplicitly]
    public class ProcessProgressDataService : IProcessProgressDataService
    {
        private const string SaveFileName = "/LevelsProgress.txt";

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
            File.WriteAllText (Application.streamingAssetsPath + SaveFileName, jsonLevelsProgressData);
        }

        public List<LevelParams> LoadProgress()
        {
            
            if (!File.Exists(Application.streamingAssetsPath + SaveFileName))
            {
                return null;
            }

            var rawTotalProgressData = File.ReadAllText(Application.streamingAssetsPath + SaveFileName);

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