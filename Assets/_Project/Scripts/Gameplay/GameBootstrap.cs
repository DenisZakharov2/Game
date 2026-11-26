using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Controllers")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private FreeRoamController freeRoamController;

        private SaveSystem _saveSystem;
        private SaveDataV1 _currentSaveData;

        private void Awake()
        {
            InitSaveSystem();
            BindSceneControllers();
        }

        private void InitSaveSystem()
        {
            _saveSystem = new SaveSystem();
            _currentSaveData = _saveSystem.LoadOrCreateDefault();
            Debug.Log($"[TimeMachine] [GameBootstrap] Система сохранения инициализирована. Загружен квест: {_currentSaveData.currentQuestId}");
        }

        public void BindSceneControllers()
        {
            if (playerController != null && thirdPersonCamera != null)
            {
                playerController.Initialize(thirdPersonCamera.transform);
                thirdPersonCamera.SetTarget(playerController.transform);
                Debug.Log("[TimeMachine] [GameBootstrap] Игрок и Камера успешно связаны без вызовов Find.");
            }
            else
            {
                Debug.LogWarning("[TimeMachine] [GameBootstrap] Не все контроллеры (Player/Camera) назначены на сцене!");
            }

            if (freeRoamController != null && playerController != null)
            {
                freeRoamController.Initialize(playerController.transform);
            }
        }

        public void SetSceneControllers(PlayerController player, ThirdPersonCamera cameraController, FreeRoamController freeRoam)
        {
            playerController = player;
            thirdPersonCamera = cameraController;
            freeRoamController = freeRoam;
            BindSceneControllers();
        }
    }
}
