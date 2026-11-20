using System;
using System.Collections.Generic;
using TimeMachine.Core;

namespace TimeMachine.Data
{
    public class InventorySystem
    {
        private readonly GameState _state;

        private static readonly Dictionary<string, int> PartMassGrams = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            { GameConstants.ItemIds.PartQuartzStabilizer, 850 },
            { GameConstants.ItemIds.PartMagneticCoil, 650 },
            { GameConstants.ItemIds.PartCapacitorBlock, 400 },
            { GameConstants.ItemIds.PartChronoCore, 600 },
            { GameConstants.ItemIds.PartMicrowaveMotor, 500 }
        };

        public InventorySystem(GameState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || count <= 0) return;

            var list = _state.Data.inventory;
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].itemId, itemId, StringComparison.Ordinal))
                {
                    list[i].count += count;
                    GameEventBus.RaiseInventoryChanged();
                    return;
                }
            }

            list.Add(new InventoryItemEntry { itemId = itemId, count = count });
            GameEventBus.RaiseInventoryChanged();
        }

        public bool HasItem(string itemId, int minCount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            var list = _state.Data.inventory;
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].itemId, itemId, StringComparison.Ordinal))
                {
                    return list[i].count >= minCount;
                }
            }
            return false;
        }

        public int GetZarya7TotalMassGrams()
        {
            if (HasItem(GameConstants.ItemIds.Zarya7Assembled) || (_state.Data != null && _state.Data.isZarya7Assembled))
            {
                return GameConstants.GameplayTuning.TotalZarya7MassGrams;
            }

            int sum = 0;
            foreach (var kvp in PartMassGrams)
            {
                if (HasItem(kvp.Key))
                {
                    sum += kvp.Value;
                }
            }
            return sum;
        }

        public bool HasAllZarya7Components()
        {
            var parts = GameConstants.ItemIds.RequiredZarya7Parts;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!HasItem(parts[i])) return false;
            }
            return true;
        }
    }
}
