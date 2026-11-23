using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif
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

        private Canvas _parentCanvas;
        private EventSystem _cachedEventSystem;
        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);

        public Vector2 MoveAxis => _moveAxis;
        public bool IsLeftHandedMode => leftHandedMode;

        private void Awake()
        {
            CacheParentCanvas();
            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Disable();
            }
#endif
            ResetAxes();
        }

        public void ConfigureJoystickRefs(RectTransform baseRect, RectTransform handleRect)
        {
            joystickBase = baseRect;
            joystickHandle = handleRect;
            CacheParentCanvas();
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

        private void CacheParentCanvas()
        {
            if (joystickBase != null)
            {
                _parentCanvas = joystickBase.GetComponentInParent<Canvas>();
            }
            if (_parentCanvas == null)
            {
                _parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        private float GetCanvasScaleFactor()
        {
            if (_parentCanvas == null)
            {
                CacheParentCanvas();
            }
            return (_parentCanvas != null && _parentCanvas.scaleFactor > 0.01f) ? _parentCanvas.scaleFactor : 1f;
        }

        private void Update()
        {
            bool handledTouch = false;

#if ENABLE_INPUT_SYSTEM
            var activeTouches = EnhancedTouch.activeTouches;
            if (activeTouches.Count > 0)
            {
                ProcessEnhancedTouches(activeTouches);
                handledTouch = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (!handledTouch && Input.touchCount > 0)
            {
                ProcessLegacyTouches();
                handledTouch = true;
            }
#endif

            if (!handledTouch)
            {
                if (_moveFingerId != -1 || _lookFingerId != -1)
                {
                    ResetAxes();
                }
                ProcessEditorKeyboardFallback();
            }
        }

#if ENABLE_INPUT_SYSTEM
        private void ProcessEnhancedTouches(UnityEngine.InputSystem.Utilities.ReadOnlyArray<EnhancedTouch> touches)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                EnhancedTouch touch = touches[i];
                int fingerId = touch.finger.index;
                Vector2 pos = touch.screenPosition;

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    HandleTouchBegan(fingerId, pos);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    HandleTouchMoved(fingerId, pos);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    HandleTouchEnded(fingerId);
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        private void ProcessLegacyTouches()
        {
            int count = Input.touchCount;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Touch touch = Input.GetTouch(i);
                int fingerId = touch.fingerId;
                Vector2 pos = touch.position;

                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    HandleTouchBegan(fingerId, pos);
                }
                else if (touch.phase == UnityEngine.TouchPhase.Moved ||
                         touch.phase == UnityEngine.TouchPhase.Stationary)
                {
                    HandleTouchMoved(fingerId, pos);
                }
                else if (touch.phase == UnityEngine.TouchPhase.Ended ||
                         touch.phase == UnityEngine.TouchPhase.Canceled)
                {
                    HandleTouchEnded(fingerId);
                }
            }
        }
#endif

        private void HandleTouchBegan(int fingerId, Vector2 pos)
        {
            if (IsScreenPosOverUi(pos))
            {
                return;
            }

            float halfWidth = Screen.width * 0.5f;
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

        private void HandleTouchMoved(int fingerId, Vector2 pos)
        {
            if (fingerId == _moveFingerId)
            {
                float scaleFactor = GetCanvasScaleFactor();
                float radiusScreenPx = Mathf.Max(24f, joystickRadiusPx * scaleFactor);
                Vector2 offset = pos - _moveOriginScreenPos;
                Vector2 clamped = Vector2.ClampMagnitude(offset, radiusScreenPx);
                _moveAxis = clamped / radiusScreenPx;
                if (joystickHandle != null)
                {
                    joystickHandle.anchoredPosition = clamped / scaleFactor;
                }
            }
            else if (fingerId == _lookFingerId)
            {
                _lookDelta += pos - _lastLookScreenPos;
                _lastLookScreenPos = pos;
            }
        }

        private void HandleTouchEnded(int fingerId)
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

        private bool IsScreenPosOverUi(Vector2 screenPosition)
        {
            EventSystem es = EventSystem.current;
            if (es == null)
            {
                return false;
            }

            if (_pointerEventData == null || _cachedEventSystem != es)
            {
                _cachedEventSystem = es;
                _pointerEventData = new PointerEventData(es);
            }

            _pointerEventData.position = screenPosition;
            _uiRaycastResults.Clear();
            es.RaycastAll(_pointerEventData, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
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
            float x = 0f;
            float y = 0f;
#if ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            x = Input.GetAxisRaw("Horizontal");
            y = Input.GetAxisRaw("Vertical");
#endif
            Vector2 dir = new Vector2(x, y);
            _moveAxis = dir.sqrMagnitude > 1f ? dir.normalized : dir;
#endif
        }
    }
}
