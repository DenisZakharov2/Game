#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TimeMachine.EditorTools
{
    public static class ProjectFolderSetup
    {
        [MenuItem("TimeMachine/Setup Project Folders")]
        public static void CreateRequiredFolders()
        {
            string[] folders = new string[]
            {
                "Assets/_Project/Art/Materials",
                "Assets/_Project/Art/Fonts",
                "Assets/_Project/Art/Textures",
                "Assets/_Project/Prefabs/Characters",
                "Assets/_Project/Prefabs/Environment",
                "Assets/_Project/Audio/Music",
                "Assets/_Project/Audio/SFX",
                "Assets/_Project/Resources/Localization",
                "Assets/_Project/Resources/GameData"
            };

            int createdCount = 0;
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    createdCount++;
                    Debug.Log($"[TimeMachine] Создана папка: {folder}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TimeMachine] Структура папок обновлена. Создано новых директорий: {createdCount}");
        }
    }
}
#endif
