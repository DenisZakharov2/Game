using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Explicit References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController characterController;
        private Vector3 velocity;
        private Vector2 inputVector;
        private bool isControlEnabled = true;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            ValidateDependencies();
        }

        /// <summary>
        /// Явное связывание зависимостей без вызовов Find в runtime.
        /// </summary>
        public void Initialize(Transform targetCameraTransform)
        {
            cameraTransform = targetCameraTransform;
        }

        private void ValidateDependencies()
        {
            if (cameraTransform == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[PlayerController] cameraTransform не назначен вручную! Используется fallback для редактора.", this);
                if (Camera.main != null) cameraTransform = Camera.main.transform;
#else
                Debug.LogError("[PlayerController] КРИТИЧЕСКАЯ ОШИБКА: cameraTransform не проинициализирован!", this);
#endif
            }
        }

        public void SetInputVector(Vector2 input)
        {
            inputVector = input;
        }

        public void SetControlEnabled(bool enabled)
        {
            isControlEnabled = enabled;
            if (!enabled)
            {
                inputVector = Vector2.zero;
            }
        }

        private void Update()
        {
            if (!isControlEnabled) return;

            ApplyGravity();
            MovePlayer();
        }

        private void MovePlayer()
        {
            if (inputVector.sqrMagnitude < 0.01f || cameraTransform == null) return;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * inputVector.y + right * inputVector.x).normalized;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
            }
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Безопасная телепортация персонажа с временным отключением CharacterController.
        /// </summary>
        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            characterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            velocity = Vector3.zero;
            characterController.enabled = true;
        }
    }
}
