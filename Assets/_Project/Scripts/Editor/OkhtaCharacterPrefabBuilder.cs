using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.IO;
using TimeMachine.Gameplay;

namespace TimeMachine.EditorTools
{
    /// <summary>
    /// Автоматический генератор URP-материалов и префабов персонажей
    /// в стиле «Книжная акварель и гуашь» (Smoothness = 0).
    /// </summary>
    public static class OkhtaCharacterPrefabBuilder
    {
        private const string MaterialsFolder = "Assets/_Project/Art/Materials";
        private const string PrefabsFolder = "Assets/_Project/Prefabs/Characters";

        [MenuItem("TimeMachine/Build Character Prefabs & Materials")]
        public static void BuildPrefabsAndMaterials()
        {
            EnsureDirectoriesExist();

            // 1. Создание URP-материалов в матовом стиле книжной иллюстрации
            Material matSonya = CreateWatercolorMaterial("Mat_Sonya2027", new Color(0.85f, 0.35f, 0.25f));     // Теплый терракотовый
            Material matBarni = CreateWatercolorMaterial("Mat_Barni", new Color(0.92f, 0.75f, 0.45f));         // Золотистый ретривер
            Material matSonya2040 = CreateWatercolorMaterial("Mat_Sonya2040", new Color(0.25f, 0.45f, 0.65f)); // Сине-бирюзовый костюм
            Material matArtur = CreateWatercolorMaterial("Mat_Artur", new Color(0.40f, 0.50f, 0.35f));         // Оливково-зеленый
            Material matZhulik = CreateWatercolorMaterial("Mat_Zhulik", new Color(0.90f, 0.50f, 0.20f));        // Рыжий кот
            Material matZinaida = CreateWatercolorMaterial("Mat_Zinaida", new Color(0.65f, 0.55f, 0.70f));      // Пастельно-сиреневый
            Material matFriends = CreateWatercolorMaterial("Mat_Friends", new Color(0.30f, 0.60f, 0.70f));      // Бирюзово-голубой

            // 2. Создание префаба Главной Героини (Соня-2027)
            BuildSonyaPrefab(matSonya);

            // 3. Создание префаба Спутника (Пёс Барни)
            BuildCompanionBarniPrefab(matBarni);

            // 4. Создание префабов NPC и Спутников-людей
            BuildNpcPrefab("NPC_Sonya2040", matSonya2040, typeof(Sonya2040EscortController));
            BuildNpcPrefab("NPC_Artur", matArtur);
            BuildNpcPrefab("NPC_Zhulik", matZhulik);
            BuildNpcPrefab("NPC_ZinaidaPetrovna", matZinaida);
            BuildNpcPrefab("NPC_Sasha", matFriends);
            BuildNpcPrefab("NPC_Kristina", matFriends);
            BuildNpcPrefab("NPC_Lyova", matFriends);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TimeMachine] [OkhtaCharacterPrefabBuilder] Все материалы и префабы персонажей успешно сгенерированы!");
        }

        private static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(MaterialsFolder)) Directory.CreateDirectory(MaterialsFolder);
            if (!Directory.Exists(PrefabsFolder)) Directory.CreateDirectory(PrefabsFolder);
        }

        private static Material CreateWatercolorMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null)
            {
                Shader uploaderShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/SimpleLit") ?? Shader.Find("Standard");
                mat = new Material(uploaderShader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;

            // Принудительное отключение глянца и бликов (стиль книжной гуаши)
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void BuildSonyaPrefab(Material material)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Sonya2027";
            root.transform.position = Vector3.zero;

            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (root.GetComponent<PlayerController>() == null) root.AddComponent<PlayerController>();
            if (root.GetComponent<Animator>() == null) root.AddComponent<Animator>();

            SavePrefab(root, $"{PrefabsFolder}/Sonya2027.prefab");
            Object.DestroyImmediate(root);
        }

        private static void BuildCompanionBarniPrefab(Material material)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Companion_Barni";
            root.transform.localScale = new Vector3(0.8f, 0.6f, 1.2f);

            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (root.GetComponent<NavMeshAgent>() == null)
            {
                NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
                agent.speed = 3.8f;
                agent.stoppingDistance = 2.2f;
            }

            if (root.GetComponent<CompanionController>() == null) root.AddComponent<CompanionController>();
            if (root.GetComponent<Animator>() == null) root.AddComponent<Animator>();

            SavePrefab(root, $"{PrefabsFolder}/Companion_Barni.prefab");
            Object.DestroyImmediate(root);
        }

        private static void BuildNpcPrefab(string name, Material material, System.Type extraComponent = null)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = name;

            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (extraComponent != null && root.GetComponent(extraComponent) == null)
            {
                root.AddComponent(extraComponent);
            }

            if (root.GetComponent<InteractableObject>() == null) root.AddComponent<InteractableObject>();
            if (root.GetComponent<Animator>() == null) root.AddComponent<Animator>();

            SavePrefab(root, $"{PrefabsFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
        }
    }
}
