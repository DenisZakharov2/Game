using System;
using UnityEngine;

namespace TimeMachine.Core
{
    public static class GameEventBus
    {
        public static event Action<string> OnQuestStateChanged;
        public static event Action OnInventoryChanged;
        public static event Action<int, int> OnXpChanged;
        public static event Action<SkillBranch, int> OnSkillUpgraded;
        public static event Action<string> OnLoreCardUnlocked;
        public static event Action<string, string> OnCompanionSpeak;
        public static event Action<string> OnTaskRequested;
        public static event Action<string, bool> OnTaskCompleted;
        public static event Action<IInteractable> OnInteractableFocused;
        public static event Action OnInteractableCleared;
        public static event Action<Vector3, float> OnPlayerTeleported;

        public static void RaiseQuestStateChanged(string questId) => OnQuestStateChanged?.Invoke(questId);
        public static void RaiseInventoryChanged() => OnInventoryChanged?.Invoke();
        public static void RaiseXpChanged(int delta, int totalXp) => OnXpChanged?.Invoke(delta, totalXp);
        public static void RaiseSkillUpgraded(SkillBranch branch, int newLevel) => OnSkillUpgraded?.Invoke(branch, newLevel);
        public static void RaiseLoreCardUnlocked(string loreId) => OnLoreCardUnlocked?.Invoke(loreId);
        public static void RaiseCompanionSpeak(string speakerId, string message) => OnCompanionSpeak?.Invoke(speakerId, message);
        public static void RaiseTaskRequested(string taskId) => OnTaskRequested?.Invoke(taskId);
        public static void RaiseTaskCompleted(string taskId, bool solvedNormally) => OnTaskCompleted?.Invoke(taskId, solvedNormally);
        public static void RaiseInteractableFocused(IInteractable target) => OnInteractableFocused?.Invoke(target);
        public static void RaiseInteractableCleared() => OnInteractableCleared?.Invoke();
        public static void RaisePlayerTeleported(Vector3 position, float yaw) => OnPlayerTeleported?.Invoke(position, yaw);

        public static void ClearAllListeners()
        {
            OnQuestStateChanged = null;
            OnInventoryChanged = null;
            OnXpChanged = null;
            OnSkillUpgraded = null;
            OnLoreCardUnlocked = null;
            OnCompanionSpeak = null;
            OnTaskRequested = null;
            OnTaskCompleted = null;
            OnInteractableFocused = null;
            OnInteractableCleared = null;
            OnPlayerTeleported = null;
        }
    }
}
