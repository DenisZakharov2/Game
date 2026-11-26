using System;
using System.Collections.Generic;

namespace TimeMachine.Data
{
    [Serializable]
    public class QuestProgressEntry
    {
        public string questId = "";
        public string status = "NotStarted"; // "NotStarted", "InProgress", "Completed"
        public int currentStepIndex = 0;
    }

    [Serializable]
    public class SkillProgressEntry
    {
        public string skillId = "";
        public int level = 1;
    }

    [Serializable]
    public class SaveDataV1
    {
        public int schemaVersion = 1;
        public string currentQuestId = "q_2027_01";
        public int currentEra = 2027; // Числовой тип int
        public int playerXP = 0;
        public int totalXp = 0;
        public bool isZarya7Assembled = false;
        public bool isStoryCompleted = false;
        public bool lessonFreeModeEnabled = false;
        public string activeCompanionId = "barni";
        public string savedAtIsoUtc = "";

        public List<QuestProgressEntry> quests = new List<QuestProgressEntry>();
        public List<SkillProgressEntry> skills = new List<SkillProgressEntry>();
        public List<string> inventory = new List<string>();
        public List<string> unlockedLoreIds = new List<string>();
        public List<string> completedQuestIds = new List<string>();
        public List<string> attempts = new List<string>();
    }
}
