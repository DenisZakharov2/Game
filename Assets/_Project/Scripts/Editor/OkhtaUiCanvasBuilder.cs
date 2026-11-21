#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TimeMachine.Core;
using TimeMachine.Gameplay;
using TimeMachine.UI;

namespace TimeMachine.Editor
{
    public static class OkhtaUiCanvasBuilder
    {
        private static readonly Color32 PaperBg = new Color32(0xF7, 0xF1, 0xE3, 0xFF);
        private static readonly Color32 BlueprintBg = new Color32(0xE6, 0xEE, 0xF2, 0xFF);
        private static readonly Color32 Note1950Bg = new Color32(0xED, 0xE0, 0xC8, 0xFF);
        private static readonly Color32 InkDark = new Color32(0x23, 0x2A, 0x34, 0xFF);
        private static readonly Color32 BtnAccentRed = new Color32(0xC9, 0x3B, 0x2B, 0xFF);
        private static readonly Color32 BtnBlue = new Color32(0x4A, 0x6B, 0x82, 0xFF);
        private static readonly Color32 BtnSage = new Color32(0x3D, 0x7A, 0x52, 0xFF);
        private static readonly Color32 WhiteText = new Color32(0xFF, 0xFD, 0xF9, 0xFF);
        private static readonly Color32 GoldFade = new Color32(0xF6, 0xDF, 0x9E, 0xFF);

        [MenuItem("TimeMachine/Build Portrait UI Canvas (9x16)")]
        public static void BuildPortraitUiCanvas()
        {
            EventSystem existingEs = Object.FindAnyObjectByType<EventSystem>();
            if (existingEs == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }

            GameObject oldCanvas = GameObject.Find("Canvas_Main_9x16");
            if (oldCanvas != null)
            {
                Object.DestroyImmediate(oldCanvas);
            }

            var canvasGo = new GameObject("Canvas_Main_9x16");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // TouchZones
            RectTransform touchZones = CreateRect("TouchZones", canvasGo.transform);
            StretchFull(touchZones);
            var touchInput = touchZones.gameObject.AddComponent<TouchInput>();
            RectTransform joyBase = CreateRect("JoystickBase", touchZones);
            joyBase.sizeDelta = new Vector2(220f, 220f);
            var joyBaseImg = joyBase.gameObject.AddComponent<Image>();
            joyBaseImg.color = new Color32(0x23, 0x2A, 0x34, 0x44);
            joyBaseImg.raycastTarget = false;

            RectTransform joyHandle = CreateRect("JoystickHandle", joyBase);
            joyHandle.sizeDelta = new Vector2(96f, 96f);
            var joyHandleImg = joyHandle.gameObject.AddComponent<Image>();
            joyHandleImg.color = new Color32(0xC9, 0x3B, 0x2B, 0xAA);
            joyHandleImg.raycastTarget = false;
            touchInput.ConfigureJoystickRefs(joyBase, joyHandle);

            // SafeAreaRoot
            RectTransform safeAreaRoot = CreateRect("SafeAreaRoot", canvasGo.transform);
            StretchFull(safeAreaRoot);
            safeAreaRoot.gameObject.AddComponent<SafeAreaFitter>();

            // 1. UI_HUD_9x16
            RectTransform hudRoot = CreateRect("UI_HUD_9x16", safeAreaRoot);
            StretchFull(hudRoot);
            var hudPresenter = hudRoot.gameObject.AddComponent<HUDPresenter>();

            RectTransform topBar = CreatePanel("TopPanel", hudRoot, PaperBg);
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.anchoredPosition = new Vector2(0f, -16f);
            topBar.sizeDelta = new Vector2(-48f, 156f);

            TMP_Text objectiveText = CreateLabel("QuestObjectiveText", topBar, "Глава 1: Осмотреть провал у горки-бомбоубежища", 30f, InkDark);
            objectiveText.rectTransform.anchorMin = new Vector2(0f, 0f);
            objectiveText.rectTransform.anchorMax = new Vector2(0.62f, 1f);
            objectiveText.rectTransform.offsetMin = new Vector2(20f, 12f);
            objectiveText.rectTransform.offsetMax = new Vector2(-8f, -12f);

            TMP_Text xpText = CreateLabel("XpValueText", topBar, "0 XP", 32f, BtnAccentRed);
            xpText.alignment = TextAlignmentOptions.Center;
            xpText.rectTransform.anchorMin = new Vector2(0.63f, 0f);
            xpText.rectTransform.anchorMax = new Vector2(0.83f, 1f);
            xpText.rectTransform.offsetMin = Vector2.zero;
            xpText.rectTransform.offsetMax = Vector2.zero;

            Button pauseBtn = CreateButton("Btn_PauseSettings", topBar, "Пауза", BtnBlue, WhiteText, new Vector2(140f, 136f));
            pauseBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
            pauseBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
            pauseBtn.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
            pauseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

            RectTransform compassRoot = CreatePanel("CompassWidget", hudRoot, Note1950Bg);
            compassRoot.anchorMin = new Vector2(0f, 1f);
            compassRoot.anchorMax = new Vector2(0f, 1f);
            compassRoot.pivot = new Vector2(0f, 1f);
            compassRoot.anchoredPosition = new Vector2(28f, -192f);
            compassRoot.sizeDelta = new Vector2(160f, 160f);
            RectTransform compassNeedle = CreateRect("CompassNeedle", compassRoot);
            compassNeedle.sizeDelta = new Vector2(14f, 84f);
            compassNeedle.gameObject.AddComponent<Image>().color = BtnAccentRed;
            TMP_Text compassDist = CreateLabel("CompassDistanceText", compassRoot, "150 м", 26f, InkDark);
            compassDist.alignment = TextAlignmentOptions.Bottom;
            StretchFull(compassDist.rectTransform);

            RectTransform speechRoot = CreatePanel("DialogueBubble", hudRoot, PaperBg);
            speechRoot.anchorMin = new Vector2(0.5f, 0f);
            speechRoot.anchorMax = new Vector2(0.5f, 0f);
            speechRoot.pivot = new Vector2(0.5f, 0f);
            speechRoot.anchoredPosition = new Vector2(0f, 540f);
            speechRoot.sizeDelta = new Vector2(960f, 190f);
            TMP_Text speakerText = CreateLabel("SpeakerNameText", speechRoot, "Саша", 28f, BtnAccentRed);
            speakerText.rectTransform.anchorMin = new Vector2(0f, 0.65f);
            speakerText.rectTransform.anchorMax = new Vector2(1f, 1f);
            speakerText.rectTransform.offsetMin = new Vector2(24f, 4f);
            speakerText.rectTransform.offsetMax = new Vector2(-24f, -8f);
            TMP_Text speechMsgText = CreateLabel("SpeechMessageText", speechRoot, "Готовы исследовать квартал!", 30f, InkDark);
            speechMsgText.rectTransform.anchorMin = new Vector2(0f, 0f);
            speechMsgText.rectTransform.anchorMax = new Vector2(1f, 0.65f);
            speechMsgText.rectTransform.offsetMin = new Vector2(24f, 12f);
            speechMsgText.rectTransform.offsetMax = new Vector2(-24f, -4f);

            Button actBtn = CreateButton("Btn_Interact", hudRoot, "Осмотреть", BtnAccentRed, WhiteText, new Vector2(360f, 144f));
            RectTransform actRect = actBtn.GetComponent<RectTransform>();
            actRect.anchorMin = new Vector2(1f, 0f);
            actRect.anchorMax = new Vector2(1f, 0f);
            actRect.pivot = new Vector2(1f, 0f);
            actRect.anchoredPosition = new Vector2(-32f, 380f);
            TMP_Text actPrompt = actBtn.GetComponentInChildren<TMP_Text>();

            Button friendBtn = CreateButton("Btn_CallFriend", hudRoot, "Друг", BtnBlue, WhiteText, new Vector2(160f, 144f));
            RectTransform friendRect = friendBtn.GetComponent<RectTransform>();
            friendRect.anchorMin = new Vector2(1f, 0f);
            friendRect.anchorMax = new Vector2(1f, 0f);
            friendRect.pivot = new Vector2(1f, 0f);
            friendRect.anchoredPosition = new Vector2(-32f, 204f);

            Button journalBtn = CreateButton("Btn_Journal", hudRoot, "Дневник", BtnSage, WhiteText, new Vector2(180f, 144f));
            RectTransform jrnlRect = journalBtn.GetComponent<RectTransform>();
            jrnlRect.anchorMin = new Vector2(1f, 0f);
            jrnlRect.anchorMax = new Vector2(1f, 0f);
            jrnlRect.pivot = new Vector2(1f, 0f);
            jrnlRect.anchoredPosition = new Vector2(-216f, 204f);

            hudPresenter.ConfigureUiReferences(
                objectiveText, xpText, pauseBtn, compassRoot, compassNeedle, compassDist,
                speechRoot.gameObject, speakerText, speechMsgText, actBtn, actPrompt, friendBtn, journalBtn);

            // 2. UI_TaskDialog
            RectTransform taskRoot = CreatePanel("UI_TaskDialog", safeAreaRoot, PaperBg);
            taskRoot.sizeDelta = new Vector2(980f, 1920f);
            var taskPresenter = taskRoot.gameObject.AddComponent<TaskDialogPresenter>();

            TMP_Text topicText = CreateLabel("TopicHeaderText", taskRoot, "Математика • 4 класс ФГОС", 34f, BtnBlue);
            topicText.rectTransform.anchorMin = new Vector2(0f, 1f);
            topicText.rectTransform.anchorMax = new Vector2(0.82f, 1f);
            topicText.rectTransform.pivot = new Vector2(0f, 1f);
            topicText.rectTransform.anchoredPosition = new Vector2(32f, -24f);
            topicText.rectTransform.sizeDelta = new Vector2(0f, 136f);

            Button closeTaskBtn = CreateButton("Btn_CloseTask", taskRoot, "✕", BtnAccentRed, WhiteText, new Vector2(136f, 136f));
            closeTaskBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
            closeTaskBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            closeTaskBtn.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            closeTaskBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, -24f);

            TMP_Text qBodyText = CreateLabel("QuestionBodyText", taskRoot, "Текст задачи", 36f, InkDark);
            qBodyText.rectTransform.anchorMin = new Vector2(0f, 0.56f);
            qBodyText.rectTransform.anchorMax = new Vector2(1f, 0.90f);
            qBodyText.rectTransform.offsetMin = new Vector2(36f, 12f);
            qBodyText.rectTransform.offsetMax = new Vector2(-36f, -12f);

            TMP_InputField inputField = CreateInputField("AnswerInputField", taskRoot, new Vector2(900f, 140f), new Vector2(0f, -60f));

            Button[] optBtns = new Button[6];
            TMP_Text[] optLabels = new TMP_Text[6];
            for (int i = 0; i < 6; i++)
            {
                int row = i / 2;
                int col = i % 2;
                float xPos = col == 0 ? -230f : 230f;
                float yPos = -240f - row * 156f;
                Button ob = CreateButton($"Btn_Option_{i + 1}", taskRoot, $"Вариант {i + 1}", BtnBlue, WhiteText, new Vector2(430f, 136f));
                ob.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
                optBtns[i] = ob;
                optLabels[i] = ob.GetComponentInChildren<TMP_Text>();
            }

            TMP_Text hintText = CreateLabel("HintDisplayText", taskRoot, "Подсказки 1–3 уровня", 30f, InkDark);
            hintText.rectTransform.anchoredPosition = new Vector2(0f, -680f);
            hintText.rectTransform.sizeDelta = new Vector2(900f, 140f);

            TMP_Text fbText = CreateLabel("FeedbackText", taskRoot, "", 30f, BtnSage);
            fbText.rectTransform.anchoredPosition = new Vector2(0f, -540f);
            fbText.rectTransform.sizeDelta = new Vector2(900f, 100f);

            Button h1Btn = CreateButton("Btn_Hint1", taskRoot, "Друг", BtnBlue, WhiteText, new Vector2(280f, 136f));
            h1Btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-310f, -820f);
            Button h2Btn = CreateButton("Btn_Hint2", taskRoot, "Правило", BtnBlue, WhiteText, new Vector2(280f, 136f));
            h2Btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -820f);
            Button h3Btn = CreateButton("Btn_Hint3", taskRoot, "Соня-2040", BtnBlue, WhiteText, new Vector2(280f, 136f));
            h3Btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(310f, -820f);

            Button freeBtn = CreateButton("Btn_LessonFree", taskRoot, "Без урока", Note1950Bg, InkDark, new Vector2(360f, 140f));
            freeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-240f, -980f);
            Button subBtn = CreateButton("Btn_Submit", taskRoot, "Проверить", BtnSage, WhiteText, new Vector2(460f, 140f));
            subBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(210f, -980f);

            taskPresenter.ConfigureUiReferences(
                taskRoot.gameObject, topicText, qBodyText, hintText, fbText, inputField,
                optBtns, optLabels, closeTaskBtn, subBtn, h1Btn, h2Btn, h3Btn, freeBtn);

            // 3. UI_SonyJournal
            RectTransform journalRoot = CreatePanel("UI_SonyJournal", safeAreaRoot, PaperBg);
            journalRoot.sizeDelta = new Vector2(980f, 1960f);
            var journalPresenter = journalRoot.gameObject.AddComponent<JournalPresenter>();

            Button tabLoreBtn = CreateButton("Tab_LoreHistory", journalRoot, "История", BtnBlue, WhiteText, new Vector2(240f, 136f));
            tabLoreBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-340f, 880f);
            Button tabSkillsBtn = CreateButton("Tab_Skills", journalRoot, "Навыки", BtnBlue, WhiteText, new Vector2(240f, 136f));
            tabSkillsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80f, 880f);
            Button tabParentBtn = CreateButton("Tab_ParentReport", journalRoot, "Родителям", BtnSage, WhiteText, new Vector2(260f, 136f));
            tabParentBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, 880f);
            Button closeJrnlBtn = CreateButton("Btn_CloseJournal", journalRoot, "✕", BtnAccentRed, WhiteText, new Vector2(136f, 136f));
            closeJrnlBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(410f, 880f);

            TMP_Text jrnlBody = CreateLabel("JournalBodyText", journalRoot, "Дневник Сони", 32f, InkDark);
            jrnlBody.rectTransform.sizeDelta = new Vector2(900f, 1520f);
            jrnlBody.rectTransform.anchoredPosition = new Vector2(0f, -40f);

            RectTransform note1950Sub = CreatePanel("SubPanel_Note1950", journalRoot, Note1950Bg);
            note1950Sub.sizeDelta = new Vector2(900f, 1400f);
            note1950Sub.gameObject.SetActive(false);

            RectTransform passportSub = CreatePanel("SubPanel_PlacePassport", journalRoot, BlueprintBg);
            passportSub.sizeDelta = new Vector2(900f, 400f);
            passportSub.anchoredPosition = new Vector2(0f, -660f);
            passportSub.gameObject.SetActive(false);

            journalPresenter.ConfigureUiReferences(
                journalRoot.gameObject, jrnlBody, tabLoreBtn, tabSkillsBtn, tabParentBtn, closeJrnlBtn,
                note1950Sub.gameObject, passportSub.gameObject);

            // 4. UI_AssemblyWorkbench
            RectTransform wbRoot = CreatePanel("UI_AssemblyWorkbench", safeAreaRoot, BlueprintBg);
            wbRoot.sizeDelta = new Vector2(1000f, 2160f);
            var wbPresenter = wbRoot.gameObject.AddComponent<AssemblyWorkbenchPresenter>();

            TMP_Text wbStep = CreateLabel("StepHeaderText", wbRoot, "Сборка «Зари-7»: шаг 1 из 5", 38f, InkDark);
            wbStep.rectTransform.anchoredPosition = new Vector2(-70f, 980f);
            wbStep.rectTransform.sizeDelta = new Vector2(760f, 80f);

            TMP_Text wbMass = CreateLabel("MassCounterText", wbRoot, "Масса узлов: 0 г / 3 000 г (3 кг)", 30f, BtnBlue);
            wbMass.rectTransform.anchoredPosition = new Vector2(-70f, 900f);
            wbMass.rectTransform.sizeDelta = new Vector2(760f, 64f);

            Button wbClose = CreateButton("Btn_CloseWorkbench", wbRoot, "✕", BtnAccentRed, WhiteText, new Vector2(136f, 136f));
            wbClose.GetComponent<RectTransform>().anchoredPosition = new Vector2(400f, 940f);

            Button[] wbSlots = new Button[5];
            Image[] wbSlotBgs = new Image[5];
            Vector2[] slotCoords = new Vector2[]
            {
                new Vector2(0f, 380f),
                new Vector2(-280f, 380f),
                new Vector2(0f, 160f),
                new Vector2(280f, 380f),
                new Vector2(0f, 600f)
            };
            for (int i = 0; i < 5; i++)
            {
                Button sb = CreateButton($"Slot_0{i + 1}", wbRoot, $"SLOT-0{i + 1}", Note1950Bg, InkDark, new Vector2(210f, 180f));
                sb.GetComponent<RectTransform>().anchoredPosition = slotCoords[i];
                wbSlots[i] = sb;
                wbSlotBgs[i] = sb.GetComponent<Image>();
            }

            TMP_Text wbBanner = CreateLabel("StatusMessageText", wbRoot, "Выбери деталь в лотке и нажми на слот чертежа.", 30f, InkDark);
            wbBanner.rectTransform.anchoredPosition = new Vector2(0f, -40f);
            wbBanner.rectTransform.sizeDelta = new Vector2(920f, 120f);

            string[] partIds = new[]
            {
                GameConstants.ItemIds.PartChronoCore,
                GameConstants.ItemIds.PartMicrowaveMotor,
                GameConstants.ItemIds.PartMagneticCoil,
                GameConstants.ItemIds.PartCapacitorBlock,
                GameConstants.ItemIds.PartQuartzStabilizer
            };
            string[] partNames = new[] { "Сердцевина\n600 г", "Мотор СВЧ\n500 г", "Катушка\n650 г", "Конденсатор\n400 г", "Стабилизатор\n850 г" };
            AssemblyPartDragItem[] dragItems = new AssemblyPartDragItem[5];
            for (int i = 0; i < 5; i++)
            {
                RectTransform card = CreatePanel($"Card_Part_{i + 1}", wbRoot, PaperBg);
                card.sizeDelta = new Vector2(180f, 200f);
                card.anchoredPosition = new Vector2(-380f + i * 190f, -260f);
                CreateLabel("Label", card, partNames[i], 24f, InkDark);
                var drag = card.gameObject.AddComponent<AssemblyPartDragItem>();
                drag.Configure(partIds[i], wbPresenter);
                dragItems[i] = drag;
            }

            RectTransform ghostRect = CreatePanel("DragGhostIcon", wbRoot, BtnSage);
            ghostRect.sizeDelta = new Vector2(150f, 150f);

            Button wbFriend = CreateButton("Btn_AskFriend", wbRoot, "Друг", BtnBlue, WhiteText, new Vector2(280f, 136f));
            wbFriend.GetComponent<RectTransform>().anchoredPosition = new Vector2(-310f, -480f);
            Button wbStamp = CreateButton("Btn_BlueprintStamp", wbRoot, "Штамп 1950", BtnBlue, WhiteText, new Vector2(300f, 136f));
            wbStamp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -480f);
            Button wbFree = CreateButton("Btn_LessonFreeWb", wbRoot, "Без урока", Note1950Bg, InkDark, new Vector2(280f, 136f));
            wbFree.GetComponent<RectTransform>().anchoredPosition = new Vector2(310f, -480f);

            RectTransform stampModal = CreatePanel("Stamp1950Overlay", wbRoot, Note1950Bg);
            stampModal.sizeDelta = new Vector2(880f, 900f);
            TMP_Text stampText = CreateLabel("StampBodyText", stampModal, "", 32f, InkDark);
            stampText.rectTransform.sizeDelta = new Vector2(800f, 640f);
            Button closeStamp = CreateButton("Btn_CloseStamp", stampModal, "Понятно", BtnSage, WhiteText, new Vector2(320f, 136f));
            closeStamp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -340f);

            wbPresenter.ConfigureUiReferences(
                wbRoot.gameObject, wbStep, wbMass, wbBanner, wbClose, wbSlots, wbSlotBgs, dragItems, ghostRect,
                wbFriend, wbStamp, wbFree, stampModal.gameObject, stampText, closeStamp);

            // 5. UI_TimeJumpTransition
            RectTransform jumpRoot = CreateRect("UI_TimeJumpTransition", safeAreaRoot);
            StretchFull(jumpRoot);
            var jumpPresenter = jumpRoot.gameObject.AddComponent<TimeJumpTransitionPresenter>();

            RectTransform confirmPanel = CreatePanel("ConfirmationModalPanel", jumpRoot, PaperBg);
            confirmPanel.sizeDelta = new Vector2(920f, 1080f);
            TMP_Text jHeader = CreateLabel("HeaderTitleText", confirmPanel, "Калибровка прыжка", 40f, InkDark);
            jHeader.rectTransform.anchoredPosition = new Vector2(0f, 380f);
            TMP_Text jDate = CreateLabel("TargetDateText", confirmPanel, "14 ноября 1950 года", 34f, BtnAccentRed);
            jDate.rectTransform.anchoredPosition = new Vector2(0f, 240f);
            TMP_Text jRoles = CreateLabel("RolesSummaryText", confirmPanel, "", 30f, InkDark);
            jRoles.rectTransform.sizeDelta = new Vector2(820f, 360f);
            jRoles.rectTransform.anchoredPosition = new Vector2(0f, 20f);
            Button jConfirm = CreateButton("Btn_ConfirmJump", confirmPanel, "Запустить «Зарю-7»", BtnSage, WhiteText, new Vector2(440f, 140f));
            jConfirm.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -260f);
            Button jCancel = CreateButton("Btn_CancelJump", confirmPanel, "Отмена", BtnBlue, WhiteText, new Vector2(320f, 136f));
            jCancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -430f);

            RectTransform overlayRect = CreatePanel("WatercolorTransitionOverlay", jumpRoot, GoldFade);
            StretchFull(overlayRect);
            var overlayGroup = overlayRect.gameObject.AddComponent<CanvasGroup>();
            TMP_Text subTitle = CreateLabel("TransitionSubtitleText", overlayRect, "2027 → 14 ноября 1950", 46f, InkDark);
            subTitle.alignment = TextAlignmentOptions.Center;
            subTitle.rectTransform.anchoredPosition = new Vector2(0f, 120f);
            TMP_Text locHint = CreateLabel("TransitionLocationHintText", overlayRect, "Большая Охта", 32f, InkDark);
            locHint.alignment = TextAlignmentOptions.Center;
            locHint.rectTransform.anchoredPosition = new Vector2(0f, 20f);

            jumpPresenter.ConfigureUiReferences(
                confirmPanel.gameObject, jHeader, jDate, jRoles, jConfirm, jCancel, overlayGroup, subTitle, locHint);

            // 6. UI_EpilogueSummary
            RectTransform epilogueRoot = CreatePanel("UI_EpilogueSummary", safeAreaRoot, PaperBg);
            epilogueRoot.sizeDelta = new Vector2(960f, 1880f);
            var epiloguePresenter = epilogueRoot.gameObject.AddComponent<EpilogueSummaryPresenter>();

            TMP_Text epTitle = CreateLabel("EpilogueTitleText", epilogueRoot, "Приключение завершено!", 42f, BtnAccentRed);
            epTitle.rectTransform.anchoredPosition = new Vector2(0f, 740f);
            TMP_Text epStory = CreateLabel("EpilogueStoryText", epilogueRoot, "", 32f, InkDark);
            epStory.rectTransform.sizeDelta = new Vector2(860f, 420f);
            epStory.rectTransform.anchoredPosition = new Vector2(0f, 420f);
            TMP_Text epStats = CreateLabel("StatsSummaryText", epilogueRoot, "", 32f, InkDark);
            epStats.rectTransform.sizeDelta = new Vector2(860f, 440f);
            epStats.rectTransform.anchoredPosition = new Vector2(0f, -40f);

            Button btnFreeRoam = CreateButton("Btn_EnterFreeRoam", epilogueRoot, "Свободный режим", BtnSage, WhiteText, new Vector2(680f, 144f));
            btnFreeRoam.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -420f);
            Button btnPassport = CreateButton("Btn_OpenPassportReport", epilogueRoot, "Паспорт места и отчёт", BtnBlue, WhiteText, new Vector2(680f, 144f));
            btnPassport.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -590f);
            Button btnMenu = CreateButton("Btn_MainMenu", epilogueRoot, "В главное меню", Note1950Bg, InkDark, new Vector2(680f, 144f));
            btnMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -760f);

            epiloguePresenter.ConfigureUiReferences(
                epilogueRoot.gameObject, epTitle, epStory, epStats, btnFreeRoam, btnPassport, btnMenu);

            // 7. UI_PauseAndSettings
            RectTransform settingsRoot = CreatePanel("UI_PauseAndSettings", safeAreaRoot, PaperBg);
            settingsRoot.sizeDelta = new Vector2(920f, 1560f);
            settingsRoot.gameObject.SetActive(false);
            TMP_Text settingsTitle = CreateLabel("SettingsTitle", settingsRoot, "Настройки и отдых (100% офлайн)", 38f, InkDark);
            settingsTitle.rectTransform.anchoredPosition = new Vector2(0f, 620f);
            Button resumeBtn = CreateButton("Btn_ResumeGame", settingsRoot, "Продолжить игру", BtnSage, WhiteText, new Vector2(560f, 144f));
            resumeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -540f);
            resumeBtn.onClick.AddListener(() => settingsRoot.gameObject.SetActive(false));

            hudPresenter.SetLinkedPanels(journalRoot.gameObject, settingsRoot.gameObject);

            // Wire GameBootstrap if present
            GameBootstrap bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                GameObject managers = GameObject.Find("--- MANAGERS ---") ?? new GameObject("--- MANAGERS ---");
                bootstrap = managers.AddComponent<GameBootstrap>();
            }
            bootstrap.SetUiPresenters(hudPresenter, taskPresenter, journalPresenter, wbPresenter, jumpPresenter, epiloguePresenter);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[TimeMachine] Портретный интерфейс Canvas_Main_9x16 (1080x2400, кнопки >= 48 dp) успешно собран!");
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color32 color)
        {
            RectTransform rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return rt;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text, float fontSize, Color32 color)
        {
            RectTransform rt = CreateRect(name, parent);
            rt.sizeDelta = new Vector2(840f, 100f);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color32 bgColor, Color32 textColor, Vector2 size)
        {
            RectTransform rt = CreatePanel(name, parent, bgColor);
            rt.sizeDelta = new Vector2(Mathf.Max(132f, size.x), Mathf.Max(132f, size.y));
            var btn = rt.gameObject.AddComponent<Button>();
            TMP_Text tmp = CreateLabel("Label", rt, label, 30f, textColor);
            tmp.alignment = TextAlignmentOptions.Center;
            StretchFull(tmp.rectTransform);
            return btn;
        }

        private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 size, Vector2 anchoredPos)
        {
            RectTransform rt = CreatePanel(name, parent, WhiteText);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            RectTransform textArea = CreateRect("TextArea", rt);
            StretchFull(textArea);
            textArea.offsetMin = new Vector2(20f, 10f);
            textArea.offsetMax = new Vector2(-20f, -10f);

            TMP_Text textComp = CreateLabel("Text", textArea, "", 36f, InkDark);
            StretchFull(textComp.rectTransform);

            var field = rt.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = textArea;
            field.textComponent = textComp;
            return field;
        }
    }
}
#endif
