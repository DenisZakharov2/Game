using System;
using System.Collections.Generic;
using TimeMachine.Core;

namespace TimeMachine.Data
{
    public enum QuestStatus
    {
        Locked = 0,
        Available = 1,
        Active = 2,
        Completed = 3
    }

    [Serializable]
    public class TaskData
    {
        public string id;
        public string questId;
        public string locationId;
        public string subject;
        public string fgosTopic;
        public string answerType;
        public string questionText;
        public string correctAnswer;
        public string[] acceptedVariants;
        public string[] options;
        public string hint1Friend;
        public string hint2Rule;
        public string hint3Sonya2040;
        public string explanation;
    }

    [Serializable]
    public class TemplateData
    {
        public string id;
        public string subject;
        public string fgosTopic;
        public string answerType;
        public string unit;
        public string formula;
        public string[] contexts;
    }

    [Serializable]
    public class LoreCardData
    {
        public string id;
        public string theme;
        public string title;
        public string locationId;
        public string sourceUrl;
        public string historicalText;
    }

    [Serializable]
    public class QuestData
    {
        public string id;
        public int chapter;
        public string eraLayer;
        public string locationId;
        public string title;
        public string primaryTaskId;
        public string rewardLoreId;
        public string rewardItemId;
        public int xpReward;
        public string nextQuestId;
    }

    [Serializable]
    public class InventoryItemEntry
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class QuestProgressEntry
    {
        public string questId;
        public QuestStatus status;
    }

    [Serializable]
    public class AttemptRecord
    {
        public string taskId;
        public string fgosTopic;
        public string subject;
        public string submittedAnswer;
        public bool isCorrect;
        public int hintsUsed;
        public bool lessonFreeMode;
    }

    [Serializable]
    public class SkillLevelEntry
    {
        public SkillBranch branch;
        public int level;
    }

    [Serializable]
    public class SaveDataV1
    {
        public int schemaVersion = GameConstants.GameplayTuning.CurrentSaveSchemaVersion;
        public string savedAtIsoUtc;
        public int totalXp;
        public bool isZarya7Assembled;
        public bool isStoryCompleted;
        public bool lessonFreeModeEnabled;
        public string currentEra = "2027";
        public string activeCompanionId = GameConstants.CharacterIds.Sasha;
        public List<InventoryItemEntry> inventory = new List<InventoryItemEntry>();
        public List<QuestProgressEntry> quests = new List<QuestProgressEntry>();
        public List<string> unlockedLoreIds = new List<string>();
        public List<AttemptRecord> attempts = new List<AttemptRecord>();
        public List<SkillLevelEntry> skills = new List<SkillLevelEntry>();
    }

    [Serializable]
    public class TaskCollectionWrapper { public List<TaskData> tasks = new List<TaskData>(); }

    [Serializable]
    public class TemplateCollectionWrapper { public List<TemplateData> templates = new List<TemplateData>(); }

    [Serializable]
    public class LoreCollectionWrapper { public List<LoreCardData> loreCards = new List<LoreCardData>(); }

    [Serializable]
    public class QuestCollectionWrapper { public List<QuestData> quests = new List<QuestData>(); }
}
