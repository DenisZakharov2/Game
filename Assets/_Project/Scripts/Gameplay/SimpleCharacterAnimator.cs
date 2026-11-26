using UnityEngine;
using UnityEngine.AI;

namespace TimeMachine.Gameplay
{
    /// <summary>
    /// Контроллер параметров Animator для Сони, Барни и NPC.
    /// Переключает состояния Idle/Walk на основе реального перемещения.
    /// </summary>
    public class SimpleCharacterAnimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private NavMeshAgent navAgent;

        [Header("Animation Parameters")]
        [SerializeField] private string speedParamName = "Speed";
        [SerializeField] private string isWalkingParamName = "IsWalking";
        [SerializeField] private float dampTime = 0.1f;

        private int speedHash;
        private int isWalkingHash;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();

            speedHash = Animator.StringToHash(speedParamName);
            isWalkingHash = Animator.StringToHash(isWalkingParamName);
        }

        private void Update()
        {
            if (animator == null) return;

            float currentSpeed = CalculateCurrentSpeed();
            bool isWalking = currentSpeed > 0.1f;

            animator.SetFloat(speedHash, currentSpeed, dampTime, Time.deltaTime);
            animator.SetBool(isWalkingHash, isWalking);
        }

        private float CalculateCurrentSpeed()
        {
            if (characterController != null && characterController.enabled)
            {
                Vector3 vel = characterController.velocity;
                vel.y = 0f;
                return vel.magnitude;
            }

            if (navAgent != null && navAgent.enabled)
            {
                Vector3 vel = navAgent.velocity;
                vel.y = 0f;
                return vel.magnitude;
            }

            return 0f;
        }

        public void TriggerAction(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
