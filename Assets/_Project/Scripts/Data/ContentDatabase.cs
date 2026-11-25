using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Data
{
    public class ContentDatabase : MonoBehaviour
    {
        public static ContentDatabase Instance { get; private set; }

        private readonly Dictionary<string, string> localizationDictionary = new Dictionary<string, string>();

        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadLocalization();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadLocalization()
        {
            localizationDictionary.Clear();
            TextAsset csvFile = Resources.Load<TextAsset>("Localization/strings_ru");

            if (csvFile == null)
            {
                Debug.LogWarning("[ContentDatabase] Файл Localization/strings_ru не найден в Resources!");
                return;
            }

            string[] lines = csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(';');
                if (parts.Length >= 4)
                {
                    string key = parts[0].Trim();
                    string text = parts[3].Trim().Replace("\\n", "\n");
                    if (!localizationDictionary.ContainsKey(key))
                    {
                        localizationDictionary.Add(key, text);
                    }
                }
            }

            IsLoaded = true;
            Debug.Log($"[ContentDatabase] Успешно загружено {localizationDictionary.Count} строк локализации.");
        }

        public string GetLocalizedString(string key, string fallback = "")
        {
            if (localizationDictionary.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }
    }
}
