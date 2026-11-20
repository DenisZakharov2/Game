using NUnit.Framework;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;
using TimeMachine.Education;
using TimeMachine.Gameplay;
using TimeMachine.Quests;

namespace TimeMachine.Tests.EditMode
{
    [TestFixture]
    public class FullCampaignIntegrationTests
    {
        [Test]
        public void FullCampaign_Q01_To_Q24_CompletesChain_Grants2220Xp_AndVerifiesAllLore()
        {
            var db = new ContentDatabase();
            db.LoadFromResources();

            var state = new GameState(GameState.CreateNewDefaultState());
            var inventory = new InventorySystem(state);
            var loreJournal = new LoreJournal(db, state);
            var saveSystem = new SaveSystem();
            var runner = new QuestRunner(db, state, inventory, loreJournal, saveSystem);
            runner.Initialize();

            for (int i = 1; i <= 24; i++)
            {
                string qId = i < 10 ? $"Q-0{i}" : $"Q-{i}";
                Assert.IsTrue(runner.TryStartQuest(qId), $"Не удалось запустить {qId}");
                Assert.IsTrue(runner.CompleteQuest(qId), $"Не удалось завершить {qId}");
            }

            Assert.AreEqual(2220, state.Data.totalXp);
            Assert.AreEqual(25, state.Data.unlockedLoreIds.Count);
            Assert.IsTrue(state.Data.isStoryCompleted);
            saveSystem.DeleteSave();
        }

        [Test]
        public void AssemblyPuzzle_Chapter3_EnforcesBlueprintSequence_AndBuildsZarya7()
        {
            var go = new GameObject("TestAssembly");
            var puzzle = go.AddComponent<AssemblyPuzzleController>();
            var state = new GameState(GameState.CreateNewDefaultState());
            var inventory = new InventorySystem(state);
            puzzle.BindSystems(state, inventory, null, null);

            // Wrong order rejected without penalty
            Assert.IsFalse(puzzle.TryPlacePart(0, GameConstants.ItemIds.PartQuartzStabilizer));

            // Correct sequence SLOT-01 .. SLOT-05
            Assert.IsTrue(puzzle.TryPlacePart(0, GameConstants.ItemIds.PartChronoCore));
            Assert.IsTrue(puzzle.TryPlacePart(1, GameConstants.ItemIds.PartMicrowaveMotor));
            GameEventBus.RaiseTaskCompleted("T-MATH-11", true);
            Assert.IsTrue(puzzle.TryPlacePart(2, GameConstants.ItemIds.PartMagneticCoil));
            Assert.IsTrue(puzzle.TryPlacePart(3, GameConstants.ItemIds.PartCapacitorBlock));
            GameEventBus.RaiseTaskCompleted("T-RUS-11", true);
            Assert.IsTrue(puzzle.TryPlacePart(4, GameConstants.ItemIds.PartQuartzStabilizer));

            Assert.IsTrue(puzzle.IsFullyAssembled);
            Assert.AreEqual(3000, puzzle.GetInstalledMassGrams());
            Assert.IsTrue(inventory.HasItem(GameConstants.ItemIds.Zarya7Assembled));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EraSwitcher_Switches2027To1950AndBack_KeepingAnchorsAlwaysActive()
        {
            var root = new GameObject("EraRoot");
            var anchors = new GameObject("Layer_Anchors");
            var l2027 = new GameObject("Layer_2027");
            var l1950 = new GameObject("Layer_1950");

            var switcher = root.AddComponent<EraSwitcher>();
            switcher.ConfigureContainers(anchors, l2027, l1950);

            var state = new GameState(GameState.CreateNewDefaultState());
            var inventory = new InventorySystem(state);
            switcher.BindSystems(state, inventory, null);

            // Blocked before Zarya-7 assembly
            Assert.IsFalse(switcher.TrySwitchEra(EraLayerId.Era1950));
            Assert.IsTrue(anchors.activeSelf);
            Assert.IsTrue(l2027.activeSelf);

            // Allowed after Zarya-7 assembly
            state.Data.isZarya7Assembled = true;
            Assert.IsTrue(switcher.TrySwitchEra(EraLayerId.Era1950));
            Assert.IsTrue(anchors.activeSelf);
            Assert.IsFalse(l2027.activeSelf);
            Assert.IsTrue(l1950.activeSelf);

            // Return to 2027
            Assert.IsTrue(switcher.TrySwitchEra(EraLayerId.Era2027));
            Assert.IsTrue(anchors.activeSelf);
            Assert.IsTrue(l2027.activeSelf);
            Assert.IsFalse(l1950.activeSelf);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(anchors);
            Object.DestroyImmediate(l2027);
            Object.DestroyImmediate(l1950);
        }

        [Test]
        public void FreeRoam_TemplatesAndSkillTree_WorkAfterCampaignCompletion()
        {
            var db = new ContentDatabase();
            db.LoadFromResources();
            var state = new GameState(GameState.CreateNewDefaultState());
            state.AddXp(1200);

            Assert.IsTrue(state.TryUpgradeSkill(SkillBranch.Knowledge, 100));
            Assert.IsTrue(state.TryUpgradeSkill(SkillBranch.Knowledge, 100));
            Assert.IsTrue(state.TryUpgradeSkill(SkillBranch.Knowledge, 100));
            Assert.AreEqual(3, state.GetSkillLevel(SkillBranch.Knowledge));

            var go = new GameObject("TestFreeRoam");
            var freeRoam = go.AddComponent<FreeRoamController>();
            freeRoam.BindSystems(state, db, new LoreJournal(db, state), new TaskGenerator(), null);
            freeRoam.ActivateFreeRoam();

            Assert.IsTrue(freeRoam.TryStartMathPracticeTask(out GeneratedTaskInstance mathTask));
            Assert.IsNotNull(mathTask);
            Assert.IsTrue(freeRoam.TryStartRussianPracticeTask(out GeneratedTaskInstance rusTask));
            Assert.IsNotNull(rusTask);

            Object.DestroyImmediate(go);
        }
    }
}
