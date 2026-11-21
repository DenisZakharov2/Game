using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Quests;

namespace TimeMachine.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestTrigger : MonoBehaviour
    {
        [SerializeField] private string questId = QuestIds.Q01;
        [SerializeField] private string firstTaskId = "T-RUS-01";
        [SerializeField] private bool autoStartOnEnter = true;

        private QuestRunner _questRunner;
        private bool _triggeredInSession;

        public string QuestId => questId;
        public string FirstTaskId => firstTaskId;

        public void Configure(string targetQuestId, string targetTaskId, bool autoStart)
        {
            questId = targetQuestId;
            firstTaskId = targetTaskId;
            autoStartOnEnter = autoStart;
        }

        public void BindQuestRunner(QuestRunner runner)
        {
            _questRunner = runner;
        }

        private void Awake()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!autoStartOnEnter || _triggeredInSession)
            {
                return;
            }
            if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            _triggeredInSession = true;
            if (_questRunner != null)
            {
                _questRunner.TryStartQuest(questId);
            }
            else
            {
                GameEventBus.RaiseQuestStateChanged(questId);
            }

            if (!string.IsNullOrEmpty(firstTaskId))
            {
                GameEventBus.RaiseTaskRequested(firstTaskId);
            }
        }
    }
}
