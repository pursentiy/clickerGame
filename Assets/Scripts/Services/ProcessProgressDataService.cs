using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using ModestTree;
using Storage.Levels.Params;
using UnityEngine;

namespace Services
{
    [UsedImplicitly]
    public class ProcessProgressDataService : IProcessProgressDataService
    {
        private const string SaveFileName = "/LevelsProgress.raw";

        public void SaveProgress(List<LevelParams> levelsParams)
        {
            var filePath = Application.persistentDataPath + SaveFileName;
            
            var formatter = new BinaryFormatter();
            var stream = new FileStream(filePath, FileMode.Create);
            
            var jsonLevelsProgressData = "";
            var index = 0;
            levelsParams.ForEach(levelParams =>
            {
                jsonLevelsProgressData += JsonUtility.ToJson(levelParams);
                
                if (index < levelsParams.Count - 1)
                {
                    jsonLevelsProgressData += "\n";
                }

                index++;
            });
            
            formatter.Serialize(stream, jsonLevelsProgressData);
            stream.Close();
        }

        public List<LevelParams> LoadProgress()
        {
            var filePath = Application.persistentDataPath + SaveFileName;
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            var stream = new FileStream(filePath, FileMode.Open);
            var rawTotalProgressData = formatter.Deserialize(stream) as string;
            if (string.IsNullOrEmpty(rawTotalProgressData))
            {
                return null;
            }
            
            var rawProgressDataArray = rawTotalProgressData.Split('\n');
            var levelParamsList = rawProgressDataArray.Select(JsonUtility.FromJson<LevelParams>).ToList();
            return levelParamsList.Where(level => level != null).ToList();
        }
    }
}