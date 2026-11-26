using UnityEngine;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private FreeRoamController freeRoamController;

        private void Awake()
        {
            InitializeScene();
        }

        public void InitializeScene()
        {
            SetSceneControllers(playerController, thirdPersonCamera, freeRoamController);
        }

        public void SetSceneControllers(PlayerController player, ThirdPersonCamera cameraController, FreeRoamController freeRoam)
        {
            playerController = player;
            thirdPersonCamera = cameraController;
            freeRoamController = freeRoam;

            if (playerController != null && thirdPersonCamera != null)
            {
                playerController.Initialize(thirdPersonCamera.transform);
                thirdPersonCamera.SetTarget(playerController.transform);
                Debug.Log("[TimeMachine] [GameBootstrap] Игрок и Камера успешно привязаны.");
            }
            else
            {
                Debug.LogWarning("[TimeMachine] [GameBootstrap] PlayerController или ThirdPersonCamera не привязаны на сцене!");
            }

            if (freeRoamController != null && playerController != null)
            {
                freeRoamController.Initialize(playerController.transform);
                Debug.Log("[TimeMachine] [GameBootstrap] FreeRoamController успешно инициализирован.");
            }
        }
    }
}
