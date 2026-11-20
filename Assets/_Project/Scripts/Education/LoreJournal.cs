using System;
using System.Collections.Generic;
using TimeMachine.Core;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public class LoreJournal
    {
        private readonly ContentDatabase _database;
        private readonly GameState _state;

        public LoreJournal(ContentDatabase database, GameState state)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public bool IsCardVerified(LoreCardData card)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.sourceUrl)) return false;
            return card.sourceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   card.sourceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryUnlockCard(string loreId)
        {
            if (string.IsNullOrWhiteSpace(loreId)) return false;
            if (!_database.TryGetLoreCard(loreId, out LoreCardData card)) return false;
            if (!IsCardVerified(card)) return false;

            if (!_state.Data.unlockedLoreIds.Contains(loreId))
            {
                _state.Data.unlockedLoreIds.Add(loreId);
                GameEventBus.RaiseLoreCardUnlocked(loreId);
            }
            return true;
        }

        public void GetUnlockedVerifiedCards(List<LoreCardData> outputBuffer)
        {
            if (outputBuffer == null) return;
            outputBuffer.Clear();
            var ids = _state.Data.unlockedLoreIds;
            for (int i = 0; i < ids.Count; i++)
            {
                if (_database.TryGetLoreCard(ids[i], out LoreCardData card) && IsCardVerified(card))
                {
                    outputBuffer.Add(card);
                }
            }
        }
    }
}
