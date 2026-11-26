using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    public class AssemblyPuzzleController : MonoBehaviour
    {
        [Header("Puzzle State")]
        [SerializeField] private bool isAssembled = false;

        public bool IsAssembled => isAssembled;

        private void Start()
        {
            SyncWithGameState();
        }

        private void SyncWithGameState()
        {
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                isAssembled = GameState.Instance.CurrentData.isZarya7Assembled;
            }
        }

        public void CompleteAssembly()
        {
            isAssembled = true;
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                GameState.Instance.CurrentData.isZarya7Assembled = true;
                GameState.Instance.SaveCurrentState();
                Debug.Log("[TimeMachine] [AssemblyPuzzleController] Сборка прибора «Заря-7» завершена!");
            }
        }
    }
}
