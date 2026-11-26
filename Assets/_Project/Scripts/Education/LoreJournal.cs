using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public class LoreJournal : MonoBehaviour
    {
        public static LoreJournal Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsUnlocked(string loreId)
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) return false;
            return GameState.Instance.CurrentData.unlockedLoreIds.Contains(loreId);
        }

        public void UnlockLore(string loreId)
        {
            if (string.IsNullOrEmpty(loreId)) return;
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) return;

            if (!GameState.Instance.CurrentData.unlockedLoreIds.Contains(loreId))
            {
                GameState.Instance.CurrentData.unlockedLoreIds.Add(loreId);
                GameState.Instance.SaveCurrentState();
                Debug.Log($"[TimeMachine] [LoreJournal] Открыта краеведческая справка: {loreId}");
            }
        }

        public List<string> GetUnlockedLoreIds()
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null)
                return new List<string>();

            return new List<string>(GameState.Instance.CurrentData.unlockedLoreIds);
        }
    }
}
