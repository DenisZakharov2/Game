using System;
using TimeMachine.Core;

namespace TimeMachine.Data
{
    public class GameState
    {
        public SaveDataV1 Data { get; private set; }

        public GameState(SaveDataV1 initialData = null)
        {
            Data = initialData ?? CreateNewDefaultState();
            EnsureInitialized();
        }

        public static SaveDataV1 CreateNewDefaultState()
        {
            var data = new SaveDataV1
            {
                schemaVersion = GameConstants.GameplayTuning.CurrentSaveSchemaVersion,
                savedAtIsoUtc = DateTime.UtcNow.ToString("o"),
                totalXp = 0,
                isZarya7Assembled = false,
                isStoryCompleted = false,
                lessonFreeModeEnabled = false,
                currentEra = "2027",
                activeCompanionId = GameConstants.CharacterIds.Sasha
            };

            for (int i = 0; i < QuestIds.OrderedChain.Length; i++)
            {
                data.quests.Add(new QuestProgressEntry
                {
                    questId = QuestIds.OrderedChain[i],
                    status = i == 0 ? QuestStatus.Available : QuestStatus.Locked
                });
            }

            foreach (SkillBranch branch in Enum.GetValues(typeof(SkillBranch)))
            {
                data.skills.Add(new SkillLevelEntry { branch = branch, level = 0 });
            }

            return data;
        }

        private void EnsureInitialized()
        {
            if (Data.quests == null) Data.quests = new System.Collections.Generic.List<QuestProgressEntry>();
            if (Data.skills == null) Data.skills = new System.Collections.Generic.List<SkillLevelEntry>();
            if (Data.inventory == null) Data.inventory = new System.Collections.Generic.List<InventoryItemEntry>();
            if (Data.unlockedLoreIds == null) Data.unlockedLoreIds = new System.Collections.Generic.List<string>();
            if (Data.attempts == null) Data.attempts = new System.Collections.Generic.List<AttemptRecord>();

            foreach (SkillBranch branch in Enum.GetValues(typeof(SkillBranch)))
            {
                bool found = false;
                for (int i = 0; i < Data.skills.Count; i++)
                {
                    if (Data.skills[i].branch == branch) { found = true; break; }
                }
                if (!found) Data.skills.Add(new SkillLevelEntry { branch = branch, level = 0 });
            }
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            Data.totalXp += amount;
            GameEventBus.RaiseXpChanged(amount, Data.totalXp);
        }

        public int GetSkillLevel(SkillBranch branch)
        {
            for (int i = 0; i < Data.skills.Count; i++)
            {
                if (Data.skills[i].branch == branch) return Data.skills[i].level;
            }
            return 0;
        }

        public bool TryUpgradeSkill(SkillBranch branch, int xpCost = GameConstants.GameplayTuning.XpPerSkillLevel)
        {
            if (Data.totalXp < xpCost) return false;
            for (int i = 0; i < Data.skills.Count; i++)
            {
                if (Data.skills[i].branch == branch)
                {
                    if (Data.skills[i].level >= GameConstants.GameplayTuning.MaxSkillLevel) return false;
                    Data.totalXp -= xpCost;
                    Data.skills[i].level++;
                    GameEventBus.RaiseXpChanged(-xpCost, Data.totalXp);
                    GameEventBus.RaiseSkillUpgraded(branch, Data.skills[i].level);
                    return true;
                }
            }
            return false;
        }
    }
}
