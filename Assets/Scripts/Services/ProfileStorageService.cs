using System;
using System.IO;
using Extensions;
using Playgama;
using Storage.Records;
using UnityEngine;

namespace Services
{
    public class ProfileStorageService
    {
        private const string SaveKey = "user_profile_data";
        
        private string filePath => Path.Combine(Application.persistentDataPath, "profile.json");
        
        public void SaveProfileRecord(ProfileRecord profileRecord)
        {
            if (profileRecord == null) 
                return;

            if (AppConfigService.IsProduction())
            {
                SaveProfileRecordProduction(profileRecord);
            }
            else
            {
                SaveProfileRecordDebug(profileRecord);
            }
        }
        
        public void LoadProfileRecord(Action<ProfileRecord> onLoaded)
        {
            if (AppConfigService.IsProduction())
            {
                LoadProfileRecordProduction(onLoaded);
            }
            else
            {
                LoadProfileRecordDebug(onLoaded);
            }
        }

        private void SaveProfileRecordProduction(ProfileRecord profileRecord)
        {
            var json = JsonUtility.ToJson(profileRecord);
            Bridge.storage.Set(SaveKey, json, (success) => 
            {
                if (!success)
                {
                    LoggerService.LogError("[Storage] Cloud Save Failed");
                }
            });
        }

        private void SaveProfileRecordDebug(ProfileRecord profileRecord)
        {
            try 
            {
                var json = JsonUtility.ToJson(profileRecord, true);
                File.WriteAllText(filePath, json);
                LoggerService.LogDebug($"[Storage] Saved to File: {filePath}");
            } 
            catch (Exception e) 
            {
                LoggerService.LogError($"[Storage] File Save Error: {e.Message}");
            }
        }
        
        private void LoadProfileRecordProduction(Action<ProfileRecord> onLoaded)
        {
            Bridge.storage.Get(SaveKey, (success, data) => 
            {
                if (success && data != null && !string.IsNullOrEmpty(data.ToString())) 
                {
                    var record = JsonUtility.FromJson<ProfileRecord>(data.ToString());
                    onLoaded.SafeInvoke(record);
                } 
                else 
                {
                    onLoaded.SafeInvoke(null);
                }
            });
        }

        private void LoadProfileRecordDebug(Action<ProfileRecord> onLoaded)
        {
            if (File.Exists(filePath)) 
            {
                var json = File.ReadAllText(filePath);
                var record = JsonUtility.FromJson<ProfileRecord>(json);
                onLoaded.SafeInvoke(record);
            } 
            else 
            {
                onLoaded.SafeInvoke(null);
            }
        }
    }
}