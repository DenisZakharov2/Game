using UnityEditor;
using UnityEngine;

namespace TimeMachine.EditorTools
{
    /// <summary>
    /// Мастер-контроллер полной автоматизации Этапа 2.
    /// Запускает создание папок, сборку сцены, генерацию префабов персонажей и окружения.
    /// </summary>
    public static class OkhtaStage2MasterBuilder
    {
        [MenuItem("TimeMachine/Execute Full Stage 2 Automation", false, 0)]
        public static void ExecuteFullStage2Automation()
        {
            Debug.Log("[TimeMachine] === ЗАПУСК АВТОМАТИЗАЦИИ ЭТАПА 2 ===");

            // 1. Создание структуры папок
            ProjectFolderSetup.CreateRequiredFolders();

            // 2. Генерация акварельных URP-материалов и префабов персонажей
            OkhtaCharacterPrefabBuilder.BuildPrefabsAndMaterials();

            // 3. Генерация материалов и префабов окружения
            OkhtaEnvironmentPrefabBuilder.BuildEnvironmentPrefabsAndMaterials();

            // 4. Сборка игровой сцены Scene_Okhta_DualEra
            OkhtaGreyboxSceneBuilder.BuildOkhtaScene();

            // 5. Запуск валидатора готовности
            OkhtaFullGameValidator.ValidateFullGame();

            Debug.Log("[TimeMachine] === ЭТАП 2 УСПЕШНО СОБРАН И ВАЛИДИРОВАН ===");
        }
    }
}
