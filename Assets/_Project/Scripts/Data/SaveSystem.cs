using System;
using System.IO;
using UnityEngine;

namespace TimeMachine.Data
{
    public class SaveSystem
    {
        public const int CURRENT_SCHEMA_VERSION = 1;
        private static readonly string SaveFileName = "savegame.json";
        private static readonly string TempFileName = "savegame.tmp";
        private static readonly string BackupFileName = "savegame.bak";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private static string TempPath => Path.Combine(Application.persistentDataPath, TempFileName);
        private static string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        public SaveDataV1 LoadOrCreateDefault()
        {
            return Load();
        }

        public bool SaveData(SaveDataV1 data)
        {
            return Save(data);
        }

        public static bool Save(SaveDataV1 data)
        {
            if (data == null)
            {
                Debug.LogError("[TimeMachine] [SaveSystem] Попытка сохранить null данные!");
                return false;
            }

            try
            {
                data.schemaVersion = CURRENT_SCHEMA_VERSION;
                data.savedAtIsoUtc = DateTime.UtcNow.ToString("o");

                string json = JsonUtility.ToJson(data, true);

                File.WriteAllText(TempPath, json);

                if (File.Exists(SavePath))
                {
                    if (File.Exists(BackupPath))
                    {
                        File.Delete(BackupPath);
                    }
                    File.Copy(SavePath, BackupPath);
                }

                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                File.Move(TempPath, SavePath);

                long fileSizeBytes = new FileInfo(SavePath).Length;
                Debug.Log($"[TimeMachine] [SaveSystem] Файл успешно сохранен: {SavePath} ({fileSizeBytes} байт)");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка сохранения: {ex.Message}");
                if (File.Exists(TempPath))
                {
                    try { File.Delete(TempPath); } catch { }
                }
                return false;
            }
        }

        public static SaveDataV1 Load()
        {
            if (!File.Exists(SavePath))
            {
                if (File.Exists(BackupPath))
                {
                    Debug.LogWarning("[TimeMachine] [SaveSystem] Восстановление из резервного бэкапа...");
                    try
                    {
                        File.Copy(BackupPath, SavePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка восстановления: {ex.Message}");
                        return CreateNewSave();
                    }
                }
                else
                {
                    Debug.Log("[TimeMachine] [SaveSystem] Создание нового игрового профиля.");
                    return CreateNewSave();
                }
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);

                if (data == null)
                {
                    Debug.LogError("[TimeMachine] [SaveSystem] Поврежденное сохранение.");
                    return RestoreFromBackup();
                }

                Debug.Log($"[TimeMachine] [SaveSystem] Успешно загружено. Квест: {data.currentQuestId}, XP: {data.totalXp}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка чтения: {ex.Message}");
                return RestoreFromBackup();
            }
        }

        private static SaveDataV1 RestoreFromBackup()
        {
            if (File.Exists(BackupPath))
            {
                try
                {
                    string json = File.ReadAllText(BackupPath);
                    SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);
                    if (data != null) return data;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка чтения бэкапа: {ex.Message}");
                }
            }
            return CreateNewSave();
        }

        private static SaveDataV1 CreateNewSave()
        {
            SaveDataV1 newData = new SaveDataV1();
            Save(newData);
            return newData;
        }

        public static bool DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                if (File.Exists(TempPath)) File.Delete(TempPath);
                Debug.Log("[TimeMachine] [SaveSystem] Файлы сохранений удалены.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка при удалении: {ex.Message}");
                return false;
            }
        }
    }
}
