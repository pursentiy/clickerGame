using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using Storage.Records;
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
            
            return JsonUtility.FromJson<ProfileRecord>(rawTotalProgressData);
        }
    }
}