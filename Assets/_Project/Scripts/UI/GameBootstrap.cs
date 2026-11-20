using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;
using TimeMachine.Quests;

namespace TimeMachine.UI
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Gameplay Controllers")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private CompanionController companionController;
        [SerializeField] private TouchInput touchInput;
        [SerializeField] private EraSwitcher eraSwitcher;
        [SerializeField] private AssemblyPuzzleController assemblyPuzzleController;
        [SerializeField] private Sonya2040EscortController sonya2040Escort;
        [SerializeField] private DualReturnSynchronizer dualReturnSynchronizer;
        [SerializeField] private FreeRoamController freeRoamController;

        [Header("UI Presenters (9:16)")]
        [SerializeField] private HUDPresenter hudPresenter;
        [SerializeField] private TaskDialogPresenter taskDialogPresenter;
        [SerializeField] private JournalPresenter journalPresenter;
        [SerializeField] private AssemblyWorkbenchPresenter assemblyWorkbenchPresenter;
        [SerializeField] private TimeJumpTransitionPresenter timeJumpTransitionPresenter;
        [SerializeField] private EpilogueSummaryPresenter epilogueSummaryPresenter;

        private ContentDatabase _database;
        private SaveSystem _saveSystem;
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private TaskEngine _taskEngine;
        private TaskGenerator _taskGenerator;
        private LoreJournal _loreJournal;
        private QuestRunner _questRunner;

        private readonly Dictionary<string, string> _localizationRu = new Dictionary<string, string>(512);

        public IReadOnlyDictionary<string, string> LocalizationRu => _localizationRu;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            LoadLocalizationCsv();

            _database = new ContentDatabase();
            _database.LoadFromResources();

            _saveSystem = new SaveSystem();
            SaveDataV1 loadedData = _saveSystem.LoadOrCreateDefault();
            _gameState = new GameState(loadedData);
            _inventorySystem = new InventorySystem(_gameState);

            _taskEngine = new TaskEngine(_gameState);
            _taskGenerator = new TaskGenerator();
            _loreJournal = new LoreJournal(_database, _gameState);
            _questRunner = new QuestRunner(_database, _gameState, _inventorySystem, _loreJournal, _saveSystem);
            _questRunner.Initialize();

            BindAllSystems();
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("MUS-02", 1.0f);
            }
            if (hudPresenter != null && _gameState != null && _gameState.Data != null)
            {
                hudPresenter.SetXpValue(_gameState.Data.totalXp);
                if (_questRunner != null)
                {
                    hudPresenter.SetQuestObjective(_questRunner.GetCurrentObjectiveText());
                }
            }
            if (_gameState != null && _gameState.Data != null && _gameState.Data.isStoryCompleted && freeRoamController != null)
            {
                freeRoamController.ActivateFreeRoam();
            }
        }

        public void SetSceneControllers(
            PlayerController player,
            ThirdPersonCamera cam,
            CompanionController companion,
            TouchInput input,
            EraSwitcher era,
            AssemblyPuzzleController assembly,
            Sonya2040EscortController escort,
            DualReturnSynchronizer dualReturn,
            FreeRoamController freeRoam)
        {
            playerController = player;
            thirdPersonCamera = cam;
            companionController = companion;
            touchInput = input;
            eraSwitcher = era;
            assemblyPuzzleController = assembly;
            sonya2040Escort = escort;
            dualReturnSynchronizer = dualReturn;
            freeRoamController = freeRoam;
        }

        public void SetUiPresenters(
            HUDPresenter hud,
            TaskDialogPresenter taskDialog,
            JournalPresenter journal,
            AssemblyWorkbenchPresenter workbench,
            TimeJumpTransitionPresenter jumpTransition,
            EpilogueSummaryPresenter epilogue)
        {
            hudPresenter = hud;
            taskDialogPresenter = taskDialog;
            journalPresenter = journal;
            assemblyWorkbenchPresenter = workbench;
            timeJumpTransitionPresenter = jumpTransition;
            epilogueSummaryPresenter = epilogue;
        }

        private void BindAllSystems()
        {
            if (eraSwitcher != null)
            {
                eraSwitcher.BindSystems(_gameState, _inventorySystem, _saveSystem);
            }
            if (assemblyPuzzleController != null)
            {
                assemblyPuzzleController.BindSystems(_gameState, _inventorySystem, _saveSystem, _questRunner);
            }
            if (dualReturnSynchronizer != null)
            {
                dualReturnSynchronizer.BindSystems(eraSwitcher, _inventorySystem);
            }
            if (freeRoamController != null)
            {
                freeRoamController.BindSystems(_gameState, _database, _loreJournal, _taskGenerator, _saveSystem);
                if (playerController != null)
                {
                    freeRoamController.SetPlayerTransform(playerController.transform);
                }
            }
            if (hudPresenter != null && playerController != null)
            {
                hudPresenter.BindWorldActors(playerController.transform, companionController);
            }
            if (taskDialogPresenter != null)
            {
                taskDialogPresenter.BindSystems(_database, _taskEngine, _gameState, _saveSystem);
            }
            if (journalPresenter != null)
            {
                journalPresenter.BindSystems(_database, _gameState, _inventorySystem, _loreJournal, _saveSystem);
            }
            if (assemblyWorkbenchPresenter != null)
            {
                assemblyWorkbenchPresenter.BindController(assemblyPuzzleController);
            }
            if (timeJumpTransitionPresenter != null)
            {
                timeJumpTransitionPresenter.BindEraSwitcher(eraSwitcher);
            }
            if (epilogueSummaryPresenter != null)
            {
                epilogueSummaryPresenter.BindSystems(_gameState, _loreJournal, freeRoamController, journalPresenter, _saveSystem);
            }
        }

        private void LoadLocalizationCsv()
        {
            TextAsset csv = Resources.Load<TextAsset>("Localization/strings_ru");
            if (csv == null || string.IsNullOrEmpty(csv.text))
            {
                return;
            }

            string[] lines = csv.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int firstSep = line.IndexOf(';');
                if (firstSep <= 0)
                {
                    continue;
                }
                string key = line.Substring(0, firstSep).Trim();
                string[] cols = line.Split(';');
                if (cols.Length >= 5)
                {
                    _localizationRu[key] = cols[4].Trim();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _saveSystem != null && _gameState != null && _gameState.Data != null)
            {
                _saveSystem.Save(_gameState.Data);
            }
        }

        private void OnApplicationQuit()
        {
            if (_saveSystem != null && _gameState != null && _gameState.Data != null)
            {
                _saveSystem.Save(_gameState.Data);
            }
        }
    }
}
