using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using Storage.Extensions;
using Storage.Levels.Params;
using Storage.Records;
using Storage.Snapshots;
using UnityEngine;

namespace Services
{
    [UsedImplicitly]
    public class ProfileSerializerService
    {
        private const string SaveFileName = "/ProfileRecord.raw";

        public void SaveProfileRecord(ProfileRecord profileRecord)
        {
            if (profileRecord == null)
            {
                return;
            }

            var filePath = Application.persistentDataPath + SaveFileName;
            
            var formatter = new BinaryFormatter();
            var stream = new FileStream(filePath, FileMode.Create);
            
            var jsonPackProgressData = JsonUtility.ToJson(profileRecord);
            //var jsonPackProgressData = "";
            // var index = 0;
            // packsParams.ForEach(packParams =>
            // {
            //     jsonPackProgressData += JsonUtility.ToJson(packParams);
            //     
            //     if (index < packsParams.Count - 1)
            //     {
            //         jsonPackProgressData += "\n";
            //     }
            //
            //     index++;
            // });
            
            formatter.Serialize(stream, jsonPackProgressData);
            stream.Close();
        }

        public ProfileRecord LoadProfileRecord()
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
            return JsonUtility.FromJson<ProfileRecord>(rawTotalProgressData);
            //return levelParamsList.Where(level => level != null).ToList();
        }
    }
}