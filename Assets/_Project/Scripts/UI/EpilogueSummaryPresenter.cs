using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class EpilogueSummaryPresenter : MonoBehaviour
    {
        [Header("Root & Summary Texts (9:16, No Grades 2 or 3)")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TMP_Text epilogueTitleText;
        [SerializeField] private TMP_Text epilogueStoryText;
        [SerializeField] private TMP_Text statsSummaryText;

        [Header("Action Buttons (>= 48 dp)")]
        [SerializeField] private Button enterFreeRoamButton;
        [SerializeField] private Button openPassportReportButton;
        [SerializeField] private Button mainMenuButton;

        private GameState _gameState;
        private LoreJournal _loreJournal;
        private FreeRoamController _freeRoamController;
        private JournalPresenter _journalPresenter;
        private SaveSystem _saveSystem;

        private void Awake()
        {
            if (enterFreeRoamButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(enterFreeRoamButton.GetComponent<RectTransform>());
                enterFreeRoamButton.onClick.AddListener(OnEnterFreeRoamClicked);
            }
            if (openPassportReportButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(openPassportReportButton.GetComponent<RectTransform>());
                openPassportReportButton.onClick.AddListener(OnOpenPassportClicked);
            }
            if (mainMenuButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(mainMenuButton.GetComponent<RectTransform>());
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        public void BindSystems(
            GameState state,
            LoreJournal loreJournal,
            FreeRoamController freeRoam,
            JournalPresenter journal,
            SaveSystem saveSystem)
        {
            _gameState = state;
            _loreJournal = loreJournal;
            _freeRoamController = freeRoam;
            _journalPresenter = journal;
            _saveSystem = saveSystem;
        }

        public void ConfigureUiReferences(
            GameObject root,
            TMP_Text titleText,
            TMP_Text storyText,
            TMP_Text statsText,
            Button freeRoamBtn,
            Button passportBtn,
            Button menuBtn)
        {
            rootPanel = root;
            epilogueTitleText = titleText;
            epilogueStoryText = storyText;
            statsSummaryText = statsText;
            enterFreeRoamButton = freeRoamBtn;
            openPassportReportButton = passportBtn;
            mainMenuButton = menuBtn;
        }

        public void ShowEpilogue()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("MUS-05", 1.2f);
                AudioManager.Instance.PlaySfx("SFX-74");
            }

            if (epilogueTitleText != null)
            {
                epilogueTitleText.SetText("Приключение завершено: будущее спасено!");
            }
            if (epilogueStoryText != null)
            {
                epilogueStoryText.SetText(
                    "Взрослая Соня-2040 благополучно вернулась в 2040 год, а Соня-2027 с памятным значком юного инженера " +
                    "вернулась к семейному ужину в ЖК «ЗимаЛето» (пр. Энергетиков, д. 9, корп. 6).");
            }

            int totalXp = _gameState != null && _gameState.Data != null ? _gameState.Data.totalXp : 2220;
            int solvedTasks = 0;
            if (_gameState != null && _gameState.Data != null && _gameState.Data.attempts != null)
            {
                for (int i = 0; i < _gameState.Data.attempts.Count; i++)
                {
                    if (_gameState.Data.attempts[i].isCorrect)
                    {
                        solvedTasks++;
                    }
                }
            }
            int loreUnlocked = _gameState != null && _gameState.Data != null && _gameState.Data.unlockedLoreIds != null
                ? _gameState.Data.unlockedLoreIds.Count
                : 0;

            if (statsSummaryText != null)
            {
                statsSummaryText.SetText(
                    "• Пройдено глав: 6 из 6 (квесты Q-01 – Q-24)\n" +
                    "• Накоплено опыта: {0} XP\n" +
                    "• Решено заданий 4 класса ФГОС: {1}\n" +
                    "• Открыто карточек «Паспорта места»: {2} из 25",
                    totalXp,
                    solvedTasks,
                    loreUnlocked);
            }
        }

        private void OnEnterFreeRoamClicked()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
            if (_freeRoamController != null)
            {
                _freeRoamController.ActivateFreeRoam();
            }
        }

        private void OnOpenPassportClicked()
        {
            OnEnterFreeRoamClicked();
            if (_journalPresenter != null)
            {
                _journalPresenter.OpenJournal();
                _journalPresenter.ShowParentReportTab();
            }
        }

        private void OnMainMenuClicked()
        {
            if (_saveSystem != null && _gameState != null && _gameState.Data != null)
            {
                _saveSystem.Save(_gameState.Data);
            }
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}
