using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using TimeMachine.Data;

namespace TimeMachine.UI
{
    public class JournalPresenter : MonoBehaviour
    {
        [Header("UI Tabs & Panels")]
        [SerializeField] private GameObject journalRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text playerXpText;
        [SerializeField] private TMP_Text questProgressText;
        [SerializeField] private TMP_Text loreStatsText;
        [SerializeField] private TMP_Text attemptsStatsText;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseJournal);
            }
        }

        public void OpenJournal()
        {
            if (journalRoot != null)
            {
                journalRoot.SetActive(true);
            }

            RefreshJournalData();
        }

        public void CloseJournal()
        {
            if (journalRoot != null)
            {
                journalRoot.SetActive(false);
            }
        }

        public void RefreshJournalData()
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null)
            {
                Debug.LogWarning("[TimeMachine] [JournalPresenter] GameState.Instance или CurrentData отсутствуют!");
                return;
            }

            var data = GameState.Instance.CurrentData;

            // 1. Отображение опыта и текущего квеста
            if (playerXpText != null)
            {
                playerXpText.text = $"Опыт Сони: {data.playerXP} XP (Всего: {data.totalXp} XP)";
            }

            if (questProgressText != null)
            {
                int completedCount = data.completedQuestIds != null ? data.completedQuestIds.Count : 0;
                questProgressText.text = $"Текущий квест: {data.currentQuestId} | Завершено квестов: {completedCount} / 24";
            }

            // 2. Статистика по карточкам краеведения
            if (loreStatsText != null)
            {
                int unlockedCount = data.unlockedLoreIds != null ? data.unlockedLoreIds.Count : 0;
                loreStatsText.text = $"Краеведческий дневник: {unlockedCount} записей открыто";
            }

            // 3. Статистика решений (разбор формата "timestamp|taskKey|answer|isCorrect")
            if (attemptsStatsText != null)
            {
                int totalAttempts = data.attempts != null ? data.attempts.Count : 0;
                int correctAttempts = CalculateCorrectAttempts(data.attempts);
                attemptsStatsText.text = $"Решено заданий: {correctAttempts} (Всего попыток: {totalAttempts})";
            }
        }

        private int CalculateCorrectAttempts(List<string> attempts)
        {
            if (attempts == null) return 0;

            int count = 0;
            foreach (string record in attempts)
            {
                if (string.IsNullOrEmpty(record)) continue;

                string[] parts = record.Split('|');
                if (parts.Length >= 4 && bool.TryParse(parts[3], out bool isCorrect) && isCorrect)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
