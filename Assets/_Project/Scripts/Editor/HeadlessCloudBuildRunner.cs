#if UNITY_EDITOR
using System;
using System.IO;
using TimeMachine.Core;
using TimeMachine.Gameplay;
using TimeMachine.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace TimeMachine.Editor
{
    public class CloudSceneAutoPopulator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            HeadlessCloudBuildRunner.PopulateActiveSceneIfEmpty();
        }
    }

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

        public static void PopulateActiveSceneIfEmpty()
        {
            if (GameObject.Find("--- MANAGERS ---") != null)
            {
                return;
            }

            Debug.Log("[CloudBuild] Автосборка сцены Scene_Okhta_DualEra и портретного UI 9:16...");

            ApplyAndroidPortraitSettings();
            BuildGreyboxIntoActiveScene();
            SetupManagersActorsAndUi();

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
            }

            OkhtaFullGameValidator.ValidateFullGame();
            Debug.Log("[CloudBuild] Сцена и UI 9:16 сохранены и готовы к упаковке в .apk!");
        }

        private static void ApplyAndroidPortraitSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        }

        private static void BuildGreyboxIntoActiveScene()
        {
            var lightGo = new GameObject("Directional_Light_PetersburgDay");
            var dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color32(0xE3, 0xE8, 0xF0, 0xFF);
            dirLight.intensity = 1.1f;
            dirLight.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color32(0xC9, 0xD3, 0xE0, 0xFF);
            RenderSettings.ambientEquatorColor = new Color32(0xD8, 0xD5, 0xCE, 0xFF);
            RenderSettings.ambientGroundColor = new Color32(0xDF, 0xD6, 0xC7, 0xFF);

            var envRoot = new GameObject("--- ENVIRONMENT ---");
            var propsRoot = new GameObject("--- PROPS ---");
            var triggersRoot = new GameObject("--- TRIGGERS ---");
            var npcsRoot = new GameObject("--- NPCS ---");
            var spawnsRoot = new GameObject("--- SPAWNS ---");

            Material matGround = CreateMat("Mat_Ground_Beige", new Color32(0xC8, 0xC2, 0xB8, 0xFF));
            Material matGrass = CreateMat("Mat_Grass_Olive", new Color32(0x5B, 0x7A, 0x53, 0xFF));
            Material matConcrete = CreateMat("Mat_Concrete_Grey", new Color32(0x8F, 0x94, 0x99, 0xFF));
            Material matBrick = CreateMat("Mat_Brick_Terracotta", new Color32(0xA8, 0x4A, 0x3B, 0xFF));
            Material matSilicate = CreateMat("Mat_Silicate_LightGrey", new Color32(0xD2, 0xCE, 0xC6, 0xFF));
            Material matAccentRed = CreateMat("Mat_Accent_Red", new Color32(0xC9, 0x3B, 0x2B, 0xFF));
            Material matZimaLeto = CreateMat("Mat_ZimaLeto_LimeBlue", new Color32(0x6E, 0xB8, 0xA6, 0xFF));
            Material matKitchen = CreateMat("Mat_Kitchen_Ochre", new Color32(0xD4, 0xA3, 0x59, 0xFF));
            Material matSteel = CreateMat("Mat_Bridge_Steel", new Color32(0x6C, 0x76, 0x82, 0xFF));

            var ground = CreateBox("ENV_Quarter_Ground", envRoot.transform, new Vector3(75f, -0.25f, 10f), new Vector3(360f, 0.5f, 360f), matGround, true);
            var navSurface = ground.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.All;

            // L-2027-HILL
            CreateBox("ENV_Hill_Mound", envRoot.transform, new Vector3(0f, 1.75f, 0f), new Vector3(18f, 3.5f, 14f), matGrass, true);
            CreateBox("ENV_Hill_Portal", envRoot.transform, new Vector3(2f, 1.3f, -4f), new Vector3(4f, 2.6f, 3.5f), matConcrete, true);
            CreateBox("PROP_Hill_Door_2027", propsRoot.transform, new Vector3(2f, 1.1f, -2.5f), new Vector3(1.4f, 2.1f, 0.15f), matAccentRed, true);
            var bottle = CreateBox("PROP_Hill_MilkBottle", propsRoot.transform, new Vector3(0.6f, 0.25f, -4.8f), new Vector3(0.25f, 0.4f, 0.25f), matZimaLeto, false);
            var bottleInteract = bottle.AddComponent<InteractableObject>();
            bottleInteract.Configure("INT_Q01_Bottle", "Достать бутылку из провала", "T-RUS-01", true);
            bottleInteract.SetLinkedLoreId("LORE-01");

            CreateBox("ENV_Hill_Stairs", envRoot.transform, new Vector3(-4f, 0.8f, -2f), new Vector3(2.2f, 1.6f, 4.5f), matConcrete, true);
            CreateBox("PROP_Hill_VentShaft", propsRoot.transform, new Vector3(-6f, 0.9f, 8f), new Vector3(2f, 1.8f, 2f), matConcrete, true);
            CreateBox("ENV_Playground_Zone", envRoot.transform, new Vector3(12f, 0.05f, 25f), new Vector3(20f, 0.1f, 20f), matBrick, true);

            CreateQuestTrigger("TRIG_Q01", triggersRoot.transform, new Vector3(2f, 0f, -4f), 2.5f, QuestIds.Q01, "T-RUS-01");
            CreateMarker("SPAWN_Start_Hill", spawnsRoot.transform, new Vector3(0f, 0f, -7f), matZimaLeto);
            CreateMarker("NPC_barni", npcsRoot.transform, new Vector3(1.2f, 0f, -4.5f), matAccentRed);

            // L-2027-KITCHEN
            CreateBox("ENV_ZimaLeto_Entrance9_6", envRoot.transform, new Vector3(52f, 2.0f, 86f), new Vector3(6f, 4.0f, 4f), matZimaLeto, true);
            CreateMarker("SPAWN_Exit_ZimaLeto", spawnsRoot.transform, new Vector3(52f, 0f, 83.2f), matZimaLeto);

            var doorEnterGo = CreateTriggerBox("TRIG_Door_Kitchen", triggersRoot.transform, new Vector3(52f, 1.0f, 84.5f), new Vector3(2.5f, 2.2f, 2.0f));
            doorEnterGo.AddComponent<KitchenPortalTrigger>().Configure("TRIG_Door_Kitchen", "Подняться на кухню Сони (ЖК «ЗимаЛето», 9/6)", new Vector3(500f, 0.1f, 498.5f), 0f);

            CreateBox("ENV_Kitchen_Floor", envRoot.transform, new Vector3(500f, -0.1f, 500f), new Vector3(5f, 0.2f, 4f), matKitchen, true);
            CreateBox("ENV_Kitchen_WallNorth", envRoot.transform, new Vector3(500f, 1.5f, 502f), new Vector3(5f, 3.0f, 0.2f), matSilicate, true);
            CreateBox("ENV_Kitchen_WallSouth", envRoot.transform, new Vector3(500f, 1.5f, 498f), new Vector3(5f, 3.0f, 0.2f), matSilicate, true);
            CreateBox("ENV_Kitchen_WallWest", envRoot.transform, new Vector3(497.5f, 1.5f, 500f), new Vector3(0.2f, 3.0f, 4f), matSilicate, true);
            CreateBox("ENV_Kitchen_WallEast", envRoot.transform, new Vector3(502.5f, 1.5f, 500f), new Vector3(0.2f, 3.0f, 4f), matSilicate, true);
            CreateBox("PROP_Kitchen_Table", propsRoot.transform, new Vector3(500f, 0.375f, 501f), new Vector3(1.6f, 0.75f, 0.9f), matKitchen, true);

            var microwave = CreateBox("PROP_Kitchen_Microwave", propsRoot.transform, new Vector3(500f, 0.9f, 501f), new Vector3(0.5f, 0.3f, 0.35f), matSilicate, false);
            microwave.AddComponent<InteractableObject>().Configure("INT_Q02_Microwave", "Разобрать старую микроволновку с Сашей", "T-MATH-02", false);

            CreateBox("PROP_Kitchen_ParentNote", propsRoot.transform, new Vector3(499.45f, 0.76f, 501f), new Vector3(0.25f, 0.02f, 0.2f), matAccentRed, false);
            CreateQuestTrigger("TRIG_Q02", triggersRoot.transform, new Vector3(500f, 0f, 500.2f), 2.0f, QuestIds.Q02, "T-MATH-02");

            var doorExitGo = CreateTriggerBox("TRIG_Door_Kitchen_Exit", triggersRoot.transform, new Vector3(500f, 1.0f, 498.4f), new Vector3(2.0f, 2.2f, 1.5f));
            doorExitGo.AddComponent<KitchenPortalTrigger>().Configure("TRIG_Door_Kitchen_Exit", "Выйти во двор ЖК «ЗимаЛето»", new Vector3(52f, 0.1f, 83.2f), 180f);

            // L-2027-COURTYARD71
            CreateBox("ENV_House71_Block", envRoot.transform, new Vector3(18f, 7.5f, 28f), new Vector3(16f, 15f, 12f), matSilicate, true);
            CreateBox("PROP_Courtyard71_Bench", propsRoot.transform, new Vector3(24f, 0.25f, 34f), new Vector3(2.0f, 0.5f, 0.6f), matKitchen, true);
            CreateBox("ENV_DogPark_Zone", envRoot.transform, new Vector3(32f, 0.05f, 46f), new Vector3(22f, 0.1f, 13.3f), matGrass, true);
            CreateBox("ENV_Garages_Row", envRoot.transform, new Vector3(28f, 1.3f, 12f), new Vector3(18f, 2.6f, 6f), matConcrete, true);

            CreateQuestTrigger("TRIG_Q03", triggersRoot.transform, new Vector3(26f, 0f, 38f), 3.5f, QuestIds.Q03, "T-MATH-03");
            CreateMarker("NPC_kristina", npcsRoot.transform, new Vector3(26.5f, 0f, 37.5f), matKitchen);
            CreateMarker("NPC_lyova", npcsRoot.transform, new Vector3(28.0f, 0f, 39.0f), matAccentRed);
            CreateMarker("NPC_babushka", npcsRoot.transform, new Vector3(23.5f, 0f, 34.0f), matBrick);
            CreateMarker("NPC_zhulik", npcsRoot.transform, new Vector3(31.0f, 1.35f, 40.0f), matKitchen);

            // L-2027-HOUSE57 & HOUSE53
            CreateBox("ENV_House57_Body", envRoot.transform, new Vector3(96f, 6.75f, -72f), new Vector3(24f, 13.5f, 12f), matSilicate, true);
            CreateBox("ENV_House57_Arch", envRoot.transform, new Vector3(88f, 1.8f, -72f), new Vector3(3.2f, 3.6f, 4.0f), matBrick, true);
            var markedBrick = CreateBox("PROP_House57_MarkedBrick", propsRoot.transform, new Vector3(88f, 1.2f, -72f), new Vector3(0.4f, 0.3f, 0.4f), matAccentRed, false);
            markedBrick.AddComponent<InteractableObject>().Configure("INT_Q04_Brick", "Осмотреть кирпич с клеймом «Баррикада»", "T-MATH-04", false);
            CreateQuestTrigger("TRIG_Q04", triggersRoot.transform, new Vector3(88f, 0f, -72f), 2.5f, QuestIds.Q04, "T-MATH-04");

            CreateBox("ENV_House53_Body", envRoot.transform, new Vector3(112f, 6.75f, -84f), new Vector3(24f, 13.5f, 12f), matSilicate, true);
            CreateBox("PROP_House53_Gate", propsRoot.transform, new Vector3(114f, 1.0f, -78f), new Vector3(2.2f, 2.0f, 0.2f), matAccentRed, true);
            CreateBox("ENV_House53_ShelterStairs", envRoot.transform, new Vector3(115f, -1.2f, -80f), new Vector3(2.2f, 2.4f, 4.0f), matConcrete, true);
            CreateBox("ENV_House53_ShelterRoom", envRoot.transform, new Vector3(115f, -3.8f, -84f), new Vector3(10f, 2.8f, 8f), matConcrete, true);

            var switchboard = CreateBox("PROP_House53_Switchboard", propsRoot.transform, new Vector3(115f, -1.2f, -87.5f), new Vector3(0.8f, 1.1f, 0.3f), matKitchen, false);
            switchboard.AddComponent<InteractableObject>().Configure("INT_Q05_Switchboard", "Осмотреть щиток в убежище", "T-MATH-05", false);
            CreateMarker("NPC_artur", npcsRoot.transform, new Vector3(113.5f, 0f, -77.5f), matZimaLeto);
            CreateQuestTrigger("TRIG_Q05", triggersRoot.transform, new Vector3(115f, -2.4f, -83.5f), 2.5f, QuestIds.Q05, "T-MATH-05");

            // L-2027-ZIMALETO & BRIDGE
            CreateBox("ENV_ZimaLeto_Tower1", envRoot.transform, new Vector3(45f, 33f, 95f), new Vector3(22f, 66f, 22f), matZimaLeto, true);
            CreateBox("ENV_ZimaLeto_Tower2", envRoot.transform, new Vector3(75f, 33f, 95f), new Vector3(22f, 66f, 22f), matZimaLeto, true);
            CreateBox("ENV_Factory17_LitP", envRoot.transform, new Vector3(72f, 4f, 68f), new Vector3(20f, 8f, 15f), matBrick, true);
            CreateBox("ENV_RailwaySpur_Abandoned", envRoot.transform, new Vector3(87.5f, 0.05f, 46f), new Vector3(95f, 0.1f, 3f), matConcrete, true);
            CreateQuestTrigger("TRIG_Q06", triggersRoot.transform, new Vector3(64f, 0f, 62f), 3.0f, QuestIds.Q06, "T-MATH-06");

            CreateBox("ENV_Bridge_AbutmentWest", envRoot.transform, new Vector3(144f, 2.2f, 42f), new Vector3(6f, 4.4f, 8f), matConcrete, true);
            CreateBox("PROP_Bridge_BrickTower", propsRoot.transform, new Vector3(141f, 2.2f, 39f), new Vector3(2.8f, 4.4f, 2.8f), matKitchen, true);
            var niche = CreateBox("PROP_Bridge_DryStoneNiche", propsRoot.transform, new Vector3(144f, 1.2f, 42f), new Vector3(0.9f, 0.9f, 0.6f), matBrick, false);
            niche.AddComponent<InteractableObject>().Configure("INT_Q07_Niche", "Разобрать сухую кладку в устое моста", "T-MATH-07", false);

            CreateBox("ENV_Bridge_SteelTruss", envRoot.transform, new Vector3(182.5f, 4.5f, 42f), new Vector3(77f, 6.0f, 5.2f), matSteel, true);
            CreateBox("ENV_Bridge_Footbridge", envRoot.transform, new Vector3(182.5f, 2.8f, 37.5f), new Vector3(77f, 0.3f, 2.0f), matConcrete, true);
            CreateBox("PROP_Bridge_FreightCar", propsRoot.transform, new Vector3(132f, 1.9f, 46f), new Vector3(12f, 3.8f, 3f), matBrick, true);
            CreateBox("PROP_Bridge_Bench", propsRoot.transform, new Vector3(138f, 0.25f, 38f), new Vector3(2.0f, 0.5f, 0.6f), matKitchen, true);

            CreateQuestTrigger("TRIG_Q07", triggersRoot.transform, new Vector3(144f, 0f, 42f), 2.5f, QuestIds.Q07, "T-MATH-07");
            CreateQuestTrigger("TRIG_Q08", triggersRoot.transform, new Vector3(142f, 0f, 41f), 3.0f, QuestIds.Q08, "T-MATH-08");

            navSurface.BuildNavMesh();
        }

        private static void SetupManagersActorsAndUi()
        {
            GameObject managersRoot = new GameObject("--- MANAGERS ---");
            GameBootstrap bootstrap = managersRoot.AddComponent<GameBootstrap>();
            managersRoot.AddComponent<AudioManager>();
            EraSwitcher eraSwitcher = managersRoot.AddComponent<EraSwitcher>();
            AssemblyPuzzleController assemblyCtrl = managersRoot.AddComponent<AssemblyPuzzleController>();
            Sonya2040EscortController escortCtrl = managersRoot.AddComponent<Sonya2040EscortController>();
            DualReturnSynchronizer dualReturnCtrl = managersRoot.AddComponent<DualReturnSynchronizer>();
            FreeRoamController freeRoamCtrl = managersRoot.AddComponent<FreeRoamController>();

            GameObject eraRoot = new GameObject("EraWorldRoot");
            var anchors = new GameObject("Layer_Anchors");
            anchors.transform.SetParent(eraRoot.transform);
            var layer2027 = new GameObject("Layer_2027");
            layer2027.transform.SetParent(eraRoot.transform);
            var layer1950 = new GameObject("Layer_1950");
            layer1950.transform.SetParent(eraRoot.transform);
            layer1950.SetActive(false);
            eraSwitcher.ConfigureContainers(anchors, layer2027, layer1950);

            // Player (Sonya 2027)
            GameObject playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player_Sonya2027";
            playerGo.tag = "Player";
            playerGo.transform.position = new Vector3(0f, 1.0f, -7f);
            UnityEngine.Object.DestroyImmediate(playerGo.GetComponent<CapsuleCollider>());
            CharacterController cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.6f;
            cc.radius = 0.35f;
            PlayerController player = playerGo.AddComponent<PlayerController>();

            // Camera (9:16)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            ThirdPersonCamera cam = camGo.AddComponent<ThirdPersonCamera>();
            cam.SetTarget(playerGo.transform);
            player.SetCameraTransform(camGo.transform);

            // Companion (Sasha)
            GameObject compGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            compGo.name = "Companion_Sasha";
            compGo.transform.position = new Vector3(1.5f, 1.0f, -7f);
            compGo.AddComponent<NavMeshAgent>();
            CompanionController companion = compGo.AddComponent<CompanionController>();
            companion.SetFollowTarget(playerGo.transform);

            // Portrait UI 9:16
            OkhtaUiCanvasBuilder.BuildPortraitUiCanvas();
            TouchInput touchInput = UnityEngine.Object.FindAnyObjectByType<TouchInput>();
            player.SetTouchInput(touchInput);

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

        private static void BuildAndroidPackage(bool buildAppBundle, string fallbackOutputPath)
        {
            PopulateActiveSceneIfEmpty();
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;

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
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"[CloudBuild] Ошибка сборки Android: {report.summary.result}, ошибок: {report.summary.totalErrors}");
            }
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool isStatic)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.isStatic = isStatic;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateTriggerBox(string name, Transform parent, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return go;
        }

        private static void CreateQuestTrigger(string name, Transform parent, Vector3 pos, float radius, string questId, string firstTaskId)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;
            go.AddComponent<QuestTrigger>().Configure(questId, firstTaskId, true);
        }

        private static void CreateMarker(string name, Transform parent, Vector3 pos, Material mat)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = name;
            capsule.transform.SetParent(parent);
            capsule.transform.position = pos + Vector3.up * 0.7f;
            capsule.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
            if (mat != null) capsule.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static Material CreateMat(string matName, Color32 color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = matName };
            mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
            return mat;
        }
    }
}
#endif
