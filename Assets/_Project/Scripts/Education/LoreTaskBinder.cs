using System;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public class LoreTaskBinder
    {
        private readonly ContentDatabase _database;
        private readonly TaskGenerator _generator;

        public LoreTaskBinder(ContentDatabase database, TaskGenerator generator)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public GeneratedTaskInstance GeneratePracticeForLore(LoreCardData card, LoreJournal journal, string templateId, int seed)
        {
            if (card == null || journal == null || !journal.IsCardVerified(card)) return null;
            if (!_database.TryGetTemplate(templateId, out TemplateData tpl)) return null;
            return _generator.GenerateFromTemplate(tpl, seed);
        }
    }
}
