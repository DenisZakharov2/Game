using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Data
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

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

        public bool HasItem(string itemId)
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) return false;
            return GameState.Instance.CurrentData.inventory.Contains(itemId);
        }

        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            if (GameState.Instance != null && GameState.Instance.CurrentData != null)
            {
                GameState.Instance.AddInventoryItem(itemId);
                Debug.Log($"[TimeMachine] [InventorySystem] Добавлен предмет в инвентарь: {itemId}");
            }
        }

        public bool RemoveItem(string itemId)
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) return false;

            if (GameState.Instance.CurrentData.inventory.Contains(itemId))
            {
                GameState.Instance.CurrentData.inventory.Remove(itemId);
                GameState.Instance.SaveCurrentState();
                Debug.Log($"[TimeMachine] [InventorySystem] Удален предмет из инвентаря: {itemId}");
                return true;
            }
            return false;
        }

        public List<string> GetAllItems()
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) 
                return new List<string>();

            return new List<string>(GameState.Instance.CurrentData.inventory);
        }
    }
}
