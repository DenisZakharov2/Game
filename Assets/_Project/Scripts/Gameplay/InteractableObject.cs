using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionId = "INT_Default";
        [SerializeField] private string promptLabel = "Осмотреть объект";
        [SerializeField] private string linkedTaskId = "";
        [SerializeField] private string linkedLoreId = "";
        [SerializeField] private bool singleUse = true;

        private bool _used;

        public string InteractionId => interactionId;
        public string PromptLabel => promptLabel;
        public bool CanInteract => !singleUse || !_used;

        public void Configure(string id, string prompt, string taskId, bool isSingleUse)
        {
            interactionId = id;
            promptLabel = prompt;
            linkedTaskId = taskId;
            singleUse = isSingleUse;
        }

        public void SetLinkedLoreId(string loreId)
        {
            linkedLoreId = loreId;
        }

        public void Interact(string actorId)
        {
            if (!CanInteract)
            {
                return;
            }
            if (singleUse)
            {
                _used = true;
                GameEventBus.RaiseInteractableCleared();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-02");
            }

            if (!string.IsNullOrEmpty(linkedLoreId))
            {
                GameEventBus.RaiseLoreCardUnlocked(linkedLoreId);
            }

            if (!string.IsNullOrEmpty(linkedTaskId))
            {
                GameEventBus.RaiseTaskRequested(linkedTaskId);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!CanInteract)
            {
                return;
            }
            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                GameEventBus.RaiseInteractableFocused(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                GameEventBus.RaiseInteractableCleared();
            }
        }
    }
}
