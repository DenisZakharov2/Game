using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class JournalPresenter : MonoBehaviour
    {
        [Header("Root & Tabs (>= 48 dp)")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TMP_Text journalBodyText;
        [SerializeField] private Button tabLoreHistoryButton;
        [SerializeField] private Button tabSkillsButton;
        [SerializeField] private Button tabParentReportButton;
        [SerializeField] private Button closeButton;

        [Header("SubPanels")]
        [SerializeField] private GameObject subPanelNote1950;
        [SerializeField] private GameObject subPanelPlacePassport;

        private ContentDatabase _database;
        private GameState _gameState;
        private InventorySystem _inventory;
        private LoreJournal _loreJournal;
        private SaveSystem _saveSystem;

        private readonly StringBuilder _sb = new StringBuilder(2048);
        private readonly List<LoreCardData> _verifiedCardsBuffer = new List<LoreCardData>(32);

        private void Awake()
        {
            if (tabLoreHistoryButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(tabLoreHistoryButton.GetComponent<RectTransform>());
                tabLoreHistoryButton.onClick.AddListener(ShowLoreHistoryTab);
            }
            if (tabSkillsButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(tabSkillsButton.GetComponent<RectTransform>());
                tabSkillsButton.onClick.AddListener(ShowSkillsAndBackpackTab);
            }
            if (tabParentReportButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(tabParentReportButton.GetComponent<RectTransform>());
                tabParentReportButton.onClick.AddListener(ShowParentReportTab);
            }
            if (closeButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(closeButton.GetComponent<RectTransform>());
                closeButton.onClick.AddListener(CloseJournal);
            }
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        public void BindSystems(
            ContentDatabase db,
            GameState state,
            InventorySystem inventory,
            LoreJournal loreJournal,
            SaveSystem saveSystem)
        {
            _database = db;
            _gameState = state;
            _inventory = inventory;
            _loreJournal = loreJournal;
            _saveSystem = saveSystem;
        }

        public void ConfigureUiReferences(
            GameObject root,
            TMP_Text bodyText,
            Button loreBtn,
            Button skillsBtn,
            Button parentBtn,
            Button closeBtn,
            GameObject note1950Panel,
            GameObject passportPanel)
        {
            rootPanel = root;
            journalBodyText = bodyText;
            tabLoreHistoryButton = loreBtn;
            tabSkillsButton = skillsBtn;
            tabParentReportButton = parentBtn;
            closeButton = closeBtn;
            subPanelNote1950 = note1950Panel;
            subPanelPlacePassport = passportPanel;
        }

        public void OpenJournal()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            ShowLoreHistoryTab();
        }

        public void CloseJournal()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        public void ShowLoreHistoryTab()
        {
            SetSubPanels(false, false);
            _sb.Clear();
            _sb.AppendLine("<b>ИСТОРИЯ МЕСТА: БОЛЬШАЯ ОХТА (1950 ↔ 2027)</b>");
            _sb.AppendLine("Открываются только краеведческие карточки с подтверждённым источником:
");

            if (_loreJournal != null)
            {
                _loreJournal.GetUnlockedVerifiedCards(_verifiedCardsBuffer);
                if (_verifiedCardsBuffer.Count == 0)
                {
                    _sb.AppendLine("Пока карточек нет — исследуй горку-убежище, двор дома № 71 и мост через Охту!");
                }
                else
                {
                    for (int i = 0; i < _verifiedCardsBuffer.Count; i++)
                    {
                        LoreCardData card = _verifiedCardsBuffer[i];
                        _sb.Append("• <b>").Append(card.title).Append("</b> (").Append(card.theme).AppendLine(")");
                        _sb.AppendLine(card.historicalText);
                        _sb.Append("<i>Источник: ").Append(card.sourceUrl).AppendLine("</i>
");
                    }
                }
            }

            if (journalBodyText != null)
            {
                journalBodyText.SetText(_sb.ToString());
            }
        }

        public void ShowSkillsAndBackpackTab()
        {
            SetSubPanels(false, false);
            _sb.Clear();
            _sb.AppendLine("<b>УРОКИ, НАВЫКИ И РЮКЗАК СОНИ</b>
");

            if (_gameState != null && _gameState.Data != null)
            {
                _sb.Append("Накоплено опыта: <b>").Append(_gameState.Data.totalXp).AppendLine(" XP</b>");
                _sb.Append("• Знание (Математика): ур. ").Append(_gameState.GetSkillLevel(SkillBranch.Knowledge)).AppendLine(" / 3");
                _sb.Append("• Речь (Русский язык): ур. ").Append(_gameState.GetSkillLevel(SkillBranch.Speech)).AppendLine(" / 3");
                _sb.Append("• Город (Ориентирование): ур. ").Append(_gameState.GetSkillLevel(SkillBranch.City)).AppendLine(" / 3");
                _sb.Append("• Знание места (Краеведение): ур. ").Append(_gameState.GetSkillLevel(SkillBranch.PlaceLore)).AppendLine(" / 3
");
            }

            if (_inventory != null)
            {
                _sb.Append("Масса узлов «Зари-7» в рюкзаке: <b>")
                   .Append(_inventory.GetZarya7TotalMassGrams())
                   .AppendLine(" г / 3 000 г (3 кг)</b>");
            }

            if (journalBodyText != null)
            {
                journalBodyText.SetText(_sb.ToString());
            }
        }

        public void ShowParentReportTab()
        {
            SetSubPanels(false, true);
            _sb.Clear();
            _sb.AppendLine("<b>ОТЧЁТ ДЛЯ РОДИТЕЛЕЙ (4 КЛАСС ФГОС НОО)</b>");
            _sb.AppendLine("В игре нет школьных оценок «2» и «3», таймеров и штрафов за ошибки.
");

            if (_gameState != null && _gameState.Data != null)
            {
                int attemptsCount = _gameState.Data.attempts != null ? _gameState.Data.attempts.Count : 0;
                int solvedCount = 0;
                if (_gameState.Data.attempts != null)
                {
                    for (int i = 0; i < _gameState.Data.attempts.Count; i++)
                    {
                        if (_gameState.Data.attempts[i].isCorrect)
                        {
                            solvedCount++;
                        }
                    }
                }
                _sb.Append("• Решено учебных заданий: <b>").Append(solvedCount).AppendLine("</b>");
                _sb.Append("• Всего спокойных попыток: <b>").Append(attemptsCount).AppendLine("</b>");
                _sb.Append("• Общий прогресс опыта: <b>").Append(_gameState.Data.totalXp).AppendLine(" XP</b>");
            }

            _sb.AppendLine("
<b>ПАСПОРТ МЕСТА: БОЛЬШАЯ ОХТА И МАГНИТОГОРСКАЯ УЛИЦА</b>");
            _sb.AppendLine("1950 г. (Зубов переулок, завод ЖБИ «Баррикада», осн. 1901 г.) ↔ 2027 г. (ул. Магнитогорская, ЖК «ЗимаЛето», мост через Охту).");

            if (journalBodyText != null)
            {
                journalBodyText.SetText(_sb.ToString());
            }
        }

        public void ShowNote1950SubPanel()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            SetSubPanels(true, false);
        }

        private void SetSubPanels(bool note1950, bool passport)
        {
            if (subPanelNote1950 != null)
            {
                subPanelNote1950.SetActive(note1950);
            }
            if (subPanelPlacePassport != null)
            {
                subPanelPlacePassport.SetActive(passport);
            }
        }
    }
}
