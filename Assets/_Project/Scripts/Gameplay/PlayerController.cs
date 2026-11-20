using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings (9:16 Portrait)")]
        [SerializeField] private float walkSpeed = GameConstants.GameplayTuning.WalkSpeed;
        [SerializeField] private float runSpeed = GameConstants.GameplayTuning.RunSpeed;
        [SerializeField] private float rotationSmoothSpeed = 12f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float stepFootstepInterval = 0.48f;

        [Header("References")]
        [SerializeField] private TouchInput touchInput;
        [SerializeField] private Transform cameraTransform;

        private CharacterController _controller;
        private float _verticalVelocity;
        private float _footstepTimer;
        private bool _movementLocked;
        private SurfaceType _currentSurface = SurfaceType.Asphalt;

        public bool IsMovementLocked
        {
            get => _movementLocked;
            set => _movementLocked = value;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.slopeLimit = 35f;
            _controller.stepOffset = 0.35f;
            if (touchInput == null)
            {
                touchInput = FindAnyObjectByType<TouchInput>();
            }
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            GameEventBus.OnPlayerTeleported += HandleTeleport;
        }

        private void OnDisable()
        {
            GameEventBus.OnPlayerTeleported -= HandleTeleport;
        }

        public void SetCameraTransform(Transform camTransform)
        {
            cameraTransform = camTransform;
        }

        public void SetTouchInput(TouchInput input)
        {
            touchInput = input;
        }

        public void SetSurface(SurfaceType surface)
        {
            _currentSurface = surface;
        }

        private void HandleTeleport(Vector3 targetPosition, float targetYaw)
        {
            _controller.enabled = false;
            transform.position = targetPosition;
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
            _verticalVelocity = 0f;
            _controller.enabled = true;
        }

        private void Update()
        {
            if (!_controller.enabled)
            {
                return;
            }

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            _verticalVelocity += gravity * Time.deltaTime;

            Vector2 moveInput = Vector2.zero;
            if (!_movementLocked && touchInput != null)
            {
                moveInput = touchInput.MoveAxis;
            }

            float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);
            Vector3 moveDirection = Vector3.zero;

            if (inputMagnitude > 0.05f)
            {
                Vector3 forward = Vector3.forward;
                Vector3 right = Vector3.right;

                if (cameraTransform != null)
                {
                    forward = cameraTransform.forward;
                    forward.y = 0f;
                    if (forward.sqrMagnitude > 0.0001f)
                    {
                        forward.Normalize();
                    }
                    right = cameraTransform.right;
                    right.y = 0f;
                    if (right.sqrMagnitude > 0.0001f)
                    {
                        right.Normalize();
                    }
                }

                moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
                Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);

                float currentSpeed = inputMagnitude > 0.72f ? runSpeed : walkSpeed;
                Vector3 velocity = moveDirection * (currentSpeed * inputMagnitude);
                velocity.y = _verticalVelocity;
                _controller.Move(velocity * Time.deltaTime);

                _footstepTimer += Time.deltaTime * (inputMagnitude > 0.72f ? 1.35f : 1.0f);
                if (_footstepTimer >= stepFootstepInterval)
                {
                    _footstepTimer = 0f;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayFootstep(_currentSurface);
                    }
                }
            }
            else
            {
                _footstepTimer = stepFootstepInterval * 0.7f;
                Vector3 gravityOnly = new Vector3(0f, _verticalVelocity, 0f);
                _controller.Move(gravityOnly * Time.deltaTime);
            }
        }
    }
}
