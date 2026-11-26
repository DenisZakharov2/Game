using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Core
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public SaveDataV1 CurrentData { get; private set; }
        private SaveSystem _saveSystem;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeState()
        {
            _saveSystem = new SaveSystem();
            CurrentData = _saveSystem.LoadOrCreateDefault();
            if (CurrentData == null)
            {
                CurrentData = CreateNewDefaultState();
            }
        }

        public SaveDataV1 CreateNewDefaultState()
        {
            SaveDataV1 data = new SaveDataV1
            {
                schemaVersion = SaveSystem.CURRENT_SCHEMA_VERSION,
                currentQuestId = "q_2027_01",
                currentEra = 2027,
                playerXP = 0,
                totalXp = 0,
                isZarya7Assembled = false,
                isStoryCompleted = false,
                lessonFreeModeEnabled = false,
                activeCompanionId = "barni"
            };
            SaveSystem.Save(data);
            return data;
        }

        public void SaveCurrentState()
        {
            if (CurrentData != null)
            {
                SaveSystem.Save(CurrentData);
            }
        }

        public void AddXP(int amount)
        {
            if (CurrentData == null) return;
            CurrentData.playerXP += amount;
            CurrentData.totalXp += amount;
            SaveCurrentState();
        }

        public void SetCurrentQuest(string questId)
        {
            if (CurrentData == null) return;
            CurrentData.currentQuestId = questId;
            SaveCurrentState();
        }
    }
}
