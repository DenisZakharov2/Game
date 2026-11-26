using UnityEngine;
using TMPro;
using UnityEngine.UI;
using TimeMachine.Data;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class EpilogueSummaryPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text totalXpText;
        [SerializeField] private TMP_Text loreCountText;
        [SerializeField] private TMP_Text correctTasksText;
        [SerializeField] private Button continueFreeRoamButton;
        [SerializeField] private GameObject epilogueRoot;

        [Header("Controllers")]
        [SerializeField] private FreeRoamController freeRoamController;

        private void Awake()
        {
            if (continueFreeRoamButton != null)
            {
                continueFreeRoamButton.onClick.AddListener(OnContinueFreeRoamClicked);
            }
        }

        public void ShowEpilogue()
        {
            if (epilogueRoot != null)
            {
                epilogueRoot.SetActive(true);
            }

            UpdateSummaryView();
        }

        private void UpdateSummaryView()
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null)
            {
                Debug.LogWarning("[TimeMachine] [EpilogueSummaryPresenter] GameState.Instance или CurrentData не проинициализированы!");
                return;
            }

            var data = GameState.Instance.CurrentData;

            // 1. Общий накопленный опыт (XP)
            if (totalXpText != null)
            {
                totalXpText.text = $"Всего XP: {data.totalXp}";
            }

            // 2. Открытые карточки краеведческого лора
            if (loreCountText != null)
            {
                int unlockedLoreCount = data.unlockedLoreIds != null ? data.unlockedLoreIds.Count : 0;
                loreCountText.text = $"Открыто карточек лора: {unlockedLoreCount} / 25";
            }

            // 3. Разбор текстовой записи попыток: "timestamp|taskKey|answer|isCorrect"
            if (correctTasksText != null)
            {
                int correctCount = CalculateCorrectAttempts(data.attempts);
                correctTasksText.text = $"Решено задач ФГОС: {correctCount}";
            }
        }

        private int CalculateCorrectAttempts(System.Collections.Generic.List<string> attempts)
        {
            if (attempts == null) return 0;

            int correctCount = 0;
            foreach (string record in attempts)
            {
                if (string.IsNullOrEmpty(record)) continue;

                string[] parts = record.Split('|');
                if (parts.Length >= 4 && bool.TryParse(parts[3], out bool isCorrect) && isCorrect)
                {
                    correctCount++;
                }
            }
            return correctCount;
        }

        private void OnContinueFreeRoamClicked()
        {
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                GameState.Instance.CurrentData.isStoryCompleted = true;
                GameState.Instance.CurrentData.lessonFreeModeEnabled = true;
                GameState.Instance.SaveCurrentState();
            }

            if (epilogueRoot != null)
            {
                epilogueRoot.SetActive(false);
            }

            if (freeRoamController != null)
            {
                freeRoamController.SetScanningActive(true);
                Debug.Log("[TimeMachine] [EpilogueSummaryPresenter] Свободный режим исследования (Free-Roam) активирован!");
            }
        }
    }
}
