using System;
using System.IO;
using JetBrains.Annotations;
using Storage.Records;
using UnityEngine;

namespace Services
{
    [UsedImplicitly]
    public class ProfileSerializerService
    {
        private const string SaveFileName = "/ProfileRecord.json";

        private string FilePath => Application.persistentDataPath + SaveFileName;

        public void SaveProfileRecord(ProfileRecord profileRecord)
        {
            if (profileRecord == null)
            {
                return;
            }

            try
            {
                var json = JsonUtility.ToJson(profileRecord, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                LoggerService.LogError($"[ProfileSerializer] Saving error: {e.Message}");
            }
        }

        public ProfileRecord LoadProfileRecord()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(FilePath);
            
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return JsonUtility.FromJson<ProfileRecord>(json);
            }
            catch (Exception e)
            {
                LoggerService.LogError($"[ProfileSerializer] Loading error: {e.Message}");
                return null;
            }
        }
    }
}