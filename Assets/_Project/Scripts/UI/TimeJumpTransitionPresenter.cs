using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class TimeJumpTransitionPresenter : MonoBehaviour
    {
        [Header("Confirmation Modal (9:16)")]
        [SerializeField] private GameObject confirmationModalPanel;
        [SerializeField] private TMP_Text headerTitleText;
        [SerializeField] private TMP_Text targetDateText;
        [SerializeField] private TMP_Text rolesSummaryText;
        [SerializeField] private Button confirmJumpButton;
        [SerializeField] private Button cancelJumpButton;

        [Header("Watercolor-Gold Crossfade Overlay (2.4s, No Scary Flashes)")]
        [SerializeField] private CanvasGroup watercolorOverlay;
        [SerializeField] private TMP_Text transitionSubtitleText;
        [SerializeField] private TMP_Text transitionLocationHintText;

        private EraSwitcher _eraSwitcher;
        private EraLayerId _pendingTargetEra = EraLayerId.Era1950;
        private Action _onTransitionFinished;

        private bool _isTransitioning;
        private float _timer;
        private bool _eraSwitchedInMidpoint;

        private const float FadeInDuration = 0.95f;
        private const float HoldDuration = 0.50f;
        private const float FadeOutDuration = 0.95f;
        private const float TotalDuration = FadeInDuration + HoldDuration + FadeOutDuration;

        private void Awake()
        {
            if (confirmJumpButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(confirmJumpButton.GetComponent<RectTransform>());
                confirmJumpButton.onClick.AddListener(ConfirmJump);
            }
            if (cancelJumpButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(cancelJumpButton.GetComponent<RectTransform>());
                cancelJumpButton.onClick.AddListener(HideConfirmation);
            }
            if (confirmationModalPanel != null) confirmationModalPanel.SetActive(false);
            if (watercolorOverlay != null)
            {
                watercolorOverlay.alpha = 0f;
                watercolorOverlay.blocksRaycasts = false;
                watercolorOverlay.gameObject.SetActive(false);
            }
        }

        public void BindEraSwitcher(EraSwitcher switcher)
        {
            _eraSwitcher = switcher;
        }

        public void ConfigureUiReferences(
            GameObject modalPanel,
            TMP_Text headerText,
            TMP_Text dateText,
            TMP_Text rolesText,
            Button confirmBtn,
            Button cancelBtn,
            CanvasGroup overlay,
            TMP_Text subtitleText,
            TMP_Text locHintText)
        {
            confirmationModalPanel = modalPanel;
            headerTitleText = headerText;
            targetDateText = dateText;
            rolesSummaryText = rolesText;
            confirmJumpButton = confirmBtn;
            cancelJumpButton = cancelBtn;
            watercolorOverlay = overlay;
            transitionSubtitleText = subtitleText;
            transitionLocationHintText = locHintText;
        }

        public void OpenJumpConfirmation(EraLayerId targetEra, Action onComplete = null)
        {
            _pendingTargetEra = targetEra;
            _onTransitionFinished = onComplete;

            if (confirmationModalPanel != null)
            {
                confirmationModalPanel.SetActive(true);
            }

            if (targetEra == EraLayerId.Era1950)
            {
                if (headerTitleText != null) headerTitleText.SetText("Калибровка прыжка: «Заря-7»");
                if (targetDateText != null) targetDateText.SetText("Целевая дата: 14 ноября 1950 года (77 лет назад)");
                if (rolesSummaryText != null)
                {
                    rolesSummaryText.SetText(
                        "Соня-2027 переносится с «Зарёй-7» к перегону у Зубова переулка.\n" +
                        "Саша, Кристина и Лёва остаются в 2027 году и удерживают маяк на «Компасе поколений».");
                }
            }
            else
            {
                if (headerTitleText != null) headerTitleText.SetText("Парный возврат домой: «Заря-7»");
                if (targetDateText != null) targetDateText.SetText("Целевая дата: 2027 год (+77 лет вперёд)");
                if (rolesSummaryText != null)
                {
                    rolesSummaryText.SetText(
                        "Усиленный медный контур и тепловой экран готовы.\n" +
                        "Соня-2027 и взрослая Соня-2040 возвращаются к друзьям у моста через Охту.");
                }
            }
        }

        public void HideConfirmation()
        {
            if (confirmationModalPanel != null)
            {
                confirmationModalPanel.SetActive(false);
            }
        }

        public void ConfirmJump()
        {
            HideConfirmation();
            BeginWatercolorTransition(_pendingTargetEra, _onTransitionFinished);
        }

        public void BeginWatercolorTransition(EraLayerId targetEra, Action onComplete = null)
        {
            _pendingTargetEra = targetEra;
            _onTransitionFinished = onComplete;
            _isTransitioning = true;
            _timer = 0f;
            _eraSwitchedInMidpoint = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-55");
            }

            if (transitionSubtitleText != null)
            {
                transitionSubtitleText.SetText(
                    targetEra == EraLayerId.Era1950 ? "2027 → 14 ноября 1950" : "14 ноября 1950 → 2027");
            }
            if (transitionLocationHintText != null)
            {
                transitionLocationHintText.SetText(
                    targetEra == EraLayerId.Era1950
                        ? "Большая Охта • Перегон у Зубова переулка"
                        : "Большая Охта • Старый ж/д мост через Охту");
            }

            if (watercolorOverlay != null)
            {
                watercolorOverlay.gameObject.SetActive(true);
                watercolorOverlay.alpha = 0f;
                watercolorOverlay.blocksRaycasts = true;
            }
        }

        private void Update()
        {
            if (!_isTransitioning)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;

            if (_timer < FadeInDuration)
            {
                if (watercolorOverlay != null)
                {
                    watercolorOverlay.alpha = Mathf.Clamp01(_timer / FadeInDuration);
                }
            }
            else if (_timer < FadeInDuration + HoldDuration)
            {
                if (watercolorOverlay != null)
                {
                    watercolorOverlay.alpha = 1f;
                }
                if (!_eraSwitchedInMidpoint)
                {
                    _eraSwitchedInMidpoint = true;
                    if (_eraSwitcher != null)
                    {
                        _eraSwitcher.TrySwitchEra(_pendingTargetEra);
                    }
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySfx("SFX-56");
                    }
                }
            }
            else if (_timer < TotalDuration)
            {
                float t = (_timer - FadeInDuration - HoldDuration) / FadeOutDuration;
                if (watercolorOverlay != null)
                {
                    watercolorOverlay.alpha = 1f - Mathf.Clamp01(t);
                }
            }
            else
            {
                _isTransitioning = false;
                if (watercolorOverlay != null)
                {
                    watercolorOverlay.alpha = 0f;
                    watercolorOverlay.blocksRaycasts = false;
                    watercolorOverlay.gameObject.SetActive(false);
                }
                _onTransitionFinished?.Invoke();
            }
        }
    }
}
