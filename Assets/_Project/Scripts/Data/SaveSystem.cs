using System;
using System.IO;
using System.Text;
using TimeMachine.Core;
using UnityEngine;

namespace TimeMachine.Data
{
    public class SaveSystem
    {
        private readonly string _primaryPath;
        private readonly string _backupPath;

        public SaveSystem(string customDirectory = null, string fileName = "save_v1.json")
        {
            string dir = string.IsNullOrWhiteSpace(customDirectory)
                ? Application.persistentDataPath
                : customDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _primaryPath = Path.Combine(dir, fileName);
            _backupPath = _primaryPath + ".bak";
        }

        public SaveDataV1 LoadOrCreateDefault()
        {
            if (TryLoadFrom(_primaryPath, out SaveDataV1 primary)) return primary;
            if (TryLoadFrom(_backupPath, out SaveDataV1 backup)) return backup;
            return GameState.CreateNewDefaultState();
        }

        public void Save(SaveDataV1 data)
        {
            if (data == null) return;
            data.schemaVersion = GameConstants.GameplayTuning.CurrentSaveSchemaVersion;
            data.savedAtIsoUtc = DateTime.UtcNow.ToString("o");

            string json = JsonUtility.ToJson(data, true);
            string tmp = _primaryPath + ".tmp";
            try
            {
                File.WriteAllText(tmp, json, new UTF8Encoding(false));
                if (File.Exists(_primaryPath))
                {
                    File.Copy(_primaryPath, _backupPath, true);
                    File.Delete(_primaryPath);
                }
                File.Move(tmp, _primaryPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Ошибка сохранения: {ex.Message}");
                if (File.Exists(tmp))
                {
                    try { File.Delete(tmp); } catch { }
                }
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(_primaryPath)) File.Delete(_primaryPath);
                if (File.Exists(_backupPath)) File.Delete(_backupPath);
            }
            catch { }
        }

        private static bool TryLoadFrom(string path, out SaveDataV1 data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return false;
                data = JsonUtility.FromJson<SaveDataV1>(json);
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }
    }
}
