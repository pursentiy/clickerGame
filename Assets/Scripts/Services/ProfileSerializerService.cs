using System;
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
            
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var jsonPackProgressData = JsonUtility.ToJson(profileRecord);
                formatter.Serialize(stream, jsonPackProgressData);
            }
        }

        public ProfileRecord LoadProfileRecord()
        {
            var filePath = Application.persistentDataPath + SaveFileName;
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            
            try 
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var rawTotalProgressData = formatter.Deserialize(stream) as string;
                    if (string.IsNullOrEmpty(rawTotalProgressData))
                    {
                        return null;
                    }
                    return JsonUtility.FromJson<ProfileRecord>(rawTotalProgressData);
                }
            }
            catch (Exception e)
            {
                LoggerService.LogError($"Failed to load profile: {e.Message}");
                return null;
            }
        }
    }
}