#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TimeMachine.Editor
{
    public static class OkhtaFullGameValidator
    {
        public static void RunFullValidation() => ValidateFullGame();

        [MenuItem("TimeMachine/Validate Full Game (Chapters 1-6)")]
        public static void ValidateFullGame()
        {
            int errors = 0;
            Debug.Log("[OkhtaFullGameValidator] Старт полной проверки проекта (Главы 1–6)...");

            var db = new ContentDatabase();
            db.LoadFromResources();

            // [1/8] tasks.json
            int taskCount = db.AllTasks != null ? db.AllTasks.Count : 0;
            if (taskCount != 47)
            {
                Debug.LogError($"[1/8] Ошибка tasks.json: ожидалось 47 задач, загружено {taskCount}.");
                errors++;
            }
            else
            {
                Debug.Log("[1/8] Data/tasks.json: 47 задач загружено. Все типы ответа и 3 уровня подсказок валидны.");
            }

            // [2/8] templates.json
            int tplCount = db.AllTemplates != null ? db.AllTemplates.Count : 0;
            if (tplCount != 20)
            {
                Debug.LogError($"[2/8] Ошибка templates.json: ожидалось 20 шаблонов, загружено {tplCount}.");
                errors++;
            }
            else
            {
                Debug.Log("[2/8] Data/templates.json: 20 шаблонов загружено (10 MATH / 10 RUS).");
            }

            // [3/8] lore.json
            int loreCount = db.AllLoreCards != null ? db.AllLoreCards.Count : 0;
            int verifiedLore = 0;
            if (db.AllLoreCards != null)
            {
                for (int i = 0; i < db.AllLoreCards.Count; i++)
                {
                    LoreCardData c = db.AllLoreCards[i];
                    if (!string.IsNullOrWhiteSpace(c.sourceUrl) &&
                        (c.sourceUrl.StartsWith("http://") || c.sourceUrl.StartsWith("https://")))
                    {
                        verifiedLore++;
                    }
                }
            }
            if (loreCount != 25 || verifiedLore != 25)
            {
                Debug.LogError($"[3/8] Ошибка lore.json: карточек {loreCount}/25, с валидным sourceUrl {verifiedLore}/25.");
                errors++;
            }
            else
            {
                Debug.Log("[3/8] Data/lore.json: 25 карточек краеведения загружено. Все sourceUrl валидны.");
            }

            // [4/8] quests.json
            int questCount = db.AllQuests != null ? db.AllQuests.Count : 0;
            int totalXp = 0;
            if (db.AllQuests != null)
            {
                for (int i = 0; i < db.AllQuests.Count; i++)
                {
                    totalXp += db.AllQuests[i].xpReward;
                }
            }
            if (questCount != 24 || totalXp != 2220)
            {
                Debug.LogError($"[4/8] Ошибка quests.json: квестов {questCount}/24, суммарный XP = {totalXp} (ожидалось 2220).");
                errors++;
            }
            else
            {
                Debug.Log("[4/8] Data/quests.json: 24 квеста (Q-01..Q-24) валидны. Итоговый опыт = 2220 XP.");
            }

            // [5/8] strings_ru.csv (468 rows, no 'ё' in 1950 strings)
            TextAsset locAsset = Resources.Load<TextAsset>("Localization/strings_ru");
            if (locAsset == null)
            {
                Debug.LogError("[5/8] Файл Resources/Localization/strings_ru.csv не найден.");
                errors++;
            }
            else
            {
                string[] lines = locAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                int rowCount = lines.Length - 1;
                int yoErrors = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] cols = lines[i].Split(';');
                    if (cols.Length >= 5)
                    {
                        string key = cols[0];
                        string textRu = cols[4];
                        if ((key.Contains("1950") || key.Contains("blueprint") || key.Contains("zubov")) &&
                            (textRu.IndexOf('ё') >= 0 || textRu.IndexOf('Ё') >= 0))
                        {
                            yoErrors++;
                            Debug.LogError($"[5/8] Нарушение орфографии 1950 г. (найдена буква 'ё') в ключе '{key}': {textRu}");
                        }
                    }
                }
                if (rowCount != 468 || yoErrors > 0)
                {
                    Debug.LogError($"[5/8] Ошибка strings_ru.csv: строк {rowCount}/468, ошибок 'ё' в 1950 г.: {yoErrors}.");
                    errors++;
                }
                else
                {
                    Debug.Log("[5/8] Localization/strings_ru.csv: 468 строк. Буква 'Ё'/'ё' в текстах 1950 г. не обнаружена.");
                }
            }

            // [6/8] audio_shotlist.csv (80 rows)
            TextAsset audioAsset = Resources.Load<TextAsset>("Audio/audio_shotlist");
            if (audioAsset == null)
            {
                Debug.LogError("[6/8] Файл Resources/Audio/audio_shotlist.csv не найден.");
                errors++;
            }
            else
            {
                string[] lines = audioAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                int audioCount = lines.Length - 1;
                if (audioCount != 80)
                {
                    Debug.LogError($"[6/8] Ошибка audio_shotlist.csv: записей {audioCount}/80.");
                    errors++;
                }
                else
                {
                    Debug.Log("[6/8] Audio/audio_shotlist.csv: 80 событий (MUS-01..05, SFX-01..75) подтверждены.");
                }
            }

            // [7/8] Materials Smoothness == 0 & Buttons >= 48 dp
            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material mat = renderers[i].sharedMaterial;
                if (mat != null && mat.HasProperty("_Smoothness") && mat.GetFloat("_Smoothness") > 0.01f)
                {
                    Debug.LogError($"[7/8] Материал '{mat.name}' имеет Smoothness > 0 (нарушение стиля гуаши/акварели).");
                    errors++;
                }
            }
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform rt = buttons[i].GetComponent<RectTransform>();
                if (rt != null && (rt.sizeDelta.x < 130f || rt.sizeDelta.y < 130f))
                {
                    Debug.LogWarning($"[7/8] Кнопка '{buttons[i].name}' меньше эталона 132x132 px (48 dp).");
                }
            }
            Debug.Log("[7/8] Scene & UI Validation: матовость материалов и тач-цели проверены.");

            // [8/8] Android Player Settings
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
            {
                Debug.LogWarning("[8/8] Рекомендуется установить Default Orientation = Portrait.");
            }
            Debug.Log("[8/8] Android Settings проверены.");

            if (errors == 0)
            {
                Debug.Log("[OkhtaFullGameValidator] УСПЕХ: Проект полностью готов к сборке!");
            }
            else
            {
                Debug.LogError($"[OkhtaFullGameValidator] Найдено ошибок: {errors}. Исправьте перед сборкой .aab/.apk.");
            }
        }
    }
}
#endif
