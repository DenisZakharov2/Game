using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class KitchenPortalTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string portalId = "TRIG_Door_Kitchen";
        [SerializeField] private string promptLabel = "Войти на кухню (ЖК «ЗимаЛето»)";
        [SerializeField] private Vector3 destinationPosition = new Vector3(500f, 0.1f, 498.5f);
        [SerializeField] private float destinationYaw;

        public string InteractionId => portalId;
        public string PromptLabel => promptLabel;
        public bool CanInteract => true;

        public void Configure(string id, string prompt, Vector3 targetPos, float targetYaw)
        {
            portalId = id;
            promptLabel = prompt;
            destinationPosition = targetPos;
            destinationYaw = targetYaw;
        }

        public void Interact(string actorId)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-29");
            }
            GameEventBus.RaiseInteractableCleared();
            GameEventBus.RaisePlayerTeleported(destinationPosition, destinationYaw);
        }

        private void OnTriggerEnter(Collider other)
        {
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
