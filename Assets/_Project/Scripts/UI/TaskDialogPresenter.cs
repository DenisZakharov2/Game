using UnityEngine;
using TMPro;
using UnityEngine.UI;
using TimeMachine.Data;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class TaskDialogPresenter : MonoBehaviour
    {
        [Header("UI Elements (TextMeshPro)")]
        [SerializeField] private TMP_Text topicHeaderText;
        [SerializeField] private TMP_Text questionBodyText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject dialogRoot;

        private void Awake()
        {
            if (submitButton != null) submitButton.onClick.AddListener(OnSubmitClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseDialog);
        }

        public void OpenTask(string taskKey, string topicKey)
        {
            if (dialogRoot != null) dialogRoot.SetActive(true);

            string topicStr = ContentDatabase.Instance != null 
                ? ContentDatabase.Instance.GetLocalizedString(topicKey, "Математика / Русский язык (4 класс)") 
                : "Задание";
            
            string questionStr = ContentDatabase.Instance != null 
                ? ContentDatabase.Instance.GetLocalizedString(taskKey, "Задание не найдено.") 
                : taskKey;

            if (topicHeaderText != null) topicHeaderText.text = topicStr;
            if (questionBodyText != null) questionBodyText.text = questionStr;
            if (inputField != null) inputField.text = "";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicDucked(true);
            }
        }

        public void CloseDialog()
        {
            if (dialogRoot != null) dialogRoot.SetActive(false);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicDucked(false);
            }
        }

        private void OnSubmitClicked()
        {
            CloseDialog();
        }
    }
}
