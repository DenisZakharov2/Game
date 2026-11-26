using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Data
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public SaveDataV1 CurrentData { get; private set; }
        private SaveSystem _saveSystem;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeState()
        {
            _saveSystem = new SaveSystem();
            CurrentData = _saveSystem.LoadOrCreateDefault();
            if (CurrentData == null)
            {
                CurrentData = CreateNewDefaultState();
            }
        }

        public SaveDataV1 CreateNewDefaultState()
        {
            SaveDataV1 data = new SaveDataV1
            {
                schemaVersion = SaveSystem.CURRENT_SCHEMA_VERSION,
                currentQuestId = "q_2027_01",
                currentEra = 2027,
                playerXP = 0,
                totalXp = 0,
                isZarya7Assembled = false,
                isStoryCompleted = false,
                lessonFreeModeEnabled = false,
                activeCompanionId = "barni",
                quests = new List<QuestProgressEntry>(),
                skills = new List<SkillProgressEntry>(),
                inventory = new List<string>(),
                unlockedLoreIds = new List<string>(),
                completedQuestIds = new List<string>(),
                attempts = new List<string>()
            };
            SaveSystem.Save(data);
            return data;
        }

        public void SaveCurrentState()
        {
            if (CurrentData != null)
            {
                SaveSystem.Save(CurrentData);
            }
        }

        public void AddXP(int amount)
        {
            if (CurrentData == null) return;
            CurrentData.playerXP += amount;
            CurrentData.totalXp += amount;
            SaveCurrentState();
        }

        public void SetCurrentQuest(string questId)
        {
            if (CurrentData == null) return;
            CurrentData.currentQuestId = questId;
            SaveCurrentState();
        }

        public void SetCurrentEra(int era)
        {
            if (CurrentData == null) return;
            CurrentData.currentEra = era;
            SaveCurrentState();
        }

        public void AddInventoryItem(string itemId)
        {
            if (CurrentData == null || string.IsNullOrEmpty(itemId)) return;
            if (!CurrentData.inventory.Contains(itemId))
            {
                CurrentData.inventory.Add(itemId);
                SaveCurrentState();
            }
        }

        public void UpdateSkillProgress(string skillId, int level)
        {
            if (CurrentData == null || string.IsNullOrEmpty(skillId)) return;
            var entry = CurrentData.skills.Find(s => s.skillId == skillId);
            if (entry != null)
            {
                entry.level = level;
            }
            else
            {
                CurrentData.skills.Add(new SkillProgressEntry { skillId = skillId, level = level });
            }
            SaveCurrentState();
        }
    }
}
