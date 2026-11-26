using UnityEngine;

namespace TimeMachine.Gameplay
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private float distance = 4.5f;

        [Header("Camera Angles (Portrait 9:16)")]
        [SerializeField] private float pitch = 25f;
        [SerializeField] private float yaw = 0f;

        [Header("Smoothing & Collision")]
        [SerializeField] private float smoothSpeed = 12f;
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float sphereCastRadius = 0.2f;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[1];

        private void Start()
        {
            ValidateTarget();
        }

        /// <summary>
        /// Явное назначение цели слежения без вызова FindWithTag во время игры.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void ValidateTarget()
        {
            if (target == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[ThirdPersonCamera] Цель (target) не назначена! Поиск игрока по тегу в Editor fallback.", this);
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
#else
                Debug.LogError("[ThirdPersonCamera] КРИТИЧЕСКАЯ ОШИБКА: Цель (target) не установлена!", this);
#endif
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

            // Проверка коллизий со статическими объектами без выделения памяти (NonAlloc)
            Vector3 rayDirection = desiredPosition - targetPosition;
            float rayDistance = rayDirection.magnitude;

            if (Physics.SphereCastNonAlloc(targetPosition, sphereCastRadius, rayDirection.normalized, hitBuffer, rayDistance, collisionLayers) > 0)
            {
                desiredPosition = targetPosition + (rayDirection.normalized * Mathf.Max(hitBuffer[0].distance - 0.1f, 1f));
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.rotation = rotation;
        }
    }
}
