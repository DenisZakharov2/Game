using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class TouchInput : MonoBehaviour
    {
        [Header("Virtual Joystick UI (1080x2400 Portrait)")]
        [SerializeField] private RectTransform joystickBase;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRadiusPx = 96f;
        [SerializeField] private bool leftHandedMode;

        private Vector2 _moveAxis;
        private Vector2 _lookDelta;
        private int _moveFingerId = -1;
        private int _lookFingerId = -1;
        private Vector2 _moveOriginScreenPos;
        private Vector2 _lastLookScreenPos;

        public Vector2 MoveAxis => _moveAxis;
        public bool IsLeftHandedMode => leftHandedMode;

        private void Awake()
        {
            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            ResetAxes();
        }

        public void ConfigureJoystickRefs(RectTransform baseRect, RectTransform handleRect)
        {
            joystickBase = baseRect;
            joystickHandle = handleRect;
            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(false);
            }
        }

        public void SetLeftHandedMode(bool isLeftHanded)
        {
            leftHandedMode = isLeftHanded;
            ResetAxes();
        }

        public Vector2 ConsumeLookDelta()
        {
            Vector2 delta = _lookDelta;
            _lookDelta = Vector2.zero;
            return delta;
        }

        public void ResetAxes()
        {
            _moveAxis = Vector2.zero;
            _lookDelta = Vector2.zero;
            _moveFingerId = -1;
            _lookFingerId = -1;
            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(false);
            }
        }

        public static void EnforceMinTouchSizeDp(RectTransform target)
        {
            if (target == null)
            {
                return;
            }
            float dpi = Screen.dpi > 0f ? Screen.dpi : 420f;
            float minPx = Mathf.Max(132f, GameConstants.GameplayTuning.MinTouchSizeDp * (dpi / 160f));
            Vector2 size = target.sizeDelta;
            if (size.x > 0f && size.x < minPx)
            {
                size.x = minPx;
            }
            if (size.y > 0f && size.y < minPx)
            {
                size.y = minPx;
            }
            target.sizeDelta = size;
        }

        private void Update()
        {
            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count > 0)
            {
                ProcessTouches(activeTouches);
            }
            else
            {
                if (_moveFingerId != -1 || _lookFingerId != -1)
                {
                    ResetAxes();
                }
                ProcessEditorKeyboardFallback();
            }
        }

        private void ProcessTouches(UnityEngine.InputSystem.Utilities.ReadOnlyArray<Touch> touches)
        {
            float halfWidth = Screen.width * 0.5f;

            for (int i = 0; i < touches.Count; i++)
            {
                Touch touch = touches[i];
                int fingerId = touch.finger.index;
                Vector2 pos = touch.screenPosition;

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId))
                    {
                        continue;
                    }

                    bool isMoveZone = leftHandedMode ? (pos.x >= halfWidth) : (pos.x < halfWidth);
                    if (isMoveZone && _moveFingerId == -1)
                    {
                        _moveFingerId = fingerId;
                        _moveOriginScreenPos = pos;
                        _moveAxis = Vector2.zero;
                        ShowJoystick(pos);
                    }
                    else if (!isMoveZone && _lookFingerId == -1)
                    {
                        _lookFingerId = fingerId;
                        _lastLookScreenPos = pos;
                    }
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    if (fingerId == _moveFingerId)
                    {
                        Vector2 offset = pos - _moveOriginScreenPos;
                        Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadiusPx);
                        _moveAxis = clamped / joystickRadiusPx;
                        if (joystickHandle != null)
                        {
                            joystickHandle.anchoredPosition = clamped;
                        }
                    }
                    else if (fingerId == _lookFingerId)
                    {
                        _lookDelta += pos - _lastLookScreenPos;
                        _lastLookScreenPos = pos;
                    }
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    if (fingerId == _moveFingerId)
                    {
                        _moveFingerId = -1;
                        _moveAxis = Vector2.zero;
                        if (joystickBase != null)
                        {
                            joystickBase.gameObject.SetActive(false);
                        }
                    }
                    else if (fingerId == _lookFingerId)
                    {
                        _lookFingerId = -1;
                    }
                }
            }
        }

        private void ShowJoystick(Vector2 screenPos)
        {
            if (joystickBase == null)
            {
                return;
            }
            joystickBase.gameObject.SetActive(true);
            joystickBase.position = screenPos;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }

        private void ProcessEditorKeyboardFallback()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }
            float x = 0f;
            float y = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            Vector2 dir = new Vector2(x, y);
            _moveAxis = dir.sqrMagnitude > 1f ? dir.normalized : dir;
#endif
        }
    }
}
