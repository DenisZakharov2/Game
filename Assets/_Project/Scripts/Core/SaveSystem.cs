using System;
using System.IO;
using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Core
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
        /// Экземплярный метод для вызова из GameBootstrap / GameState.
        /// </summary>
        public SaveDataV1 LoadOrCreateDefault()
        {
            return Load();
        }

        /// <summary>
        /// Атомарное сохранение данных игрока.
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
                data.savedAtIsoUtc = DateTime.UtcNow.ToString("o");

                string json = JsonUtility.ToJson(data, true);

                // 1. Запись во временный файл
                File.WriteAllText(TempPath, json);

                // 2. Создание резервной копии
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
                Debug.Log($"[TimeMachine] [SaveSystem] Успешно сохранено! Путь: {SavePath} | Размер: {fileSizeBytes} байт | Квест: {data.currentQuestId}");
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
        /// Чтение данных с автоматическим восстановлением из бэкапа.
        /// </summary>
        public static SaveDataV1 Load()
        {
            if (!File.Exists(SavePath))
            {
                if (File.Exists(BackupPath))
                {
                    Debug.LogWarning("[TimeMachine] [SaveSystem] Основной файл отсутствует, восстановление из .bak...");
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
                    Debug.Log("[TimeMachine] [SaveSystem] Сохранение не найдено. Создание нового профиля.");
                    return CreateNewSave();
                }
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);

                if (data == null)
                {
                    Debug.LogError("[TimeMachine] [SaveSystem] Файл сохранения повреждён.");
                    return RestoreFromBackup();
                }

                Debug.Log($"[TimeMachine] [SaveSystem] Сохранение загружено. Эпоха: {data.currentEra}, Квест: {data.currentQuestId}, XP: {data.totalXp}");
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
                    Debug.LogError($"[TimeMachine] [SaveSystem] Ошибка бэкапа: {ex.Message}");
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
                Debug.Log("[TimeMachine] [SaveSystem] Сохранения успешно удалены.");
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
