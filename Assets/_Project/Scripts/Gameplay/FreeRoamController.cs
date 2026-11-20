using System;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;

namespace TimeMachine.Gameplay
{
    [Serializable]
    public struct FreeRoamPointOfInterest
    {
        public string locationId;
        public string loreId;
        public string title;
        public Vector3 worldPosition;
        public float triggerRadius;
        public bool inspectedInSession;
    }

    public class FreeRoamController : MonoBehaviour
    {
        [Header("Free Roam District Exploration (Post Q-24)")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private FreeRoamPointOfInterest[] pointsOfInterest;
        [SerializeField] private float scanInterval = 0.15f;

        private GameState _gameState;
        private ContentDatabase _database;
        private LoreJournal _loreJournal;
        private TaskGenerator _taskGenerator;
        private SaveSystem _saveSystem;

        private bool _isFreeRoamActive;
        private float _scanTimer;
        private int _focusedIndex = -1;
        private int _mathSeedCounter = 100;
        private int _rusSeedCounter = 200;

        public event Action<FreeRoamPointOfInterest> OnPointOfInterestFocused;
        public event Action OnPointOfInterestCleared;
        public event Action<GeneratedTaskInstance> OnPracticeTaskGenerated;

        public bool IsFreeRoamActive => _isFreeRoamActive;

        public void BindSystems(
            GameState state,
            ContentDatabase database,
            LoreJournal loreJournal,
            TaskGenerator generator,
            SaveSystem saveSystem)
        {
            _gameState = state;
            _database = database;
            _loreJournal = loreJournal;
            _taskGenerator = generator;
            _saveSystem = saveSystem;
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void ConfigurePointsOfInterest(FreeRoamPointOfInterest[] pois)
        {
            pointsOfInterest = pois;
        }

        public void ActivateFreeRoam()
        {
            _isFreeRoamActive = true;
            if (_gameState != null && _gameState.Data != null)
            {
                _gameState.Data.isStoryCompleted = true;
                if (_saveSystem != null)
                {
                    _saveSystem.Save(_gameState.Data);
                }
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-75");
            }
        }

        public bool InspectPointOfInterestByIndex(int index)
        {
            if (pointsOfInterest == null || index < 0 || index >= pointsOfInterest.Length)
            {
                return false;
            }

            FreeRoamPointOfInterest poi = pointsOfInterest[index];
            if (_loreJournal != null && !string.IsNullOrEmpty(poi.loreId))
            {
                if (_database != null && _database.TryGetLoreCard(poi.loreId, out LoreCardData card))
                {
                    if (!_loreJournal.IsCardVerified(card))
                    {
                        return false;
                    }
                    _loreJournal.TryUnlockCard(poi.loreId);
                }
            }

            if (!poi.inspectedInSession)
            {
                poi.inspectedInSession = true;
                pointsOfInterest[index] = poi;
                if (_gameState != null)
                {
                    _gameState.AddXp(10);
                }
            }

            return true;
        }

        public bool TryStartMathPracticeTask(out GeneratedTaskInstance instance)
        {
            instance = null;
            if (_database == null || _taskGenerator == null)
            {
                return false;
            }
            int tplIndex = (_mathSeedCounter % 10) + 1;
            string tplId = tplIndex < 10 ? $"TPL-MATH-0{tplIndex}" : $"TPL-MATH-{tplIndex}";
            _mathSeedCounter++;

            if (_database.TryGetTemplate(tplId, out TemplateData tpl))
            {
                instance = _taskGenerator.GenerateFromTemplate(tpl, _mathSeedCounter);
                OnPracticeTaskGenerated?.Invoke(instance);
                return instance != null;
            }
            return false;
        }

        public bool TryStartRussianPracticeTask(out GeneratedTaskInstance instance)
        {
            instance = null;
            if (_database == null || _taskGenerator == null)
            {
                return false;
            }
            int tplIndex = (_rusSeedCounter % 10) + 1;
            string tplId = tplIndex < 10 ? $"TPL-RUS-0{tplIndex}" : $"TPL-RUS-{tplIndex}";
            _rusSeedCounter++;

            if (_database.TryGetTemplate(tplId, out TemplateData tpl))
            {
                instance = _taskGenerator.GenerateFromTemplate(tpl, _rusSeedCounter);
                OnPracticeTaskGenerated?.Invoke(instance);
                return instance != null;
            }
            return false;
        }

        private void Update()
        {
            if (!_isFreeRoamActive || playerTransform == null || pointsOfInterest == null || pointsOfInterest.Length == 0)
            {
                return;
            }

            _scanTimer += Time.deltaTime;
            if (_scanTimer < scanInterval)
            {
                return;
            }
            _scanTimer = 0f;

            Vector3 playerPos = playerTransform.position;
            int nearestIndex = -1;
            float bestDistSqr = float.MaxValue;

            for (int i = 0; i < pointsOfInterest.Length; i++)
            {
                float r = pointsOfInterest[i].triggerRadius > 0f ? pointsOfInterest[i].triggerRadius : 3.0f;
                float distSqr = (playerPos - pointsOfInterest[i].worldPosition).sqrMagnitude;
                if (distSqr <= r * r && distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    nearestIndex = i;
                }
            }

            if (nearestIndex != _focusedIndex)
            {
                _focusedIndex = nearestIndex;
                if (_focusedIndex >= 0)
                {
                    OnPointOfInterestFocused?.Invoke(pointsOfInterest[_focusedIndex]);
                }
                else
                {
                    OnPointOfInterestCleared?.Invoke();
                }
            }
        }
    }
}
