#if UNITY_EDITOR
using System;
using System.IO;
using TimeMachine.Gameplay;
using TimeMachine.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeMachine.Editor
{
    /// <summary>
    /// Автоматический конвейер сборки сцены, портретного UI 9:16 и Android APK/AAB
    /// в облачном режиме -batchmode (Unity Cloud Build Automation / cloud.unity.com).
    /// </summary>
    public static class HeadlessCloudBuildRunner
    {
        private const string DualEraScenePath = "Assets/_Project/Scenes/Scene_Okhta_DualEra.unity";
        private const string DefaultApkOutputPath = "Builds/Android/TimeMachine_Okhta.apk";
        private const string DefaultAabOutputPath = "Builds/Android/TimeMachine_Okhta.aab";

        [MenuItem("TimeMachine/Cloud Build Android APK (Headless)", priority = 90)]
        public static void BuildAndroidApkFromCloud()
        {
            BuildAndroidPackage(buildAppBundle: false, fallbackOutputPath: DefaultApkOutputPath);
        }

        [MenuItem("TimeMachine/Cloud Build Android AAB (Headless)", priority = 91)]
        public static void BuildAndroidAabFromCloud()
        {
            BuildAndroidPackage(buildAppBundle: true, fallbackOutputPath: DefaultAabOutputPath);
        }

        private static void BuildAndroidPackage(bool buildAppBundle, string fallbackOutputPath)
        {
            Debug.Log("[CloudBuild] Старт автоматической сборки «Машина времени. Спасаем будущее в прошлом»...");

            // 1. Генерация базовой Greybox-сцены и NavMesh
            EnsureDirectoryForAsset(DualEraScenePath);
            OkhtaGreyboxSceneBuilder.BuildGreyboxScene();

            // 2. Инициализация менеджеров Глав 1-6 и слоёв эпох (2027 <-> 1950)
            SetupManagersAndEraLayers();

            // 3. Автоматическая сборка портретного интерфейса Canvas_Main_9x16 (1080x2400, кнопки >= 48 dp)
            OkhtaUiCanvasBuilder.BuildPortraitUiCanvas();

            // 4. Сохранение двухэпохальной сцены и добавление её в Build Settings
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.SaveScene(activeScene, DualEraScenePath);
            }
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(DualEraScenePath, true)
            };

            // 5. Запуск автоматического валидатора (47 задач, 25 карточек, 24 квеста, 468 строк без 'ё' в 1950 г.)
            OkhtaFullGameValidator.ValidateFullGame();

            // 6. Настройка целевой платформы Android (Portrait 9:16, IL2CPP, ARM64, API 26+, 100% офлайн)
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;

            // Определение пути вывода (учитываем путь, переданный Unity Build Automation)
            string outputPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Environment.GetEnvironmentVariable("BUILD_OUTPUT_PATH");
            }
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = fallbackOutputPath;
            }

            string outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { DualEraScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[CloudBuild] УСПЕХ! Пакет собран: {outputPath} ({summary.totalSize / (1024 * 1024)} МБ)");
            }
            else
            {
                throw new Exception($"[CloudBuild] Ошибка сборки Android: {summary.result}, ошибок: {summary.totalErrors}");
            }
        }

        private static void SetupManagersAndEraLayers()
        {
            GameObject managersRoot = GameObject.Find("--- MANAGERS ---");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("--- MANAGERS ---");
            }

            GameBootstrap bootstrap = GetOrAddComponent<GameBootstrap>(managersRoot);
            GetOrAddComponent<AudioManager>(managersRoot);
            EraSwitcher eraSwitcher = GetOrAddComponent<EraSwitcher>(managersRoot);
            AssemblyPuzzleController assemblyCtrl = GetOrAddComponent<AssemblyPuzzleController>(managersRoot);
            Sonya2040EscortController escortCtrl = GetOrAddComponent<Sonya2040EscortController>(managersRoot);
            DualReturnSynchronizer dualReturnCtrl = GetOrAddComponent<DualReturnSynchronizer>(managersRoot);
            FreeRoamController freeRoamCtrl = GetOrAddComponent<FreeRoamController>(managersRoot);

            GameObject eraRoot = GameObject.Find("EraWorldRoot");
            if (eraRoot == null)
            {
                eraRoot = new GameObject("EraWorldRoot");
                var anchors = new GameObject("Layer_Anchors");
                anchors.transform.SetParent(eraRoot.transform);
                var layer2027 = new GameObject("Layer_2027");
                layer2027.transform.SetParent(eraRoot.transform);
                var layer1950 = new GameObject("Layer_1950");
                layer1950.transform.SetParent(eraRoot.transform);
                layer1950.SetActive(false);

                eraSwitcher.ConfigureContainers(anchors, layer2027, layer1950);
            }

            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            ThirdPersonCamera cam = UnityEngine.Object.FindAnyObjectByType<ThirdPersonCamera>();
            CompanionController companion = UnityEngine.Object.FindAnyObjectByType<CompanionController>();
            TouchInput touchInput = UnityEngine.Object.FindAnyObjectByType<TouchInput>();

            bootstrap.SetSceneControllers(
                player,
                cam,
                companion,
                touchInput,
                eraSwitcher,
                assemblyCtrl,
                escortCtrl,
                dualReturnCtrl,
                freeRoamCtrl);
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                comp = target.AddComponent<T>();
            }
            return comp;
        }

        private static void EnsureDirectoryForAsset(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
#endif
