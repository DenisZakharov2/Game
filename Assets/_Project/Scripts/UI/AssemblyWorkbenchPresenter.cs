using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class AssemblyWorkbenchPresenter : MonoBehaviour
    {
        [Header("Root & Blueprint Header (9:16, #E6EEF2)")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TMP_Text stepHeaderText;
        [SerializeField] private TMP_Text massCounterText;
        [SerializeField] private TMP_Text statusBannerText;
        [SerializeField] private Button closeButton;

        [Header("5 Blueprint Slots & Tray Cards")]
        [SerializeField] private Button[] slotButtons;
        [SerializeField] private Image[] slotBackgrounds;
        [SerializeField] private AssemblyPartDragItem[] partCards;
        [SerializeField] private RectTransform dragGhostRect;

        [Header("Bottom Controls (>= 48 dp)")]
        [SerializeField] private Button askFriendButton;
        [SerializeField] private Button blueprintStampButton;
        [SerializeField] private Button lessonFreeModeButton;

        [Header("1950 Blueprint Stamp Modal (No 'ё')")]
        [SerializeField] private GameObject stampOverlayPanel;
        [SerializeField] private TMP_Text stampBodyText;
        [SerializeField] private Button closeStampButton;

        private AssemblyPuzzleController _controller;
        private string _selectedPartId = string.Empty;
        private const float SnapRadiusPx = 110f;

        private static readonly Color32 ColorSlotEmpty = new Color32(0xD4, 0xE6, 0xF1, 0xFF);
        private static readonly Color32 ColorSlotActive = new Color32(0x2E, 0x6F, 0x95, 0xFF);
        private static readonly Color32 ColorSlotDone = new Color32(0x4F, 0x8A, 0x6B, 0xFF);

        private void Awake()
        {
            if (closeButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(closeButton.GetComponent<RectTransform>());
                closeButton.onClick.AddListener(CloseWorkbench);
            }
            if (askFriendButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(askFriendButton.GetComponent<RectTransform>());
                askFriendButton.onClick.AddListener(OnAskFriendClicked);
            }
            if (blueprintStampButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(blueprintStampButton.GetComponent<RectTransform>());
                blueprintStampButton.onClick.AddListener(OpenBlueprintStamp);
            }
            if (closeStampButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(closeStampButton.GetComponent<RectTransform>());
                closeStampButton.onClick.AddListener(CloseBlueprintStamp);
            }
            if (lessonFreeModeButton != null)
            {
                TouchInput.EnforceMinTouchSizeDp(lessonFreeModeButton.GetComponent<RectTransform>());
            }

            if (slotButtons != null)
            {
                for (int i = 0; i < slotButtons.Length; i++)
                {
                    int slotIdx = i;
                    if (slotButtons[i] != null)
                    {
                        TouchInput.EnforceMinTouchSizeDp(slotButtons[i].GetComponent<RectTransform>());
                        slotButtons[i].onClick.AddListener(() => OnSlotClicked(slotIdx));
                    }
                }
            }

            if (rootPanel != null) rootPanel.SetActive(false);
            if (stampOverlayPanel != null) stampOverlayPanel.SetActive(false);
            if (dragGhostRect != null) dragGhostRect.gameObject.SetActive(false);
        }

        public void BindController(AssemblyPuzzleController controller)
        {
            if (_controller != null)
            {
                _controller.OnAssemblyStateUpdated -= RefreshView;
                _controller.OnAssemblyFeedbackMessage -= SetStatusBanner;
            }
            _controller = controller;
            if (_controller != null)
            {
                _controller.OnAssemblyStateUpdated += RefreshView;
                _controller.OnAssemblyFeedbackMessage += SetStatusBanner;
            }
        }

        public void ConfigureUiReferences(
            GameObject root,
            TMP_Text stepText,
            TMP_Text massText,
            TMP_Text bannerText,
            Button closeBtn,
            Button[] slots,
            Image[] slotBgs,
            AssemblyPartDragItem[] cards,
            RectTransform ghost,
            Button friendBtn,
            Button stampBtn,
            Button freeBtn,
            GameObject stampModal,
            TMP_Text stampText,
            Button closeStampBtn)
        {
            rootPanel = root;
            stepHeaderText = stepText;
            massCounterText = massText;
            statusBannerText = bannerText;
            closeButton = closeBtn;
            slotButtons = slots;
            slotBackgrounds = slotBgs;
            partCards = cards;
            dragGhostRect = ghost;
            askFriendButton = friendBtn;
            blueprintStampButton = stampBtn;
            lessonFreeModeButton = freeBtn;
            stampOverlayPanel = stampModal;
            stampBodyText = stampText;
            closeStampButton = closeStampBtn;

            if (stampBodyText != null)
            {
                stampBodyText.SetText(
                    "ЧЕРТЕЖ СБОРКИ АППАРАТА «ЗАРЯ-7» (14.11.1950)\n" +
                    "Порядок монтажа на раме (40 см на 20 см): 1) укрепить хроно-сердцевину в центре; " +
                    "2) слева подключить синхронный привод; 3) поверх ядра надеть медную катушку; " +
                    "4) справа присоединить конденсаторный блок памяти; 5) сверху замкнуть кварцевый стабилизатор. " +
                    "Все пять узлов вместе весят ровно три килограмма.");
            }
        }

        public void OpenWorkbench()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            RefreshView();
        }

        public void CloseWorkbench()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        public void SelectPartFromTray(string partId)
        {
            _selectedPartId = partId;
            SetStatusBanner("Деталь выбрана! Нажми на нужный слот чертежа или перетащи деталь пальцем.");
        }

        public void BeginPartDrag(string partId, Vector2 screenPos)
        {
            _selectedPartId = partId;
            if (dragGhostRect != null)
            {
                dragGhostRect.gameObject.SetActive(true);
                dragGhostRect.position = screenPos;
            }
        }

        public void UpdatePartDrag(Vector2 screenPos)
        {
            if (dragGhostRect != null && dragGhostRect.gameObject.activeSelf)
            {
                dragGhostRect.position = screenPos;
            }
        }

        public void EndPartDrag(string partId, Vector2 screenPos)
        {
            if (dragGhostRect != null)
            {
                dragGhostRect.gameObject.SetActive(false);
            }
            if (slotButtons == null || _controller == null)
            {
                return;
            }

            int nearestSlot = -1;
            float bestDist = SnapRadiusPx;
            for (int i = 0; i < slotButtons.Length; i++)
            {
                if (slotButtons[i] == null) continue;
                float d = Vector2.Distance(screenPos, slotButtons[i].transform.position);
                if (d <= bestDist)
                {
                    bestDist = d;
                    nearestSlot = i;
                }
            }

            if (nearestSlot >= 0)
            {
                _controller.TryPlacePart(nearestSlot, partId);
                _selectedPartId = string.Empty;
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (_controller == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(_selectedPartId))
            {
                SetStatusBanner("Сначала нажми на карточку детали в нижнем лотке или перетащи её на слот.");
                return;
            }
            _controller.TryPlacePart(slotIndex, _selectedPartId);
            _selectedPartId = string.Empty;
        }

        private void OnAskFriendClicked()
        {
            if (_controller == null) return;
            var spec = _controller.GetSlotSpec(_controller.CurrentStepIndex);
            SetStatusBanner($"Подсказка Лёвы: посмотри на штамп 1950 года — сейчас нужен «{spec.displayTitle}» ({spec.massGrams} г).");
        }

        private void OpenBlueprintStamp()
        {
            if (stampOverlayPanel != null)
            {
                stampOverlayPanel.SetActive(true);
            }
        }

        private void CloseBlueprintStamp()
        {
            if (stampOverlayPanel != null)
            {
                stampOverlayPanel.SetActive(false);
            }
        }

        private void SetStatusBanner(string msg)
        {
            if (statusBannerText != null)
            {
                statusBannerText.SetText(msg);
            }
        }

        public void RefreshView()
        {
            if (_controller == null)
            {
                return;
            }

            int step = Mathf.Min(_controller.CurrentStepIndex + 1, AssemblyPuzzleController.SlotCount);
            if (stepHeaderText != null)
            {
                if (_controller.IsFullyAssembled)
                {
                    stepHeaderText.SetText("Сборка «Зари-7» завершена (5 из 5)!");
                }
                else
                {
                    stepHeaderText.SetText("Сборка «Зари-7»: шаг {0} из 5", step);
                }
            }

            if (massCounterText != null)
            {
                massCounterText.SetText("Масса узлов: {0} г / 3 000 г (3 кг)", _controller.GetInstalledMassGrams());
            }

            if (slotBackgrounds != null)
            {
                for (int i = 0; i < slotBackgrounds.Length; i++)
                {
                    if (slotBackgrounds[i] == null) continue;
                    if (_controller.IsSlotInstalled(i))
                    {
                        slotBackgrounds[i].color = ColorSlotDone;
                    }
                    else if (i == _controller.CurrentStepIndex)
                    {
                        slotBackgrounds[i].color = ColorSlotActive;
                    }
                    else
                    {
                        slotBackgrounds[i].color = ColorSlotEmpty;
                    }
                }
            }
        }
    }
}
