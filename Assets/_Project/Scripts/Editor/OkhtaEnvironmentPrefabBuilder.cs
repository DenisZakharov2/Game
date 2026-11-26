using UnityEngine;
using UnityEditor;
using System.IO;

namespace TimeMachine.EditorTools
{
    /// <summary>
    /// Генератор URP-материалов окружения и 3D-префабов локаций Большой Охты
    /// в визуальном стиле «книжная акварель и гуашь» (Smoothness = 0).
    /// </summary>
    public static class OkhtaEnvironmentPrefabBuilder
    {
        private const string MaterialsFolder = "Assets/_Project/Art/Materials/Environment";
        private const string PrefabsFolder = "Assets/_Project/Prefabs/Environment";

        [MenuItem("TimeMachine/Build Environment Prefabs & Materials")]
        public static void BuildEnvironmentPrefabsAndMaterials()
        {
            EnsureDirectoriesExist();

            // 1. Создание матовых URP-материалов для локаций Охты
            Material matBridge2027 = CreateWatercolorMaterial("Mat_Bridge_2027", new Color(0.45f, 0.50f, 0.55f)); // Стальной ферменный мост
            Material matBridge1950 = CreateWatercolorMaterial("Mat_Bridge_1950", new Color(0.55f, 0.45f, 0.35f)); // Каменный мост 1950 г.
            Material matHillGrass = CreateWatercolorMaterial("Mat_HillGrass", new Color(0.40f, 0.65f, 0.35f));  // Земляной вал с зеленью
            Material matBunkerDoor = CreateWatercolorMaterial("Mat_BunkerDoor", new Color(0.35f, 0.35f, 0.40f)); // Дверь бомбоубежища
            Material matHouse71 = CreateWatercolorMaterial("Mat_House71", new Color(0.85f, 0.75f, 0.60f));       // Исторический дом №71
            Material matZimaLeto = CreateWatercolorMaterial("Mat_ZimaLeto", new Color(0.70f, 0.80f, 0.85f));     // Светлый фасад ЖК ЗимаЛето

            // 2. Сборка 3D-префабов окружения
            BuildEnvironmentPrefab("Prefab_Bridge_2027", PrimitiveType.Cube, new Vector3(12f, 4f, 30f), matBridge2027);
            BuildEnvironmentPrefab("Prefab_Bridge_1950", PrimitiveType.Cube, new Vector3(10f, 4f, 28f), matBridge1950);
            BuildEnvironmentPrefab("Prefab_HillBunker", PrimitiveType.Cube, new Vector3(18f, 6f, 18f), matHillGrass);
            BuildEnvironmentPrefab("Prefab_House71", PrimitiveType.Cube, new Vector3(30f, 8f, 30f), matHouse71);
            BuildEnvironmentPrefab("Prefab_ZimaLetoHouse", PrimitiveType.Cube, new Vector3(22f, 14f, 22f), matZimaLeto);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TimeMachine] [OkhtaEnvironmentPrefabBuilder] Все материалы и префабы окружения успешно сгенерированы!");
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

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void BuildEnvironmentPrefab(string name, PrimitiveType primitiveType, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(primitiveType);
            obj.name = name;
            obj.transform.localScale = scale;

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            string prefabPath = $"{PrefabsFolder}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            Object.DestroyImmediate(obj);
        }
    }
}
