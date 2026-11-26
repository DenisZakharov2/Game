using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private FreeRoamController freeRoamController;

        private void Awake()
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
                Debug.Log("[TimeMachine] [GameBootstrap] Игрок и Камера успешно связаны.");
            }
            else
            {
                Debug.LogWarning("[TimeMachine] [GameBootstrap] Не все контроллеры назначены на сцене!");
            }

            if (freeRoamController != null && playerController != null)
            {
                freeRoamController.Initialize(playerController.transform);
            }
        }
    }
}
