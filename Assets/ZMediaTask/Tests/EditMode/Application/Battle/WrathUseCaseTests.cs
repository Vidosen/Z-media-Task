using NUnit.Framework;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Application.Battle
{
    public class WrathUseCaseTests
    {
        private static readonly WrathConfig Config = new(
            chargePerKill: 20,
            maxCharge: 100,
            radius: 4f,
            damage: 80,
            impactDelaySeconds: 0.35f);

        [Test]
        public void WrathCharge_Increases_OnEnemyKill()
        {
            var useCase = new OnUnitKilledUseCase(ArmySide.Left, Config, new WrathService());
            var meter = new WrathMeter(currentCharge: 0, maxCharge: Config.MaxCharge);

            var next = useCase.OnUnitKilled(meter, killerSide: ArmySide.Left, victimSide: ArmySide.Right);

            Assert.AreEqual(20, next.CurrentCharge);
        }

        [Test]
        public void OnUnitKilledUseCase_DoesNotIncrease_WhenVictimIsFriendly()
        {
            var useCase = new OnUnitKilledUseCase(ArmySide.Left, Config, new WrathService());
            var meter = new WrathMeter(currentCharge: 40, maxCharge: Config.MaxCharge);

            var next = useCase.OnUnitKilled(meter, killerSide: ArmySide.Left, victimSide: ArmySide.Left);

            Assert.AreEqual(40, next.CurrentCharge);
        }

        [Test]
        public void WrathCast_Available_OnlyWhenMeterFull()
        {
            var useCase = CreateTryCastUseCase(new AlwaysValidTargetValidator());
            var meter = new WrathMeter(currentCharge: 99, maxCharge: Config.MaxCharge);

            var result = useCase.TryCast(
                meter,
                controllerSide: ArmySide.Left,
                new BattlePoint(0f, 0f),
                currentTimeSec: 10f);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(WrathCastFailureReason.MeterNotFull, result.Failure);
            Assert.IsNull(result.Command);
        }

        [Test]
        public void WrathCast_ConsumesFullMeter_OnSuccess()
        {
            var useCase = CreateTryCastUseCase(new AlwaysValidTargetValidator());
            var meter = new WrathMeter(currentCharge: 100, maxCharge: Config.MaxCharge);

            var result = useCase.TryCast(
                meter,
                controllerSide: ArmySide.Left,
                new BattlePoint(1f, 2f),
                currentTimeSec: 5f);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.MeterAfter.CurrentCharge);
            Assert.IsTrue(result.Command.HasValue);
            Assert.AreEqual(5.35f, result.Command!.Value.ImpactTimeSec, 0.0001f);
        }

        [Test]
        public void WrathCast_DoesNotConsumeMeter_OnInvalidTarget()
        {
            var useCase = CreateTryCastUseCase(new NeverValidTargetValidator());
            var meter = new WrathMeter(currentCharge: 100, maxCharge: Config.MaxCharge);

            var result = useCase.TryCast(
                meter,
                controllerSide: ArmySide.Left,
                new BattlePoint(3f, 4f),
                currentTimeSec: 1f);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(100, result.MeterAfter.CurrentCharge);
            Assert.AreEqual(WrathCastFailureReason.InvalidTarget, result.Failure);
            Assert.IsNull(result.Command);
        }

        [Test]
        public void WrathCast_NotAvailable_ForAIController()
        {
            var useCase = CreateTryCastUseCase(new AlwaysValidTargetValidator());
            var meter = new WrathMeter(currentCharge: 100, maxCharge: Config.MaxCharge);

            var result = useCase.TryCast(
                meter,
                controllerSide: ArmySide.Right,
                new BattlePoint(0f, 0f),
                currentTimeSec: 2f);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(WrathCastFailureReason.NotOwnerController, result.Failure);
            Assert.AreEqual(100, result.MeterAfter.CurrentCharge);
        }

        private static TryCastWrathUseCase CreateTryCastUseCase(IWrathTargetValidator targetValidator)
        {
            return new TryCastWrathUseCase(
                ArmySide.Left,
                Config,
                new WrathService(),
                targetValidator);
        }

        private sealed class AlwaysValidTargetValidator : IWrathTargetValidator
        {
            public bool IsValid(BattlePoint point) => true;
        }

        private sealed class NeverValidTargetValidator : IWrathTargetValidator
        {
            public bool IsValid(BattlePoint point) => false;
        }
    }
}
