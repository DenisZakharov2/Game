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

        /// <summary>
        /// Экземплярный метод для удобной инициализации в GameState.
        /// </summary>
        public SaveDataV1 LoadOrCreateDefault()
        {
            return Load();
        }

        /// <summary>
        /// Атомарное сохранение через временный файл .tmp и бэкап .bak.
        /// </summary>
        public static bool Save(SaveDataV1 data)
        {
            if (data == null)
            {
                Debug.LogError("[TimeMachine] [SaveSystem] Попытка сохранить null-данные!");
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
                Debug.Log($"[TimeMachine] [SaveSystem] Успешное сохранение: {SavePath} | Размер: {fileSizeBytes} B | Схема: v{data.schemaVersion}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка при записи файла сохранения: {ex.Message}");
                if (File.Exists(TempPath))
                {
                    try { File.Delete(TempPath); } catch { }
                }
                return false;
            }
        }

        /// <summary>
        /// Безопасная загрузка сохранения с поддержкой автоматического восстановления и миграции.
        /// </summary>
        public static SaveDataV1 Load()
        {
            if (!File.Exists(SavePath))
            {
                if (File.Exists(BackupPath))
                {
                    Debug.LogWarning("[TimeMachine] [SaveSystem] Основное сохранение не найдено, запуск восстановления из .bak...");
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
                    Debug.Log("[TimeMachine] [SaveSystem] Сохранение не найдено. Создание нового профиля...");
                    return CreateNewSave();
                }
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);

                if (data == null)
                {
                    Debug.LogError("[TimeMachine] [SaveSystem] Файл сохранения поврежден.");
                    return RestoreFromBackup();
                }

                if (data.schemaVersion < CURRENT_SCHEMA_VERSION)
                {
                    data = MigrateSaveData(data, data.schemaVersion, CURRENT_SCHEMA_VERSION);
                }

                Debug.Log($"[TimeMachine] [SaveSystem] Профиль успешно загружен. Квест: {data.currentQuestId}, Эпоха: {data.currentEra}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка чтения сохранения: {ex.Message}");
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
                    Debug.LogError($"[TimeMachine] [SaveSystem] Чтение бэкапа завершилось ошибкой: {ex.Message}");
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
            Debug.Log($"[TimeMachine] [SaveSystem] Миграция сохранения с v{oldVersion} до v{newVersion}...");
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
                Debug.Log("[TimeMachine] [SaveSystem] Данные пользователя успешно удалены.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка при удалении сохранения: {ex.Message}");
                return false;
            }
        }
    }
}
