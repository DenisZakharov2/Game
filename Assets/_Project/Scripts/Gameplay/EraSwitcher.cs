using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    public class EraSwitcher : MonoBehaviour
    {
        [Header("Era Settings")]
        [SerializeField] private int currentEra = 2027;

        public int CurrentEra => currentEra;

        private void Start()
        {
            SyncWithGameState();
        }

        public void SyncWithGameState()
        {
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                currentEra = GameState.Instance.CurrentData.currentEra;
            }
        }

        public void SwitchEra(int targetEra)
        {
            if (targetEra != 2027 && targetEra != 1950)
            {
                Debug.LogWarning($"[TimeMachine] [EraSwitcher] Неподдерживаемая эпоха: {targetEra}");
                return;
            }

            currentEra = targetEra;

            if (GameState.Instance != null)
            {
                GameState.Instance.SetCurrentEra(currentEra);
            }

            Debug.Log($"[TimeMachine] [EraSwitcher] Совершен переход в эпоху: {currentEra} год.");
        }

        public bool IsIn1950Era() => currentEra == 1950;
        public bool IsIn2027Era() => currentEra == 2027;
    }
}
