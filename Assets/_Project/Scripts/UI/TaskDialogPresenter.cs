using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class TaskDialogPresenter : MonoBehaviour
    {
        [Header("Root & Texts")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TMP_Text topicHeaderText;
        [SerializeField] private TMP_Text questionBodyText;
        [SerializeField] private TMP_Text hintDisplayText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Inputs (All 6 FGOS Answer Modes)")]
        [SerializeField] private TMP_InputField textAnswerInput;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TMP_Text[] optionButtonLabels;

        [Header("Controls (>= 48 dp)")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button hintFriendButton;
        [SerializeField] private Button hintRuleButton;
        [SerializeField] private Button hintSonya2040Button;
        [SerializeField] private Button lessonFreeModeButton;

        private ContentDatabase _database;
        private TaskEngine _taskEngine;
        private GameState _gameState;
        private SaveSystem _saveSystem;

        private TaskData _currentTask;
        private int _hintsUsed;
        private readonly List<string> _assembledTokens = new List<string>(8);

        private void Awake()
        {
            if (closeButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(closeButton.GetComponent<RectTransform>());
                closeButton.onClick.AddListener(CloseDialog);
            }
            if (submitButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(submitButton.GetComponent<RectTransform>());
                submitButton.onClick.AddListener(SubmitCurrentAnswer);
            }
            if (hintFriendButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(hintFriendButton.GetComponent<RectTransform>());
                hintFriendButton.onClick.AddListener(() => ShowHintLevel(1));
            }
            if (hintRuleButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(hintRuleButton.GetComponent<RectTransform>());
                hintRuleButton.onClick.AddListener(() => ShowHintLevel(2));
            }
            if (hintSonya2040Button != null)
            {
                TouchInput.EnforceMinTouchSizeDp(hintSonya2040Button.GetComponent<RectTransform>());
                hintSonya2040Button.onClick.AddListener(() => ShowHintLevel(3));
            }
            if (lessonFreeModeButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(lessonFreeModeButton.GetComponent<RectTransform>());
                lessonFreeModeButton.onClick.AddListener(CompleteWithLessonFreeMode);
            }
            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    int idx = i;
                    if (optionButtons[i] != null)
                    {
                        TouchInput.EnforceMinTouchSizeDp(optionButtons[i].GetComponent<RectTransform>());
                        optionButtons[i].onClick.AddListener(() => OnOptionClicked(idx));
                    }
                }
            }
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEventBus.OnTaskRequested += OpenTaskById;
        }

        private void OnDisable()
        {
            GameEventBus.OnTaskRequested -= OpenTaskById;
        }

        public void BindSystems(ContentDatabase db, TaskEngine engine, GameState state, SaveSystem saveSystem)
        {
            _database = db;
            _taskEngine = engine;
            _gameState = state;
            _saveSystem = saveSystem;
        }

        public void ConfigureUiReferences(
            GameObject root,
            TMP_Text topicText,
            TMP_Text bodyText,
            TMP_Text hintText,
            TMP_Text fbText,
            TMP_InputField inputField,
            Button[] optBtns,
            TMP_Text[] optLabels,
            Button closeBtn,
            Button subBtn,
            Button h1Btn,
            Button h2Btn,
            Button h3Btn,
            Button freeBtn)
        {
            rootPanel = root;
            topicHeaderText = topicText;
            questionBodyText = bodyText;
            hintDisplayText = hintText;
            feedbackText = fbText;
            textAnswerInput = inputField;
            optionButtons = optBtns;
            optionButtonLabels = optLabels;
            closeButton = closeBtn;
            submitButton = subBtn;
            hintFriendButton = h1Btn;
            hintRuleButton = h2Btn;
            hintSonya2040Button = h3Btn;
            lessonFreeModeButton = freeBtn;
        }

        public void OpenTaskById(string taskId)
        {
            if (_database == null || !_database.TryGetTask(taskId, out TaskData task))
            {
                return;
            }
            OpenTask(task);
        }

        public void OpenTask(TaskData task)
        {
            _currentTask = task;
            _hintsUsed = 0;
            _assembledTokens.Clear();

            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetTaskWindowOpen(true);
                AudioManager.Instance.PlaySfx("SFX-06");
            }

            if (topicHeaderText != null)
            {
                topicHeaderText.SetText($"{task.subject} • {task.fgosTopic}");
            }
            if (questionBodyText != null)
            {
                questionBodyText.SetText(task.questionText);
            }
            if (hintDisplayText != null)
            {
                hintDisplayText.SetText("Если задача кажется сложной, нажми любую подсказку внизу — без таймера и штрафов!");
            }
            if (feedbackText != null)
            {
                feedbackText.SetText(string.Empty);
            }
            if (textAnswerInput != null)
            {
                textAnswerInput.text = string.Empty;
            }

            SetupAnswerMode(task);
        }

        private void SetupAnswerMode(TaskData task)
        {
            bool hasOptions = task.options != null && task.options.Length > 0 &&
                              (task.answerType == "choice" || task.answerType == "wordAssembly" || task.answerType == "sequence");

            if (textAnswerInput != null)
            {
                textAnswerInput.gameObject.SetActive(!hasOptions || task.answerType == "wordAssembly" || task.answerType == "sequence");
            }

            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    if (optionButtons[i] == null)
                    {
                        continue;
                    }
                    bool show = hasOptions && i < task.options.Length;
                    optionButtons[i].gameObject.SetActive(show);
                    if (show && optionButtonLabels != null && i < optionButtonLabels.Length && optionButtonLabels[i] != null)
                    {
                        optionButtonLabels[i].SetText(task.options[i]);
                    }
                }
            }
        }

        private void OnOptionClicked(int index)
        {
            if (_currentTask == null || _currentTask.options == null || index < 0 || index >= _currentTask.options.Length)
            {
                return;
            }

            string picked = _currentTask.options[index];
            if (_currentTask.answerType == "choice")
            {
                if (textAnswerInput != null)
                {
                    textAnswerInput.text = picked;
                }
                EvaluateUserInput(picked);
            }
            else if (_currentTask.answerType == "wordAssembly" || _currentTask.answerType == "sequence")
            {
                _assembledTokens.Add(picked);
                string joined = _currentTask.answerType == "wordAssembly"
                    ? string.Join("-", _assembledTokens)
                    : string.Join(", ", _assembledTokens);
                if (textAnswerInput != null)
                {
                    textAnswerInput.text = joined;
                }
            }
        }

        public void SubmitCurrentAnswer()
        {
            if (_currentTask == null)
            {
                return;
            }
            string raw = textAnswerInput != null ? textAnswerInput.text : string.Empty;
            EvaluateUserInput(raw);
        }

        private void EvaluateUserInput(string rawAnswer)
        {
            if (_currentTask == null)
            {
                return;
            }

            bool isCorrect = false;
            if (_taskEngine != null)
            {
                var result = _taskEngine.Evaluate(_currentTask, rawAnswer, _hintsUsed, false);
                isCorrect = result.isCorrect;
            }
            else
            {
                isCorrect = string.Equals(rawAnswer.Trim(), _currentTask.correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase);
            }

            if (isCorrect)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySfx("SFX-10");
                }
                if (feedbackText != null)
                {
                    feedbackText.SetText($"Верно! {_currentTask.explanation}");
                }
                if (_gameState != null)
                {
                    _gameState.AddXp(25);
                    if (_saveSystem != null && _gameState.Data != null)
                    {
                        _saveSystem.Save(_gameState.Data);
                    }
                }
                string solvedId = _currentTask.id;
                CloseDialog();
                GameEventBus.RaiseTaskCompleted(solvedId, true);
            }
            else
            {
                _assembledTokens.Clear();
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySfx("SFX-11");
                }
                if (feedbackText != null)
                {
                    feedbackText.SetText("Почти получилось! Очки опыта не снимаются — попробуй ещё раз или открой подсказку.");
                }
            }
        }

        public void ShowHintLevel(int level)
        {
            if (_currentTask == null)
            {
                return;
            }
            _hintsUsed = Mathf.Max(_hintsUsed, level);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-07");
            }

            string text = level == 1 ? _currentTask.hint1Friend :
                          level == 2 ? _currentTask.hint2Rule :
                                       _currentTask.hint3Sonya2040;
            if (hintDisplayText != null)
            {
                hintDisplayText.SetText(text);
            }
        }

        public void CompleteWithLessonFreeMode()
        {
            if (_currentTask == null)
            {
                return;
            }
            if (_taskEngine != null)
            {
                _taskEngine.Evaluate(_currentTask, _currentTask.correctAnswer, 3, true);
            }
            string solvedId = _currentTask.id;
            CloseDialog();
            GameEventBus.RaiseTaskCompleted(solvedId, false);
        }

        public void CloseDialog()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetTaskWindowOpen(false);
            }
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}
