using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TimeMachine.Data
{
    public class ContentDatabase
    {
        private readonly List<TaskData> _allTasks = new List<TaskData>(64);
        private readonly List<TemplateData> _allTemplates = new List<TemplateData>(32);
        private readonly List<LoreCardData> _allLoreCards = new List<LoreCardData>(32);
        private readonly List<QuestData> _allQuests = new List<QuestData>(32);

        private readonly Dictionary<string, TaskData> _tasksById = new Dictionary<string, TaskData>(StringComparer.Ordinal);
        private readonly Dictionary<string, TemplateData> _templatesById = new Dictionary<string, TemplateData>(StringComparer.Ordinal);
        private readonly Dictionary<string, LoreCardData> _loreById = new Dictionary<string, LoreCardData>(StringComparer.Ordinal);
        private readonly Dictionary<string, QuestData> _questsById = new Dictionary<string, QuestData>(StringComparer.Ordinal);

        public IReadOnlyList<TaskData> AllTasks => _allTasks;
        public IReadOnlyList<TemplateData> AllTemplates => _allTemplates;
        public IReadOnlyList<LoreCardData> AllLoreCards => _allLoreCards;
        public IReadOnlyList<QuestData> AllQuests => _allQuests;

        public void LoadFromResources(string baseFolder = "GameData")
        {
            string tasksJson = ReadResourceText($"{baseFolder}/tasks");
            string templatesJson = ReadResourceText($"{baseFolder}/templates");
            string loreJson = ReadResourceText($"{baseFolder}/lore");
            string questsJson = ReadResourceText($"{baseFolder}/quests");
            LoadFromJsonStrings(tasksJson, templatesJson, loreJson, questsJson);
        }

        public void LoadFromJsonStrings(string tasksJson, string templatesJson, string loreJson, string questsJson)
        {
            _allTasks.Clear();
            _allTemplates.Clear();
            _allLoreCards.Clear();
            _allQuests.Clear();
            _tasksById.Clear();
            _templatesById.Clear();
            _loreById.Clear();
            _questsById.Clear();

            if (!string.IsNullOrWhiteSpace(tasksJson))
            {
                var wrapper = JsonUtility.FromJson<TaskCollectionWrapper>(tasksJson);
                if (wrapper?.tasks != null)
                {
                    for (int i = 0; i < wrapper.tasks.Count; i++)
                    {
                        var item = wrapper.tasks[i];
                        if (!string.IsNullOrWhiteSpace(item.id))
                        {
                            _allTasks.Add(item);
                            _tasksById[item.id] = item;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(templatesJson))
            {
                var wrapper = JsonUtility.FromJson<TemplateCollectionWrapper>(templatesJson);
                if (wrapper?.templates != null)
                {
                    for (int i = 0; i < wrapper.templates.Count; i++)
                    {
                        var item = wrapper.templates[i];
                        if (!string.IsNullOrWhiteSpace(item.id))
                        {
                            _allTemplates.Add(item);
                            _templatesById[item.id] = item;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(loreJson))
            {
                var wrapper = JsonUtility.FromJson<LoreCollectionWrapper>(loreJson);
                if (wrapper?.loreCards != null)
                {
                    for (int i = 0; i < wrapper.loreCards.Count; i++)
                    {
                        var item = wrapper.loreCards[i];
                        if (!string.IsNullOrWhiteSpace(item.id))
                        {
                            _allLoreCards.Add(item);
                            _loreById[item.id] = item;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(questsJson))
            {
                var wrapper = JsonUtility.FromJson<QuestCollectionWrapper>(questsJson);
                if (wrapper?.quests != null)
                {
                    for (int i = 0; i < wrapper.quests.Count; i++)
                    {
                        var item = wrapper.quests[i];
                        if (!string.IsNullOrWhiteSpace(item.id))
                        {
                            _allQuests.Add(item);
                            _questsById[item.id] = item;
                        }
                    }
                }
            }
        }

        public bool TryGetTask(string id, out TaskData task)
        {
            if (string.IsNullOrWhiteSpace(id)) { task = null; return false; }
            return _tasksById.TryGetValue(id, out task);
        }

        public bool TryGetTemplate(string id, out TemplateData template)
        {
            if (string.IsNullOrWhiteSpace(id)) { template = null; return false; }
            return _templatesById.TryGetValue(id, out template);
        }

        public bool TryGetLoreCard(string id, out LoreCardData card)
        {
            if (string.IsNullOrWhiteSpace(id)) { card = null; return false; }
            return _loreById.TryGetValue(id, out card);
        }

        public bool TryGetQuest(string id, out QuestData quest)
        {
            if (string.IsNullOrWhiteSpace(id)) { quest = null; return false; }
            return _questsById.TryGetValue(id, out quest);
        }

        private static string ReadResourceText(string resourcePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null) return asset.text;

            string streamingPath = Path.Combine(Application.streamingAssetsPath, resourcePath + ".json");
            if (File.Exists(streamingPath)) return File.ReadAllText(streamingPath);

            return string.Empty;
        }
    }
}
