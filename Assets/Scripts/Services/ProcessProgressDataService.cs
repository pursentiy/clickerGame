using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using Storage.Levels.Params;
using UnityEngine;

namespace Services
{
    [UsedImplicitly]
    public class ProcessProgressDataService
    {
        private const string SaveFileName = "/LevelsProgress.raw";

        public void SaveProgress(List<PackParams> packsParams)
        {
            var filePath = Application.persistentDataPath + SaveFileName;
            
            var formatter = new BinaryFormatter();
            var stream = new FileStream(filePath, FileMode.Create);
            
            var jsonPackProgressData = "";
            var index = 0;
            packsParams.ForEach(packParams =>
            {
                jsonPackProgressData += JsonUtility.ToJson(packParams);
                
                if (index < packsParams.Count - 1)
                {
                    jsonPackProgressData += "\n";
                }

                index++;
            });
            
            formatter.Serialize(stream, jsonPackProgressData);
            stream.Close();
        }

        public List<PackParams> LoadProgress()
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
            var levelParamsList = rawProgressDataArray.Select(JsonUtility.FromJson<PackParams>).ToList();
            return levelParamsList.Where(level => level != null).ToList();
        }

        public void CheatResetProgress()
        {
            var filePath = Application.persistentDataPath + SaveFileName;
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Application.Quit();
            }
        }
    }
}