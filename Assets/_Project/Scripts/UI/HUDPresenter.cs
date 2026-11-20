using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class HUDPresenter : MonoBehaviour
    {
        [Header("Top Bar (9:16 SafeArea)")]
        [SerializeField] private TMP_Text questObjectiveText;
        [SerializeField] private TMP_Text xpValueText;
        [SerializeField] private Button pauseSettingsButton;

        [Header("Generations Compass (Opens in Q-06)")]
        [SerializeField] private RectTransform compassRoot;
        [SerializeField] private RectTransform compassNeedle;
        [SerializeField] private TMP_Text compassDistanceText;

        [Header("Companion Speech Bubble")]
        [SerializeField] private GameObject speechBubbleRoot;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text speechMessageText;

        [Header("Bottom Right Action Buttons (>= 48 dp)")]
        [SerializeField] private Button interactButton;
        [SerializeField] private TMP_Text interactPromptText;
        [SerializeField] private Button callFriendButton;
        [SerializeField] private Button journalButton;

        [Header("Linked Panels")]
        [SerializeField] private GameObject journalPanelRoot;
        [SerializeField] private GameObject pauseSettingsPanelRoot;

        private IInteractable _focusedInteractable;
        private CompanionController _activeCompanion;
        private Transform _playerTransform;
        private Transform _compassTargetTransform;
        private float _speechHideTimer;

        private void Awake()
        {
            EnforceButtonTouchTargets();
            if (interactButton != null)
            {
                interactButton.gameObject.SetActive(false);
                interactButton.onClick.AddListener(OnInteractClicked);
            }
            if (callFriendButton != null)
            {
                callFriendButton.onClick.AddListener(OnCallFriendClicked);
            }
            if (journalButton != null)
            {
                journalButton.onClick.AddListener(OnJournalClicked);
            }
            if (pauseSettingsButton != null)
            {
                pauseSettingsButton.onClick.AddListener(OnPauseClicked);
            }
            if (speechBubbleRoot != null)
            {
                speechBubbleRoot.SetActive(false);
            }
            if (compassRoot != null)
            {
                compassRoot.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEventBus.OnXpChanged += HandleXpChanged;
            GameEventBus.OnInteractableFocused += HandleInteractableFocused;
            GameEventBus.OnInteractableCleared += HandleInteractableCleared;
            GameEventBus.OnCompanionSpeak += HandleCompanionSpeak;
            GameEventBus.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            GameEventBus.OnXpChanged -= HandleXpChanged;
            GameEventBus.OnInteractableFocused -= HandleInteractableFocused;
            GameEventBus.OnInteractableCleared -= HandleInteractableCleared;
            GameEventBus.OnCompanionSpeak -= HandleCompanionSpeak;
            GameEventBus.OnInventoryChanged -= HandleInventoryChanged;
        }

        public void ConfigureUiReferences(
            TMP_Text objective,
            TMP_Text xpText,
            Button pauseBtn,
            RectTransform compRoot,
            RectTransform compNeedle,
            TMP_Text compDist,
            GameObject bubbleRoot,
            TMP_Text speakerText,
            TMP_Text msgText,
            Button actBtn,
            TMP_Text actPrompt,
            Button friendBtn,
            Button jrnlBtn)
        {
            questObjectiveText = objective;
            xpValueText = xpText;
            pauseSettingsButton = pauseBtn;
            compassRoot = compRoot;
            compassNeedle = compNeedle;
            compassDistanceText = compDist;
            speechBubbleRoot = bubbleRoot;
            speakerNameText = speakerText;
            speechMessageText = msgText;
            interactButton = actBtn;
            interactPromptText = actPrompt;
            callFriendButton = friendBtn;
            journalButton = jrnlBtn;
            EnforceButtonTouchTargets();
        }

        public void SetLinkedPanels(GameObject journalRoot, GameObject settingsRoot)
        {
            journalPanelRoot = journalRoot;
            pauseSettingsPanelRoot = settingsRoot;
        }

        public void BindWorldActors(Transform player, CompanionController companion)
        {
            _playerTransform = player;
            _activeCompanion = companion;
        }

        public void SetQuestObjective(string text)
        {
            if (questObjectiveText != null)
            {
                questObjectiveText.SetText(text);
            }
        }

        public void SetXpValue(int totalXp)
        {
            if (xpValueText != null)
            {
                xpValueText.SetText("{0} XP", totalXp);
            }
        }

        public void SetCompassTarget(Transform target, bool unlocked)
        {
            _compassTargetTransform = target;
            if (compassRoot != null)
            {
                compassRoot.gameObject.SetActive(unlocked && target != null);
            }
        }

        private void EnforceButtonTouchTargets()
        {
            if (pauseSettingsButton != null) TouchInput.EnforceMinTouchSizeDp(pauseSettingsButton.GetComponent<RectTransform>());
            if (interactButton != null) TouchInput.EnforceMinTouchSizeDp(interactButton.GetComponent<RectTransform>());
            if (callFriendButton != null) TouchInput.EnforceMinTouchSizeDp(callFriendButton.GetComponent<RectTransform>());
            if (journalButton != null) TouchInput.EnforceMinTouchSizeDp(journalButton.GetComponent<RectTransform>());
        }

        private void HandleXpChanged(int delta, int totalXp)
        {
            SetXpValue(totalXp);
        }

        private void HandleInventoryChanged()
        {
            if (compassRoot != null && _compassTargetTransform != null)
            {
                compassRoot.gameObject.SetActive(true);
            }
        }

        private void HandleInteractableFocused(IInteractable interactable)
        {
            _focusedInteractable = interactable;
            if (interactButton != null)
            {
                interactButton.gameObject.SetActive(interactable != null && interactable.CanInteract);
            }
            if (interactPromptText != null && interactable != null)
            {
                interactPromptText.SetText(interactable.PromptLabel);
            }
        }

        private void HandleInteractableCleared()
        {
            _focusedInteractable = null;
            if (interactButton != null)
            {
                interactButton.gameObject.SetActive(false);
            }
        }

        private void HandleCompanionSpeak(string speakerId, string message)
        {
            if (speechBubbleRoot == null)
            {
                return;
            }
            speechBubbleRoot.SetActive(true);
            _speechHideTimer = 5.5f;
            if (speakerNameText != null)
            {
                speakerNameText.SetText(GetSpeakerDisplayName(speakerId));
            }
            if (speechMessageText != null)
            {
                speechMessageText.SetText(message);
            }
        }

        private void OnInteractClicked()
        {
            if (_focusedInteractable != null && _focusedInteractable.CanInteract)
            {
                _focusedInteractable.Interact(GameConstants.CharacterIds.Sonya2027);
            }
        }

        private void OnCallFriendClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-01");
            }
            if (_activeCompanion != null)
            {
                _activeCompanion.SpeakGuidingHint();
            }
            else
            {
                HandleCompanionSpeak(GameConstants.CharacterIds.Sasha, "Сверься с целью наверху экрана и загляни в «Дневник Сони»!");
            }
        }

        private void OnJournalClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-03");
            }
            if (journalPanelRoot != null)
            {
                journalPanelRoot.SetActive(!journalPanelRoot.activeSelf);
            }
        }

        private void OnPauseClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-01");
            }
            if (pauseSettingsPanelRoot != null)
            {
                pauseSettingsPanelRoot.SetActive(!pauseSettingsPanelRoot.activeSelf);
            }
        }

        private void Update()
        {
            if (_speechHideTimer > 0f)
            {
                _speechHideTimer -= Time.unscaledDeltaTime;
                if (_speechHideTimer <= 0f && speechBubbleRoot != null)
                {
                    speechBubbleRoot.SetActive(false);
                }
            }

            if (compassRoot != null && compassRoot.gameObject.activeSelf &&
                _playerTransform != null && _compassTargetTransform != null)
            {
                Vector3 toTarget = _compassTargetTransform.position - _playerTransform.position;
                toTarget.y = 0f;
                float dist = toTarget.magnitude;
                if (compassDistanceText != null)
                {
                    compassDistanceText.SetText("{0} м", Mathf.RoundToInt(dist));
                }
                if (compassNeedle != null && dist > 0.1f)
                {
                    float angle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                    compassNeedle.localRotation = Quaternion.Euler(0f, 0f, -angle);
                }
            }
        }

        private static string GetSpeakerDisplayName(string id)
        {
            switch (id)
            {
                case GameConstants.CharacterIds.Sonya2027: return "Соня (2027)";
                case GameConstants.CharacterIds.Sonya2040: return "Соня (2040)";
                case GameConstants.CharacterIds.Sasha: return "Саша";
                case GameConstants.CharacterIds.Kristina: return "Кристина";
                case GameConstants.CharacterIds.Lyova: return "Лёва";
                case GameConstants.CharacterIds.Artur: return "Смотритель Артур";
                case GameConstants.CharacterIds.Babushka: return "Бабушка Зинаида Петровна";
                case GameConstants.CharacterIds.Barni: return "Шпиц Барни";
                case GameConstants.CharacterIds.Zhulik: return "Кот Жулик";
                default: return "Подсказка";
            }
        }
    }
}
