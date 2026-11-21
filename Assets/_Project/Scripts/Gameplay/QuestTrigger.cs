using TimeMachine.Core;
using TimeMachine.Quests;
using UnityEngine;

namespace TimeMachine.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class QuestTrigger : MonoBehaviour
    {
        [SerializeField] private string questId;
        [SerializeField] private string initialTaskId;
        [SerializeField] private bool triggerOnce = true;

        private QuestRunner _questRunner;
        private bool _triggered;

        public string QuestId => questId;

        public void Configure(string targetQuestId, string firstTaskId, bool once = true)
        {
            questId = targetQuestId;
            initialTaskId = firstTaskId;
            triggerOnce = once;
        }

        public void Bind(QuestRunner questRunner)
        {
            _questRunner = questRunner;
        }

        public void BindQuestRunner(QuestRunner questRunner)
        {
            _questRunner = questRunner;
        }

        public void ResetTrigger()
        {
            _triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && _triggered) return;
            if (!other.TryGetComponent(out PlayerController _)) return;

            _triggered = true;

            if (_questRunner != null)
            {
                _questRunner.StartQuest(questId);
            }
            else
            {
                GameEventBus.RaiseQuestStateChanged(questId);
            }

            if (!string.IsNullOrWhiteSpace(initialTaskId))
            {
                GameEventBus.RaiseTaskRequested(initialTaskId);
            }
        }
    }
}
