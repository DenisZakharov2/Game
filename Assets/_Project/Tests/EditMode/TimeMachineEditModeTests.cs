using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Quests;

namespace TimeMachine.Tests.EditMode
{
    public class TimeMachineEditModeTests
    {
        [Test]
        public void AnswerNormalizer_HandlesCaseYoFractionsNumberWordsAndUnits()
        {
            Assert.AreEqual("еще один переулок", AnswerNormalizer.NormalizeText("  ЕЩЁ   один   переулок  "));

            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue("½", out double half1));
            Assert.AreEqual(0.5, half1, 0.001);

            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue("1/2", out double half2));
            Assert.AreEqual(0.5, half2, 0.001);

            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue("0,5", out double half3));
            Assert.AreEqual(0.5, half3, 0.001);

            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue("семьдесят", out double seventy));
            Assert.AreEqual(70.0, seventy, 0.001);

            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue("сто пятьдесят метров", out double meters));
            Assert.AreEqual(150.0, meters, 0.001);
        }

        [Test]
        public void TaskEngine_EvaluatesAllSixAnswerTypes_WithoutPenalties()
        {
            var db = new ContentDatabase();
            var state = new GameState();
            state.AddXp(100);
            var engine = new TaskEngine(db, state);

            // 1. numberWithUnit
            var mathTask = new TaskData
            {
                id = "T-MATH-01",
                subject = "math",
                fgosTopic = "Единицы длины",
                answerType = "numberWithUnit",
                correctAnswer = "150 м",
                acceptedVariants = new List<string> { "150", "150 метров", "сто пятьдесят метров" }
            };
            Assert.IsFalse(engine.SubmitAnswer(mathTask, "15000 см").IsCorrect);
            Assert.AreEqual(100, state.Data.totalXp, "XP не должен уменьшаться при ошибке");
            Assert.IsTrue(engine.SubmitAnswer(mathTask, "сто пятьдесят метров").IsCorrect);

            // 2. text
            var textTask = new TaskData
            {
                id = "T-RUS-03",
                subject = "russian",
                fgosTopic = "Три склонения",
                answerType = "text",
                correctAnswer = "площадь"
            };
            Assert.IsTrue(engine.SubmitAnswer(textTask, "  ПЛОЩАДЬ ").IsCorrect);

            // 3. choice
            var choiceTask = new TaskData
            {
                id = "T-RUS-01",
                subject = "russian",
                fgosTopic = "Буквы е и ё",
                answerType = "choice",
                correctAnswer = "А) В 1950 году в типографиях букву «ё» почти не печатали"
            };
            Assert.IsTrue(engine.SubmitAnswer(choiceTask, "А").IsCorrect);

            // 4. wordAssembly
            var assemblyTask = new TaskData
            {
                id = "T-RUS-02",
                subject = "russian",
                fgosTopic = "Состав слова",
                answerType = "wordAssembly",
                correctAnswer = "под-став-к-а"
            };
            Assert.IsTrue(engine.SubmitAnswer(assemblyTask, "подставка").IsCorrect);

            // 5. stress
            var stressTask = new TaskData
            {
                id = "T-RUS-05",
                subject = "russian",
                fgosTopic = "Орфоэпия",
                answerType = "stress",
                correctAnswer = "инструмЕнт"
            };
            Assert.IsFalse(engine.SubmitAnswer(stressTask, "Инструмент").IsCorrect);
            Assert.IsTrue(engine.SubmitAnswer(stressTask, "инструмЕнт").IsCorrect);

            // 6. sequence
            var sequenceTask = new TaskData
            {
                id = "T-RUS-07",
                subject = "russian",
                fgosTopic = "Последовательность действий",
                answerType = "sequence",
                correctAnswer = "Найти устой -> Сверить метку -> Вынуть кирпичи -> Достать деталь"
            };
            Assert.IsTrue(engine.SubmitAnswer(sequenceTask, "Найти устой -> Сверить метку -> Вынуть кирпичи -> Достать деталь").IsCorrect);
            Assert.AreEqual(6, state.Data.attemptRecords.Count);
        }

        [Test]
        public void TaskEngine_LessonFreeMode_CompletesWithoutPenalty()
        {
            var db = new ContentDatabase();
            var state = new GameState();
            var engine = new TaskEngine(db, state);
            var task = new TaskData
            {
                id = "T-MATH-02",
                subject = "math",
                fgosTopic = "Доли",
                answerType = "numberWithUnit",
                correctAnswer = "75 оборотов",
                explanation = "60 : 4 * 5 = 75"
            };

            TaskEvaluationResult res = engine.CompleteWithLessonFreeMode(task);
            Assert.IsTrue(res.IsCorrect);
            Assert.IsTrue(state.Data.attemptRecords[0].solvedInLessonFreeMode);
        }

        [Test]
        public void TaskGenerator_IsDeterministicAndAvoidsImmediateContextRepeat()
        {
            var db = new ContentDatabase();
            var generator = new TaskGenerator(db);
            var template = new TemplateData
            {
                id = "TPL-MATH-01",
                subject = "math",
                fgosTopic = "Скорость, время, путь",
                answerType = "numberWithUnit",
                parameters = new List<TemplateParamDef>
                {
                    new TemplateParamDef { name = "S", min = 180, max = 720, step = 60 },
                    new TemplateParamDef { name = "t", min = 3, max = 9, step = 1 }
                },
                constraintRule = "S % t == 0",
                answerFormula = "S / t",
                unit = "м/мин",
                contextVariants = new List<string>
                {
                    "Вариант 1: {S} м за {t} мин.",
                    "Вариант 2: {S} м за {t} мин."
                }
            };

            TaskData first = generator.GenerateFromTemplate(template, 42, QuestIds.Q06, LocationIds.ZimaLeto);
            TaskData second = generator.GenerateFromTemplate(template, 42, QuestIds.Q06, LocationIds.ZimaLeto);

            Assert.AreNotEqual(first.contextAndQuestion, second.contextAndQuestion);
            Assert.IsTrue(AnswerNormalizer.TryExtractNumericValue(first.correctAnswer, out double speed) && speed > 0);
        }

        [Test]
        public void LoreJournal_BlocksCardsWithoutSourceUrl()
        {
            var db = new ContentDatabase();
            db.LoadFromJsonStrings(
                string.Empty,
                string.Empty,
                "{\"loreCards\":[{\"id\":\"LORE-UNVERIFIED\",\"title\":\"Без ссылки\",\"sourceUrl\":\"\"},{\"id\":\"LORE-01\",\"title\":\"Бомбоубежище\",\"sourceUrl\":\"https://openstreetmap.org\"}]}",
                string.Empty);

            var state = new GameState();
            var journal = new LoreJournal(db, state);

            Assert.IsFalse(journal.TryUnlockCard("LORE-UNVERIFIED"));
            Assert.IsTrue(journal.TryUnlockCard("LORE-01"));
            Assert.AreEqual(1, journal.GetUnlockedCount());
        }

        [Test]
        public void QuestRunner_AdvancesChainAndGrantsRewards()
        {
            var db = new ContentDatabase();
            db.LoadFromJsonStrings(
                string.Empty,
                string.Empty,
                "{\"loreCards\":[{\"id\":\"LORE-01\",\"title\":\"Бомбоубежище\",\"sourceUrl\":\"https://openstreetmap.org\"}]}",
                "{\"quests\":[{\"id\":\"Q-01\",\"requiredTaskIds\":[\"T-RUS-01\",\"T-MATH-01\"],\"rewardItemIds\":[\"item_note_1950\",\"part_quartz_stabilizer\"],\"rewardLoreId\":\"LORE-01\",\"rewardXp\":50,\"nextQuestId\":\"Q-02\"}]}");

            string tempDir = Path.Combine(Path.GetTempPath(), "tm_quest_" + Guid.NewGuid().ToString("N"));
            var saveSystem = new SaveSystem(tempDir, "quest_slot.json");
            var state = new GameState();
            var inv = new InventorySystem(state);
            var lore = new LoreJournal(db, state);
            using var runner = new QuestRunner(db, state, inv, lore, saveSystem);

            try
            {
                Assert.AreEqual(QuestStatus.Available, runner.GetQuestStatus(QuestIds.Q01));
                runner.StartQuest(QuestIds.Q01);
                Assert.AreEqual(QuestStatus.Active, runner.GetQuestStatus(QuestIds.Q01));

                runner.NotifyTaskCompleted("T-RUS-01");
                runner.NotifyTaskCompleted("T-MATH-01");

                Assert.AreEqual(QuestStatus.Completed, runner.GetQuestStatus(QuestIds.Q01));
                Assert.AreEqual(QuestStatus.Available, runner.GetQuestStatus(QuestIds.Q02));
                Assert.IsTrue(inv.HasItem(ItemIds.Part1QuartzStabilizer));
                Assert.AreEqual(50, state.Data.totalXp);
            }
            finally
            {
                saveSystem.DeleteSave();
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void SaveSystem_PersistsStateAndMigratesSchema()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "tm_save_" + Guid.NewGuid().ToString("N"));
            var saveSystem = new SaveSystem(tempDir, "test_slot.json");

            try
            {
                var state = new GameState();
                var inventory = new InventorySystem(state);
                inventory.AddItem(ItemIds.Part1QuartzStabilizer);
                inventory.AddItem(ItemIds.MicrowaveMotor);
                state.AddXp(120);
                Assert.IsTrue(state.TryUpgradeSkill(SkillBranch.Knowledge));

                saveSystem.Save(state);

                GameState loaded = saveSystem.LoadOrCreate();
                var loadedInv = new InventorySystem(loaded);
                Assert.AreEqual(120, loaded.Data.totalXp);
                Assert.AreEqual(1, loaded.GetSkillLevel(SkillBranch.Knowledge));
                Assert.IsTrue(loadedInv.HasItem(ItemIds.Part1QuartzStabilizer));
                Assert.IsTrue(loadedInv.HasItem(ItemIds.MicrowaveMotor));

                File.WriteAllText(saveSystem.PrimaryFilePath, "{\"schemaVersion\":0,\"totalXp\":55}");
                GameState migrated = saveSystem.LoadOrCreate();
                Assert.AreEqual(GameplayTuning.CurrentSaveSchemaVersion, migrated.Data.schemaVersion);
                Assert.AreEqual(55, migrated.Data.totalXp);
                Assert.AreEqual(24, migrated.Data.quests.Count);
            }
            finally
            {
                saveSystem.DeleteSave();
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }
}
