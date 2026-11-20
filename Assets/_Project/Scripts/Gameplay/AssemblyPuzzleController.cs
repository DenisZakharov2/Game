using System;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Quests;

namespace TimeMachine.Gameplay
{
    [Serializable]
    public struct AssemblySlotSpec
    {
        public string slotId;
        public string requiredPartId;
        public string displayTitle;
        public int massGrams;
        public string followUpTaskId;
    }

    public class AssemblyPuzzleController : MonoBehaviour
    {
        public const int SlotCount = 5;

        private readonly AssemblySlotSpec[] _slots = new AssemblySlotSpec[SlotCount];
        private readonly bool[] _slotCompleted = new bool[SlotCount];

        private GameState _gameState;
        private InventorySystem _inventory;
        private SaveSystem _saveSystem;
        private QuestRunner _questRunner;

        private int _currentStepIndex;
        private bool _waitingForTaskId;
        private string _pendingTaskId = string.Empty;

        public event Action OnAssemblyStateUpdated;
        public event Action<string> OnAssemblyFeedbackMessage;
        public event Action OnAssemblyFullyCompleted;

        public int CurrentStepIndex => _currentStepIndex;
        public bool IsFullyAssembled => _currentStepIndex >= SlotCount;
        public bool IsWaitingForTask => _waitingForTaskId;

        private void Awake()
        {
            InitializeBlueprintSlots();
        }

        private void OnEnable()
        {
            GameEventBus.OnTaskCompleted += HandleTaskCompleted;
        }

        private void OnDisable()
        {
            GameEventBus.OnTaskCompleted -= HandleTaskCompleted;
        }

        public void BindSystems(GameState state, InventorySystem inventory, SaveSystem saveSystem, QuestRunner questRunner)
        {
            _gameState = state;
            _inventory = inventory;
            _saveSystem = saveSystem;
            _questRunner = questRunner;
        }

        private void InitializeBlueprintSlots()
        {
            _slots[0] = new AssemblySlotSpec
            {
                slotId = "SLOT-01",
                requiredPartId = GameConstants.ItemIds.PartChronoCore,
                displayTitle = "Хроно-сердцевина «Зари-7»",
                massGrams = 600,
                followUpTaskId = string.Empty
            };
            _slots[1] = new AssemblySlotSpec
            {
                slotId = "SLOT-02",
                requiredPartId = GameConstants.ItemIds.PartMicrowaveMotor,
                displayTitle = "Синхронный двигатель СВЧ",
                massGrams = 500,
                followUpTaskId = "T-MATH-11"
            };
            _slots[2] = new AssemblySlotSpec
            {
                slotId = "SLOT-03",
                requiredPartId = GameConstants.ItemIds.PartMagneticCoil,
                displayTitle = "Магнитная катушка индукции",
                massGrams = 650,
                followUpTaskId = string.Empty
            };
            _slots[3] = new AssemblySlotSpec
            {
                slotId = "SLOT-04",
                requiredPartId = GameConstants.ItemIds.PartCapacitorBlock,
                displayTitle = "Конденсаторный блок памяти",
                massGrams = 400,
                followUpTaskId = "T-RUS-11"
            };
            _slots[4] = new AssemblySlotSpec
            {
                slotId = "SLOT-05",
                requiredPartId = GameConstants.ItemIds.PartQuartzStabilizer,
                displayTitle = "Кварцевый стабилизатор времени",
                massGrams = 850,
                followUpTaskId = string.Empty
            };
        }

        public AssemblySlotSpec GetSlotSpec(int index)
        {
            return _slots[Mathf.Clamp(index, 0, SlotCount - 1)];
        }

        public bool IsSlotInstalled(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                return false;
            }
            return _slotCompleted[index];
        }

        public int GetInstalledMassGrams()
        {
            int sum = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slotCompleted[i])
                {
                    sum += _slots[i].massGrams;
                }
            }
            return sum;
        }

        public bool TryPlacePart(int targetSlotIndex, string partId)
        {
            if (IsFullyAssembled)
            {
                return true;
            }

            if (_waitingForTaskId)
            {
                OnAssemblyFeedbackMessage?.Invoke("Сначала решим контрольную задачу для этого узла!");
                GameEventBus.RaiseTaskRequested(_pendingTaskId);
                return false;
            }

            if (targetSlotIndex != _currentStepIndex)
            {
                AssemblySlotSpec expected = _slots[_currentStepIndex];
                string msg = $"По синьке 1950 года сейчас шаг {_currentStepIndex + 1}: сначала установим узел «{expected.displayTitle}» в {expected.slotId}.";
                OnAssemblyFeedbackMessage?.Invoke(msg);
                GameEventBus.RaiseCompanionSpeak(GameConstants.CharacterIds.Kristina, msg);
                return false;
            }

            AssemblySlotSpec current = _slots[_currentStepIndex];
            if (!string.Equals(current.requiredPartId, partId, StringComparison.Ordinal))
            {
                string msg = $"Эта деталь для другого шага. Для {current.slotId} нужен узел «{current.displayTitle}».";
                OnAssemblyFeedbackMessage?.Invoke(msg);
                GameEventBus.RaiseCompanionSpeak(GameConstants.CharacterIds.Sasha, msg);
                return false;
            }

            _slotCompleted[_currentStepIndex] = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-49");
            }

            if (!string.IsNullOrEmpty(current.followUpTaskId))
            {
                _waitingForTaskId = true;
                _pendingTaskId = current.followUpTaskId;
                OnAssemblyFeedbackMessage?.Invoke($"Узел «{current.displayTitle}» на месте! Проверим расчёт по чертежу.");
                OnAssemblyStateUpdated?.Invoke();
                GameEventBus.RaiseTaskRequested(_pendingTaskId);
                return true;
            }

            AdvanceStep();
            return true;
        }

        private void HandleTaskCompleted(string taskId, bool solvedNormally)
        {
            if (!_waitingForTaskId || !string.Equals(taskId, _pendingTaskId, StringComparison.Ordinal))
            {
                return;
            }
            _waitingForTaskId = false;
            _pendingTaskId = string.Empty;
            AdvanceStep();
        }

        private void AdvanceStep()
        {
            _currentStepIndex++;
            if (_currentStepIndex >= SlotCount)
            {
                CompleteFullAssembly();
            }
            else
            {
                AssemblySlotSpec next = _slots[_currentStepIndex];
                OnAssemblyFeedbackMessage?.Invoke($"Отлично! Следующий шаг ({_currentStepIndex + 1}/5): {next.displayTitle}.");
                OnAssemblyStateUpdated?.Invoke();
            }
        }

        private void CompleteFullAssembly()
        {
            if (_inventory != null && !_inventory.HasItem(GameConstants.ItemIds.Zarya7Assembled))
            {
                _inventory.AddItem(GameConstants.ItemIds.Zarya7Assembled, 1);
            }
            if (_gameState != null && _gameState.Data != null)
            {
                _gameState.Data.isZarya7Assembled = true;
                if (_saveSystem != null)
                {
                    _saveSystem.Save(_gameState.Data);
                }
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-50");
            }
            OnAssemblyFeedbackMessage?.Invoke("Все 5 узлов (3 000 г = 3 кг) собраны! Машина времени «Заря-7» готова к калибровке.");
            OnAssemblyStateUpdated?.Invoke();
            OnAssemblyFullyCompleted?.Invoke();
        }
    }
}
