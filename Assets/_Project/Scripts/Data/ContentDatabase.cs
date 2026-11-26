using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Data
{
    public class ContentDatabase : MonoBehaviour
    {
        public static ContentDatabase Instance { get; private set; }

        private readonly Dictionary<string, TaskData> tasks = new Dictionary<string, TaskData>();
        private readonly Dictionary<string, TemplateData> templates = new Dictionary<string, TemplateData>();
        private readonly Dictionary<string, LoreCardData> loreCards = new Dictionary<string, LoreCardData>();
        private readonly Dictionary<string, QuestData> quests = new Dictionary<string, QuestData>();
        private readonly Dictionary<string, string> localizationDictionary = new Dictionary<string, string>();

        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadAllData()
        {
            LoadLocalization();
            LoadJsonData();
            IsLoaded = true;
        }

        public void LoadLocalization()
        {
            localizationDictionary.Clear();
            TextAsset csvFile = Resources.Load<TextAsset>("Localization/strings_ru");

            if (csvFile == null)
            {
                Debug.LogWarning("[ContentDatabase] Файл Resources/Localization/strings_ru не найден.");
                return;
            }

            string[] lines = csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(';');
                if (parts.Length >= 4)
                {
                    string key = parts[0].Trim();
                    string text = parts[3].Trim().Replace("\\n", "\n");
                    if (!localizationDictionary.ContainsKey(key))
                    {
                        localizationDictionary.Add(key, text);
                    }
                }
            }
            Debug.Log($"[ContentDatabase] Загружено {localizationDictionary.Count} строк локализации.");
        }

        private void LoadJsonData()
        {
            LoadJsonCollection<TaskDataWrapper>("GameData/tasks", wrapper => {
                if (wrapper?.tasks == null) return;
                foreach (var item in wrapper.tasks) if (!string.IsNullOrEmpty(item.id)) tasks[item.id] = item;
            });

            LoadJsonCollection<TemplateDataWrapper>("GameData/templates", wrapper => {
                if (wrapper?.templates == null) return;
                foreach (var item in wrapper.templates) if (!string.IsNullOrEmpty(item.id)) templates[item.id] = item;
            });

            LoadJsonCollection<LoreDataWrapper>("GameData/lore", wrapper => {
                if (wrapper?.loreCards == null) return;
                foreach (var item in wrapper.loreCards) if (!string.IsNullOrEmpty(item.id)) loreCards[item.id] = item;
            });

            LoadJsonCollection<QuestDataWrapper>("GameData/quests", wrapper => {
                if (wrapper?.quests == null) return;
                foreach (var item in wrapper.quests) if (!string.IsNullOrEmpty(item.id)) quests[item.id] = item;
            });
        }

        private void LoadJsonCollection<T>(string resourcePath, Action<T> onLoaded)
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
            {
                try
                {
                    T data = JsonUtility.FromJson<T>(jsonAsset.text);
                    onLoaded?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ContentDatabase] Ошибка парсинга {resourcePath}: {ex.Message}");
                }
            }
        }

        public string GetLocalizedString(string key, string fallback = "")
        {
            if (localizationDictionary.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        public bool TryGetTask(string id, out TaskData task) => tasks.TryGetValue(id, out task);
        public bool TryGetTemplate(string id, out TemplateData template) => templates.TryGetValue(id, out template);
        public bool TryGetLoreCard(string id, out LoreCardData card) => loreCards.TryGetValue(id, out card);
        public bool TryGetQuest(string id, out QuestData quest) => quests.TryGetValue(id, out quest);
    }

    [Serializable] internal class TaskDataWrapper { public List<TaskData> tasks; }
    [Serializable] internal class TemplateDataWrapper { public List<TemplateData> templates; }
    [Serializable] internal class LoreDataWrapper { public List<LoreCardData> loreCards; }
    [Serializable] internal class QuestDataWrapper { public List<QuestData> quests; }
}
