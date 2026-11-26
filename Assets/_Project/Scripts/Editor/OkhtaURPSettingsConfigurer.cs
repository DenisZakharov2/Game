using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace TimeMachine.EditorTools
{
    /// <summary>
    /// Автоматический конфигуратор URP Volume и постобработки сцены
    /// для стилизации под детскую книжную иллюстрацию (акварель и гуашь).
    /// </summary>
    public static class OkhtaURPSettingsConfigurer
    {
        private const string ProfilePath = "Assets/_Project/Art/Materials/OkhtaWatercolorVolumeProfile.asset";

        [MenuItem("TimeMachine/Configure URP Watercolor Lighting & Volume")]
        public static void ConfigureURPSettings()
        {
            // 1. Создание или загрузка URP Volume Profile
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "OkhtaWatercolorVolumeProfile";

                EnsureDirectoryExists("Assets/_Project/Art/Materials");
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            // 2. Настройка Color Adjustments (теплый акварельный тон)
            if (!profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments = profile.Add<ColorAdjustments>(true);
            }
            colorAdjustments.active = true;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = 0.2f;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.contrast.value = 10f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 15f; // Насыщенная пастель

            // 3. Настройка Tonemapping
            if (!profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(true);
            }
            tonemapping.active = true;
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.Neutral;

            // 4. Настройка мягкой виньетки
            if (!profile.TryGet(out Vignette vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.22f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.8f;

            // 5. Размещение Volume на сцене
            GameObject lightingRoot = GameObject.Find("[Lighting]");
            if (lightingRoot == null)
            {
                lightingRoot = new GameObject("[Lighting]");
            }

            Transform volumeTransform = lightingRoot.transform.Find("GlobalVolume");
            GameObject volumeObj;
            if (volumeTransform == null)
            {
                volumeObj = new GameObject("GlobalVolume");
                volumeObj.transform.SetParent(lightingRoot.transform, false);
            }
            else
            {
                volumeObj = volumeTransform.gameObject;
            }

            Volume volumeComp = volumeObj.GetComponent<Volume>();
            if (volumeComp == null) volumeComp = volumeObj.AddComponent<Volume>();

            volumeComp.isGlobal = true;
            volumeComp.priority = 1f;
            volumeComp.sharedProfile = profile;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TimeMachine] [OkhtaURPSettingsConfigurer] URP Volume и акварельная постобработка успешно применены!");
        }

        private static void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
