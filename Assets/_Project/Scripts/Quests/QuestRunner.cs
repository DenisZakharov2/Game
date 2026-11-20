using System;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;

namespace TimeMachine.Quests
{
    public class QuestRunner
    {
        private readonly ContentDatabase _database;
        private readonly GameState _state;
        private readonly InventorySystem _inventory;
        private readonly LoreJournal _loreJournal;
        private readonly SaveSystem _saveSystem;

        public QuestRunner(
            ContentDatabase database,
            GameState state,
            InventorySystem inventory,
            LoreJournal loreJournal,
            SaveSystem saveSystem)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _loreJournal = loreJournal ?? throw new ArgumentNullException(nameof(loreJournal));
            _saveSystem = saveSystem;
        }

        public void Initialize()
        {
            if (_state.Data.quests.Count == 0)
            {
                for (int i = 0; i < QuestIds.OrderedChain.Length; i++)
                {
                    _state.Data.quests.Add(new QuestProgressEntry
                    {
                        questId = QuestIds.OrderedChain[i],
                        status = i == 0 ? QuestStatus.Available : QuestStatus.Locked
                    });
                }
            }
        }

        public bool TryStartQuest(string questId)
        {
            QuestProgressEntry entry = FindOrCreateEntry(questId);
            if (entry == null || entry.status == QuestStatus.Completed) return false;
            entry.status = QuestStatus.Active;
            GameEventBus.RaiseQuestStateChanged(questId);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            QuestProgressEntry entry = FindOrCreateEntry(questId);
            if (entry == null || entry.status == QuestStatus.Completed) return false;

            entry.status = QuestStatus.Completed;

            if (_database.TryGetQuest(questId, out QuestData qData))
            {
                if (qData.xpReward > 0)
                {
                    _state.AddXp(qData.xpReward);
                }
                if (!string.IsNullOrWhiteSpace(qData.rewardItemId))
                {
                    _inventory.AddItem(qData.rewardItemId, 1);
                }
                if (!string.IsNullOrWhiteSpace(qData.rewardLoreId))
                {
                    _loreJournal.TryUnlockCard(qData.rewardLoreId);
                }
                if (!string.IsNullOrWhiteSpace(qData.nextQuestId))
                {
                    QuestProgressEntry nextEntry = FindOrCreateEntry(qData.nextQuestId);
                    if (nextEntry != null && nextEntry.status == QuestStatus.Locked)
                    {
                        nextEntry.status = QuestStatus.Available;
                    }
                }
            }

            if (string.Equals(questId, QuestIds.Q24, StringComparison.Ordinal))
            {
                _state.Data.isStoryCompleted = true;
            }

            GameEventBus.RaiseQuestStateChanged(questId);
            _saveSystem?.Save(_state.Data);
            return true;
        }

        public string GetCurrentObjectiveText()
        {
            for (int i = 0; i < _state.Data.quests.Count; i++)
            {
                var q = _state.Data.quests[i];
                if (q.status == QuestStatus.Active || q.status == QuestStatus.Available)
                {
                    if (_database.TryGetQuest(q.questId, out QuestData qData))
                    {
                        return $"Глава {qData.chapter}: {qData.title}";
                    }
                }
            }
            return "Свободный режим: исследование Большой Охты";
        }

        private QuestProgressEntry FindOrCreateEntry(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId)) return null;
            for (int i = 0; i < _state.Data.quests.Count; i++)
            {
                if (string.Equals(_state.Data.quests[i].questId, questId, StringComparison.Ordinal))
                {
                    return _state.Data.quests[i];
                }
            }
            var created = new QuestProgressEntry { questId = questId, status = QuestStatus.Available };
            _state.Data.quests.Add(created);
            return created;
        }
    }
}
