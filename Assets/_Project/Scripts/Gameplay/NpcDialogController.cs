using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    /// <summary>
    /// Контроллер взаимодействия с NPC на сцене.
    /// Переключает состояние речи и генерирует события диалога/задач.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class NpcDialogController : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] private string npcId = "npc_artur";
        [SerializeField] private string npcNameRu = "Смотритель Артур";
        [SerializeField] private string dialogKey = "dlg_artur_01";
        [SerializeField] private string taskKey = "task_math_01";
        [SerializeField] private int activeEra = 2027;

        [Header("Animation & LookAt")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float turnSpeed = 5f;

        private InteractableObject interactable;
        private bool isPlayerNearby = false;

        private void Awake()
        {
            interactable = GetComponent<InteractableObject>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (interactable != null)
            {
                interactable.OnInteracted += TriggerNpcInteraction;
            }
        }

        private void OnDestroy()
        {
            if (interactable != null)
            {
                interactable.OnInteracted -= TriggerNpcInteraction;
            }
        }

        private void Update()
        {
            // Поворот NPC в сторону игрока при приближении
            if (isPlayerNearby && playerTransform != null)
            {
                Vector3 lookDir = playerTransform.position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
            }
        }

        public void SetPlayerTarget(Transform player)
        {
            playerTransform = player;
        }

        private void TriggerNpcInteraction()
        {
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                if (GameState.Instance.CurrentData.currentEra != activeEra)
                {
                    Debug.Log($"[TimeMachine] [NpcDialogController] {npcNameRu} недоступен в эпохе {GameState.Instance.CurrentData.currentEra}. Нужна эпоха {activeEra}.");
                    return;
                }
            }

            if (animator != null)
            {
                animator.SetTrigger("Talk");
            }

            Debug.Log($"[TimeMachine] [NpcDialogController] Диалог с {npcNameRu} (ID: {npcId}) запущен. Диалог: {dialogKey}, Задача: {taskKey}");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;
                if (playerTransform == null) playerTransform = other.transform;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
            }
        }
    }
}
