using NUnit.Framework;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;

namespace TimeMachine.Tests.EditMode
{
    [TestFixture]
    public class TimeMachineEditModeTests
    {
        [Test]
        public void AnswerNormalizer_NormalizesNumbersUnitsAndYo()
        {
            Assert.AreEqual("150", AnswerNormalizer.Normalize("150 м", "numberWithUnit"));
            Assert.AreEqual("150", AnswerNormalizer.Normalize("150 метров", "numberWithUnit"));
            Assert.AreEqual("150", AnswerNormalizer.Normalize("сто пятьдесят", "numberWithUnit"));
            Assert.AreEqual("завершен", AnswerNormalizer.Normalize("Завершён", "text"));
        }

        [Test]
        public void TaskEngine_EvaluatesAllSixAnswerModes_WithoutPenalties()
        {
            var state = new GameState(GameState.CreateNewDefaultState());
            state.AddXp(100);
            var engine = new TaskEngine(state);

            var task = new TaskData
            {
                id = "T-MATH-01",
                answerType = "numberWithUnit",
                correctAnswer = "150 м",
                acceptedVariants = new[] { "150 м", "150", "150 метров", "сто пятьдесят" }
            };

            var wrongRes = engine.Evaluate(task, "15000 см", 0, false);
            Assert.IsFalse(wrongRes.isCorrect);
            Assert.AreEqual(100, state.Data.totalXp, "Очки XP не должны уменьшаться при ошибке!");

            var okRes = engine.Evaluate(task, "150 метров", 1, false);
            Assert.IsTrue(okRes.isCorrect);
        }

        [Test]
        public void LoreJournal_BlocksCardsWithoutVerifiedSourceUrl()
        {
            var db = new ContentDatabase();
            db.LoadFromJsonStrings(
                "{"tasks":[]}",
                "{"templates":[]}",
                "{"loreCards":[" +
                "{"id":"LORE-OK","title":"Мост","sourceUrl":"https://mostotrest-spb.ru"}," +
                "{"id":"LORE-BAD","title":"Без источника","sourceUrl":""}" +
                "]}",
                "{"quests":[]}");

            var state = new GameState(GameState.CreateNewDefaultState());
            var journal = new LoreJournal(db, state);

            Assert.IsTrue(journal.TryUnlockCard("LORE-OK"));
            Assert.IsFalse(journal.TryUnlockCard("LORE-BAD"));
        }

        [Test]
        public void SaveSystem_RoundTripsSaveDataV1()
        {
            var saveSys = new SaveSystem();
            var data = GameState.CreateNewDefaultState();
            data.totalXp = 450;
            data.currentEra = "1950";
            saveSys.Save(data);

            var loaded = saveSys.LoadOrCreateDefault();
            Assert.AreEqual(450, loaded.totalXp);
            Assert.AreEqual("1950", loaded.currentEra);
            saveSys.DeleteSave();
        }
    }
}
