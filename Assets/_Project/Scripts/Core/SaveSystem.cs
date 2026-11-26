using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Core
{
    [Serializable]
    public class QuestProgressData
    {
        public string questId;
        public string status; // "NotStarted", "InProgress", "Completed"
        public int currentStepIndex;
    }

    [Serializable]
    public class SaveDataV1
    {
        public int schemaVersion = 1;
        public string currentQuestId = "q_2027_01";
        public int currentEra = 2027;
        public int playerXP = 0;
        public int totalXp = 0;
        public bool isZarya7Assembled = false;

        public List<string> completedQuestIds = new List<string>();
        public List<string> inventoryItemIds = new List<string>();
        public List<string> unlockedLoreCardIds = new List<string>();
        public List<QuestProgressData> questProgressList = new List<QuestProgressData>();

        public string lastSaveTime = "";
    }

    public static class SaveSystem
    {
        public const int CURRENT_SCHEMA_VERSION = 1;
        private static readonly string SaveFileName = "savegame.json";
        private static readonly string TempFileName = "savegame.tmp";
        private static readonly string BackupFileName = "savegame.bak";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private static string TempPath => Path.Combine(Application.persistentDataPath, TempFileName);
        private static string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        /// <summary>
        /// Атомарная запись данных сохранения через .tmp файл с созданием резервной копии.
        /// </summary>
        public static bool Save(SaveDataV1 data)
        {
            if (data == null)
            {
                Debug.LogError("[TimeMachine] [SaveSystem] Попытка сохранить пустые данные (null)!");
                return false;
            }

            try
            {
                data.schemaVersion = CURRENT_SCHEMA_VERSION;
                data.lastSaveTime = DateTime.UtcNow.ToString("o");

                string json = JsonUtility.ToJson(data, true);

                // 1. Запись во временный файл
                File.WriteAllText(TempPath, json);

                // 2. Создание бэкапа существующего файла сохранения
                if (File.Exists(SavePath))
                {
                    if (File.Exists(BackupPath))
                    {
                        File.Delete(BackupPath);
                    }
                    File.Copy(SavePath, BackupPath);
                }

                // 3. Замещение основного файла из временного
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                File.Move(TempPath, SavePath);

                long fileSizeBytes = new FileInfo(SavePath).Length;
                Debug.Log($"[TimeMachine] [SaveSystem] Успешное сохранение! Путь: {SavePath} | Размер: {fileSizeBytes} байт | Схема v{data.schemaVersion}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка при записи сохранения: {ex.Message}");
                if (File.Exists(TempPath))
                {
                    try { File.Delete(TempPath); } catch { }
                }
                return false;
            }
        }

        /// <summary>
        /// Загрузка данных с автоматической проверкой целостности, восстановлением из бэкапа и миграцией.
        /// </summary>
        public static SaveDataV1 Load()
        {
            if (!File.Exists(SavePath))
            {
                if (File.Exists(BackupPath))
                {
                    Debug.LogWarning("[TimeMachine] [SaveSystem] Основной файл сохранения отсутствует, восстановление из бэкапа...");
                    try
                    {
                        File.Copy(BackupPath, SavePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TimeMachine] [SaveSystem] Не удалось восстановить бэкап: {ex.Message}");
                        return CreateNewSave();
                    }
                }
                else
                {
                    Debug.Log("[TimeMachine] [SaveSystem] Файл сохранения не обнаружен. Создаётся новое сохранение.");
                    return CreateNewSave();
                }
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);

                if (data == null)
                {
                    Debug.LogError("[TimeMachine] [SaveSystem] Данные сохранения повреждены или пусты.");
                    return RestoreFromBackup();
                }

                if (data.schemaVersion < CURRENT_SCHEMA_VERSION)
                {
                    data = MigrateSaveData(data, data.schemaVersion, CURRENT_SCHEMA_VERSION);
                }

                Debug.Log($"[TimeMachine] [SaveSystem] Сохранение загружено. Квест: {data.currentQuestId}, Эпоха: {data.currentEra}, XP: {data.totalXp}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Исключение при чтении файла сохранения: {ex.Message}");
                return RestoreFromBackup();
            }
        }

        private static SaveDataV1 RestoreFromBackup()
        {
            if (File.Exists(BackupPath))
            {
                Debug.LogWarning("[TimeMachine] [SaveSystem] Попытка загрузки данных из резервной копии .bak...");
                try
                {
                    string json = File.ReadAllText(BackupPath);
                    SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);
                    if (data != null) return data;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TimeMachine] [SaveSystem] Не удалось прочитать бэкап: {ex.Message}");
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

        private static SaveDataV1 MigrateSaveData(SaveDataV1 data, int oldVersion, int newVersion)
        {
            Debug.Log($"[TimeMachine] [SaveSystem] Миграция данных сохранения с версии {oldVersion} до версии {newVersion}...");
            data.schemaVersion = newVersion;
            Save(data);
            return data;
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
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка при удалении файлов сохранения: {ex.Message}");
                return false;
            }
        }
    }
}
