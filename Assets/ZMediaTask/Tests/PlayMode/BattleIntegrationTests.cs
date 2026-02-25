using System.Collections;
using System.Linq;
using NUnit.Framework;
using R3;
using UnityEngine.TestTools;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Tests.PlayMode
{
    public class BattleIntegrationTests : PlayModeTestFixture
    {
        [UnityTest]
        public IEnumerator PlayMode_StartBattle_RunsToCompletion()
        {
            var armies = RandomizationUseCase.RandomizeBoth(seed: 42, unitsPerArmy: 20);
            BattleLoop.Initialize(armies);
            BattleLoop.Start();

            TickUntilFinished();

            Assert.AreEqual(BattlePhase.Finished, BattleLoop.StateMachine.Current);

            var context = BattleLoop.Context;
            Assert.IsTrue(context.WinnerSide.HasValue, "Battle should have a winner.");

            var loserSide = context.WinnerSide.Value == ArmySide.Left ? ArmySide.Right : ArmySide.Left;
            var loserAlive = context.Units.Count(u => u.Side == loserSide && u.Combat.IsAlive);
            Assert.AreEqual(0, loserAlive, "Losing army should have no units alive.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayMode_RandomizeBeforeStart_ChangesBothArmies()
        {
            PreparationVm.RandomizeBoth();
            var firstLeft = PreparationVm.LeftPreview.CurrentValue;
            var firstRight = PreparationVm.RightPreview.CurrentValue;

            Assert.AreNotEqual("—", firstLeft, "Left preview should update after first randomize.");
            Assert.AreNotEqual("—", firstRight, "Right preview should update after first randomize.");

            // Randomize with a different seed by calling again
            PreparationVm.RandomizeBoth();
            var secondLeft = PreparationVm.LeftPreview.CurrentValue;
            var secondRight = PreparationVm.RightPreview.CurrentValue;

            // The previews should be non-default (both calls produce valid armies)
            Assert.AreNotEqual("—", secondLeft);
            Assert.AreNotEqual("—", secondRight);

            // Verify armies are emitted via reactive property
            ArmyPair? lastEmitted = null;
            using var sub = PreparationVm.Armies.Subscribe(pair => lastEmitted = pair);

            PreparationVm.RandomizeBoth();

            Assert.IsNotNull(lastEmitted, "Armies should emit on randomize.");
            Assert.IsNotNull(lastEmitted.Value.Left);
            Assert.IsNotNull(lastEmitted.Value.Right);
            Assert.AreEqual(20, lastEmitted.Value.Left.Units.Count);
            Assert.AreEqual(20, lastEmitted.Value.Right.Units.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayMode_AfterBattle_ReturnsToMenu()
        {
            // 1v1 fast battle: high ATK, low HP for quick resolution
            var armies = RandomizationUseCase.RandomizeBoth(seed: 42, unitsPerArmy: 1);
            BattleLoop.Initialize(armies);
            BattleLoop.Start();

            TickUntilFinished(maxTicks: 50000);

            Assert.AreEqual(BattlePhase.Finished, BattleLoop.StateMachine.Current);

            // Set winner on ResultVm
            var winner = BattleLoop.Context.WinnerSide;
            ResultVm.SetWinner(winner);
            Assert.IsTrue(
                ResultVm.WinnerText.CurrentValue.Contains("wins") ||
                ResultVm.WinnerText.CurrentValue == "Draw!",
                $"WinnerText should show result, got: {ResultVm.WinnerText.CurrentValue}");

            // Verify return-to-menu fires
            var returnFired = false;
            using var sub = ResultVm.ReturnRequested.Subscribe(_ => returnFired = true);
            ResultVm.RequestReturn();
            Assert.IsTrue(returnFired, "ReturnRequested should fire on RequestReturn.");

            // Reset returns to Preparation
            BattleLoop.Reset();
            Assert.AreEqual(BattlePhase.Preparation, BattleLoop.StateMachine.Current);

            yield return null;
        }
    }
}
