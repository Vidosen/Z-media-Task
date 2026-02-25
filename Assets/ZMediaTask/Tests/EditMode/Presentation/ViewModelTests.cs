using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Tests.EditMode.Presentation
{
    public class ViewModelTests
    {
        #region PreparationViewModel

        [Test]
        public void PreparationViewModel_RandomizeLeft_UpdatesArmyPreview()
        {
            var vm = CreatePreparationVm();

            vm.RandomizeLeft();

            Assert.AreNotEqual("—", vm.LeftPreview.CurrentValue,
                "Left preview should be updated after randomize.");
            StringAssert.Contains("(Blue)", vm.LeftPreview.CurrentValue);
        }

        [Test]
        public void PreparationViewModel_RandomizeBoth_UpdatesBothPreviews()
        {
            var vm = CreatePreparationVm();

            vm.RandomizeBoth();

            Assert.AreNotEqual("—", vm.LeftPreview.CurrentValue);
            Assert.AreNotEqual("—", vm.RightPreview.CurrentValue);
            StringAssert.Contains("(Blue)", vm.LeftPreview.CurrentValue);
            StringAssert.Contains("(Red)", vm.RightPreview.CurrentValue);
        }

        [Test]
        public void PreparationViewModel_ArmiesChanged_EmitsArmyPairOnRandomize()
        {
            var vm = CreatePreparationVm();
            ArmyPair? received = null;

            using var sub = vm.Armies.Subscribe(pair => received = pair);
            vm.RandomizeBoth();

            Assert.IsNotNull(received, "Armies property should emit after RandomizeBoth.");
            Assert.IsNotNull(received.Value.Left);
            Assert.IsNotNull(received.Value.Right);
        }

        [Test]
        public void PreparationViewModel_StartCommand_ChangesStateToRunning()
        {
            var vm = CreatePreparationVm();
            ArmyPair? received = null;

            using var sub = vm.StartRequested.Subscribe(pair => received = pair);
            vm.RandomizeBoth();
            vm.RequestStart();

            Assert.IsNotNull(received,
                "StartRequested should emit ArmyPair, which triggers state change to Running.");
            Assert.IsNotNull(received.Value.Left);
            Assert.IsNotNull(received.Value.Right);
        }

        #endregion

        #region BattleHudViewModel

        [Test]
        public void BattleHudViewModel_UpdatesAliveCounters()
        {
            var vm = new BattleHudViewModel();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, hp: 100),
                MakeUnit(2, ArmySide.Left, hp: 100),
                MakeUnit(3, ArmySide.Left, hp: 0), // dead
                MakeUnit(4, ArmySide.Right, hp: 100),
                MakeUnit(5, ArmySide.Right, hp: 0) // dead
            };
            var context = new BattleContext(units, 65f, null, MakeDefaultWrathMeters());

            vm.UpdateFromContext(context);

            Assert.AreEqual(2, vm.LeftAlive.CurrentValue);
            Assert.AreEqual(1, vm.RightAlive.CurrentValue);
            Assert.AreEqual("1:05", vm.TimerText.CurrentValue);

            vm.Dispose();
        }

        [Test]
        public void BattleHudViewModel_BalanceRatio_ComputesCorrectly()
        {
            var vm = new BattleHudViewModel();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, hp: 100),
                MakeUnit(2, ArmySide.Left, hp: 100),
                MakeUnit(3, ArmySide.Left, hp: 100),
                MakeUnit(4, ArmySide.Right, hp: 100),
            };
            var context = new BattleContext(units, 0f, null, MakeDefaultWrathMeters());

            vm.UpdateFromContext(context);

            Assert.AreEqual(0.75f, vm.BalanceRatio.CurrentValue, 0.001f,
                "3 left alive / (3 + 1) total = 0.75");

            vm.Dispose();
        }

        [Test]
        public void BattleHudViewModel_BalanceRatio_DefaultsToHalfWhenAllDead()
        {
            var vm = new BattleHudViewModel();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, hp: 0),
                MakeUnit(2, ArmySide.Right, hp: 0),
            };
            var context = new BattleContext(units, 0f, null, MakeDefaultWrathMeters());

            vm.UpdateFromContext(context);

            Assert.AreEqual(0.5f, vm.BalanceRatio.CurrentValue, 0.001f,
                "When all units dead, ratio should default to 0.5");

            vm.Dispose();
        }

        #endregion

        #region WrathViewModel

        [Test]
        public void WrathViewModel_ShowsChargeProgress_AndCanCastState()
        {
            var vm = new WrathViewModel();

            vm.UpdateFromMeter(new WrathMeter(50, 100));
            Assert.AreEqual(0.5f, vm.ChargeNormalized.CurrentValue, 0.001f);
            Assert.IsFalse(vm.CanCast.CurrentValue);

            vm.UpdateFromMeter(new WrathMeter(100, 100));
            Assert.AreEqual(1f, vm.ChargeNormalized.CurrentValue, 0.001f);
            Assert.IsTrue(vm.CanCast.CurrentValue);

            vm.Dispose();
        }

        [Test]
        public void WrathViewModel_TracksDragging()
        {
            var vm = new WrathViewModel();

            Assert.IsFalse(vm.IsDragging.CurrentValue);

            vm.SetDragging(true);
            Assert.IsTrue(vm.IsDragging.CurrentValue);

            vm.SetDragging(false);
            Assert.IsFalse(vm.IsDragging.CurrentValue);

            vm.Dispose();
        }

        #endregion

        #region ResultViewModel

        [Test]
        public void ResultViewModel_ShowsWinnerAndReturnToMenu()
        {
            var vm = new ResultViewModel();
            var returnCalled = false;

            using var sub = vm.ReturnRequested.Subscribe(_ => returnCalled = true);

            vm.SetWinner(ArmySide.Left);
            Assert.AreEqual("Blue army wins!", vm.WinnerText.CurrentValue);

            vm.SetWinner(null);
            Assert.AreEqual("Draw!", vm.WinnerText.CurrentValue);

            vm.RequestReturn();
            Assert.IsTrue(returnCalled);

            vm.Dispose();
        }

        #endregion

        #region Helpers

        private static PreparationViewModel CreatePreparationVm()
        {
            var catalog = new FakeTraitCatalog();
            var weightCatalog = new FakeWeightCatalog();
            var calculator = new StatsCalculator(catalog);
            var factory = new ArmyFactory(calculator, weightCatalog);
            var randomProvider = new FakeRandomProvider();
            var useCase = new ArmyRandomizationUseCase(factory, randomProvider);
            return new PreparationViewModel(useCase);
        }

        private static BattleUnitRuntime MakeUnit(int unitId, ArmySide side, int hp)
        {
            var pos = new BattlePoint(0f, 0f);
            var movement = new MovementAgentState(
                unitId, hp > 0, 5f, pos, null, Array.Empty<BattlePoint>(), null);
            var combat = new CombatUnitState(unitId, pos, hp, 10, 1, 0f);
            return new BattleUnitRuntime(unitId, side, UnitShape.Cube, UnitSize.Small, UnitColor.Blue, movement, combat);
        }

        private static Dictionary<ArmySide, WrathMeter> MakeDefaultWrathMeters()
        {
            return new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, 100),
                [ArmySide.Right] = new WrathMeter(0, 100)
            };
        }

        private sealed class FakeTraitCatalog : IUnitTraitCatalog
        {
            public StatModifier GetShapeModifier(UnitShape shape) => new(100, 10, 0, 0);
            public StatModifier GetSizeModifier(UnitSize size) => new(0, 0, 0, 0);
            public StatModifier GetColorModifier(UnitColor color) => new(0, 0, 0, 0);
        }

        private sealed class FakeWeightCatalog : IUnitTraitWeightCatalog
        {
            public int GetShapeWeight(UnitShape shape) => 1;
            public int GetSizeWeight(UnitSize size) => 1;
            public int GetColorWeight(UnitColor color) => 1;
        }

        private sealed class FakeRandomProvider : IRandomProvider
        {
            private System.Random _rng = new(42);

            public void Reset(int seed) => _rng = new System.Random(seed);
            public int NextInt(int minInclusive, int maxExclusive) => _rng.Next(minInclusive, maxExclusive);
        }

        #endregion
    }
}
