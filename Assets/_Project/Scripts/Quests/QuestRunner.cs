using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Quests
{
    public class QuestRunner : MonoBehaviour
    {
        public static QuestRunner Instance { get; private set; }

        private GameState _state;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _state = GameState.Instance;
        }

        public string GetCurrentQuestId()
        {
            return _state != null && _state.CurrentData != null 
                ? _state.CurrentData.currentQuestId 
                : string.Empty;
        }

        public QuestProgressEntry GetQuestProgress(string questId)
        {
            if (_state == null || _state.CurrentData == null || _state.CurrentData.quests == null) 
                return null;

            return _state.CurrentData.quests.Find(q => q.questId == questId);
        }

        public void StartQuest(string questId)
        {
            if (_state == null || _state.CurrentData == null) return;

            var progress = GetQuestProgress(questId);
            if (progress == null)
            {
                progress = new QuestProgressEntry
                {
                    questId = questId,
                    status = "InProgress",
                    currentStepIndex = 0
                };
                _state.CurrentData.quests.Add(progress);
            }
            else
            {
                progress.status = "InProgress";
            }

            _state.SetCurrentQuest(questId);
            _state.SaveCurrentState();
            Debug.Log($"[TimeMachine] [QuestRunner] Запущен квест: {questId}");
        }

        public void CompleteQuest(string questId)
        {
            if (_state == null || _state.CurrentData == null) return;

            var progress = GetQuestProgress(questId);
            if (progress != null)
            {
                progress.status = "Completed";
            }
            else
            {
                _state.CurrentData.quests.Add(new QuestProgressEntry
                {
                    questId = questId,
                    status = "Completed",
                    currentStepIndex = 0
                });
            }

            if (!_state.CurrentData.completedQuestIds.Contains(questId))
            {
                _state.CurrentData.completedQuestIds.Add(questId);
            }

            _state.SaveCurrentState();
            Debug.Log($"[TimeMachine] [QuestRunner] Квест завершен: {questId}");
        }

        public bool IsQuestCompleted(string questId)
        {
            if (_state == null || _state.CurrentData == null) return false;
            var progress = GetQuestProgress(questId);
            return (progress != null && progress.status == "Completed") 
                   || _state.CurrentData.completedQuestIds.Contains(questId);
        }
    }
}
