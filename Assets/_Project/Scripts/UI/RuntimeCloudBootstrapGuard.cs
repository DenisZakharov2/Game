using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TimeMachine.UI
{
    /// <summary>
    /// Рантайм-страховка для Android APK (в т.ч. Samsung Galaxy S20 FE, Android 13 / One UI 5.1):
    /// гарантирует портретную ориентацию 9:16, активную камеру, рабочие материалы и кириллический шрифт TextMeshPro.
    /// </summary>
    public static class RuntimeCloudBootstrapGuard
    {
        private static readonly Color32 PetersburgSkyColor = new Color32(0xB8, 0xCE, 0xE0, 0xFF);
        private static TMP_FontAsset _runtimeCyrillicFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePlayableRuntimeState()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Application.targetFrameRate = 60;

            EnsureCameraAndLight();
            RepairBrokenMaterials();
            EnsureCyrillicTmpFonts();
            EnsureFallbackDiagnosticIfSceneEmpty();
        }

        private static void EnsureCameraAndLight()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
            }

            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.transform.position = new Vector3(0f, 3.5f, -12.7f);
                camGo.transform.LookAt(new Vector3(0f, 1.5f, -5.0f));
            }

            if (RenderSettings.skybox == null || cam.clearFlags == CameraClearFlags.Skybox)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = PetersburgSkyColor;
            }

            if (UnityEngine.Object.FindAnyObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Runtime_Directional_Light");
                Light dirLight = lightGo.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.color = new Color32(0xE3, 0xE8, 0xF0, 0xFF);
                dirLight.intensity = 1.15f;
                lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            }
        }

        private static void RepairBrokenMaterials()
        {
            bool isSrp = GraphicsSettings.defaultRenderPipeline != null || QualitySettings.renderPipeline != null;
            Shader safeShader = isSrp
                ? (Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                    ?? Shader.Find("Sprites/Default"))
                : (Shader.Find("Standard")
                    ?? Shader.Find("Mobile/Diffuse")
                    ?? Shader.Find("Legacy Shaders/Diffuse")
                    ?? Shader.Find("Sprites/Default"));

            if (safeShader == null)
            {
                return;
            }

            MeshRenderer[] renderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>(
                FindObjectsInactive.Include);

            Material defaultFallback = null;
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer mr = renderers[i];
                if (mr == null) continue;

                Material current = mr.sharedMaterial;
                bool needsRepair = current == null
                    || current.shader == null
                    || string.Equals(current.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal)
                    || (!isSrp && current.shader.name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal))
                    || (isSrp && string.Equals(current.shader.name, "Standard", StringComparison.Ordinal));

                if (!needsRepair)
                {
                    continue;
                }

                if (current != null)
                {
                    Color preservedColor = new Color32(0xC8, 0xC2, 0xB8, 0xFF);
                    if (current.HasProperty("_BaseColor")) preservedColor = current.GetColor("_BaseColor");
                    else if (current.HasProperty("_Color")) preservedColor = current.GetColor("_Color");

                    Material repaired = new Material(safeShader);
                    repaired.color = preservedColor;
                    if (repaired.HasProperty("_BaseColor")) repaired.SetColor("_BaseColor", preservedColor);
                    if (repaired.HasProperty("_Smoothness")) repaired.SetFloat("_Smoothness", 0f);
                    if (repaired.HasProperty("_Glossiness")) repaired.SetFloat("_Glossiness", 0f);
                    mr.sharedMaterial = repaired;
                }
                else
                {
                    if (defaultFallback == null)
                    {
                        defaultFallback = new Material(safeShader);
                        defaultFallback.color = new Color32(0xC8, 0xC2, 0xB8, 0xFF);
                    }
                    mr.sharedMaterial = defaultFallback;
                }
            }
        }

        private static void EnsureCyrillicTmpFonts()
        {
            TMP_FontAsset cyrillicFont = GetOrCreateRuntimeCyrillicFont();
            TMP_FontAsset bundledFont = Resources.Load<TMP_FontAsset>("Fonts/Okhta_Runtime_SDF");

            if (bundledFont != null && cyrillicFont != null && bundledFont != cyrillicFont)
            {
                AttachFallback(bundledFont, cyrillicFont);
            }

            TMP_FontAsset primaryFont = bundledFont != null ? bundledFont : cyrillicFont;
            if (primaryFont == null)
            {
                return;
            }

            TMP_Text[] allTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include);

            HashSet<TMP_FontAsset> patchedFonts = new HashSet<TMP_FontAsset>();
            for (int i = 0; i < allTexts.Length; i++)
            {
                TMP_Text tmp = allTexts[i];
                if (tmp == null) continue;

                if (tmp.font == null)
                {
                    tmp.font = primaryFont;
                }
                else if (cyrillicFont != null && tmp.font != cyrillicFont && patchedFonts.Add(tmp.font))
                {
                    AttachFallback(tmp.font, cyrillicFont);
                }

                tmp.ForceMeshUpdate();
            }
        }

        private static void AttachFallback(TMP_FontAsset target, TMP_FontAsset fallback)
        {
            if (target == null || fallback == null || target == fallback)
            {
                return;
            }

            if (target.fallbackFontAssetTable == null)
            {
                target.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }

            if (!target.fallbackFontAssetTable.Contains(fallback))
            {
                target.fallbackFontAssetTable.Insert(0, fallback);
            }
        }

        private static TMP_FontAsset GetOrCreateRuntimeCyrillicFont()
        {
            if (_runtimeCyrillicFont != null)
            {
                return _runtimeCyrillicFont;
            }

            try
            {
                Font osFont = null;
                try
                {
                    osFont = Font.CreateDynamicFontFromOSFont(
                        new[] { "Roboto", "Noto Sans", "Droid Sans", "Arial", "sans-serif" },
                        36);
                }
                catch
                {
                    osFont = null;
                }

                if (osFont == null)
                {
                    osFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }

                if (osFont == null)
                {
                    return null;
                }

                _runtimeCyrillicFont = TMP_FontAsset.CreateFontAsset(osFont);
                if (_runtimeCyrillicFont != null)
                {
                    UnityEngine.Object.DontDestroyOnLoad(_runtimeCyrillicFont);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RuntimeGuard] Ошибка создания кириллического TMP_FontAsset: {ex.Message}");
            }

            return _runtimeCyrillicFont;
        }

        private static void EnsureFallbackDiagnosticIfSceneEmpty()
        {
            bool hasManagers = GameObject.Find("--- MANAGERS ---") != null;
            Canvas existingCanvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (hasManagers && existingCanvas != null)
            {
                return;
            }

            GameObject canvasGo = new GameObject("Canvas_RuntimeDiagnostic");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelGo = new GameObject("DiagnosticPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            Image bg = panelGo.AddComponent<Image>();
            bg.color = new Color32(0xF7, 0xF1, 0xE3, 0xEE);
            RectTransform panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.08f, 0.30f);
            panelRt.anchorMax = new Vector2(0.92f, 0.70f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            GameObject textGo = new GameObject("DiagnosticText");
            textGo.transform.SetParent(panelGo.transform, false);
            Text uiText = textGo.AddComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 34;
            uiText.color = new Color32(0x2B, 0x2B, 0x2B, 0xFF);
            uiText.alignment = TextAnchor.MiddleCenter;

            Scene active = SceneManager.GetActiveScene();
            bool srpActive = GraphicsSettings.defaultRenderPipeline != null || QualitySettings.renderPipeline != null;
            uiText.text =
                "Машина времени: диагностика запуска\n\n" +
                $"Сцена: {active.name} (объектов: {active.rootCount})\n" +
                $"Managers: {hasManagers} | Canvas: {existingCanvas != null}\n" +
                $"API: {SystemInfo.graphicsDeviceType} | SRP: {srpActive}\n" +
                $"GPU: {SystemInfo.graphicsDeviceName}\n\n" +
                "Проверьте строку [CloudBuild] в логе cloud.unity.com.";

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.05f, 0.05f);
            textRt.anchorMax = new Vector2(0.95f, 0.95f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }
    }
}
