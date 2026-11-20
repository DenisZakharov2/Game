using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Framing (9:16 Portrait)")]
        [SerializeField] private Transform target;
        [SerializeField] private TouchInput touchInput;
        [SerializeField] private float defaultPitch = GameConstants.GameplayTuning.CameraPitch;
        [SerializeField] private float defaultDistance = GameConstants.GameplayTuning.CameraDistance;
        [SerializeField] private float pivotHeight = GameConstants.GameplayTuning.CameraHeight;

        [Header("Rotation & SpringArm")]
        [SerializeField] private float yawSensitivity = 0.22f;
        [SerializeField] private float pitchSensitivity = 0.14f;
        [SerializeField] private float minPitch = 6f;
        [SerializeField] private float maxPitch = 34f;
        [SerializeField] private float sphereCastRadius = 0.25f;
        [SerializeField] private float minWallDistance = 1.1f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private float _yaw;
        private float _pitch;
        private float _currentDistance;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];

        private void Awake()
        {
            _pitch = defaultPitch;
            _currentDistance = defaultDistance;
            if (touchInput == null)
            {
                touchInput = FindAnyObjectByType<TouchInput>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.OnPlayerTeleported += HandlePlayerTeleported;
        }

        private void OnDisable()
        {
            GameEventBus.OnPlayerTeleported -= HandlePlayerTeleported;
        }

        public void SetTarget(Transform playerTransform)
        {
            target = playerTransform;
            if (target != null)
            {
                _yaw = target.eulerAngles.y;
            }
        }

        private void HandlePlayerTeleported(Vector3 pos, float yaw)
        {
            _yaw = yaw;
            _pitch = defaultPitch;
            _currentDistance = defaultDistance;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (touchInput != null)
            {
                Vector2 lookDelta = touchInput.ConsumeLookDelta();
                _yaw += lookDelta.x * yawSensitivity;
                _pitch = Mathf.Clamp(_pitch - lookDelta.y * pitchSensitivity, minPitch, maxPitch);
            }

            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredDirection = -(rotation * Vector3.forward);

            float safeDistance = defaultDistance;
            int hitCount = Physics.SphereCastNonAlloc(
                pivot,
                sphereCastRadius,
                desiredDirection,
                _hitBuffer,
                defaultDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _hitBuffer[i].collider;
                if (col == null || col.transform == target || col.transform.IsChildOf(target))
                {
                    continue;
                }
                float dist = Mathf.Max(minWallDistance, _hitBuffer[i].distance - 0.15f);
                if (dist < safeDistance)
                {
                    safeDistance = dist;
                }
            }

            _currentDistance = Mathf.Lerp(_currentDistance, safeDistance, 14f * Time.deltaTime);
            transform.position = pivot + desiredDirection * _currentDistance;
            transform.rotation = rotation;
        }
    }
}
