using NUnit.Framework;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Infrastructure.Config.Combat;

namespace ZMediaTask.Tests.EditMode.Presentation
{
    public class WrathDragInputTests
    {
        private static readonly ArenaBounds TestBounds = new(
            minX: -15f, maxX: 15f, minZ: -20f, maxZ: 20f);

        [Test]
        public void WrathDragInput_OnRelease_InsideArena_SendsCastCommand()
        {
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);
            var targetPoint = new BattlePoint(5f, 3f);

            Assert.IsTrue(validator.IsValid(targetPoint),
                "Point inside arena should be valid for wrath cast.");
        }

        [Test]
        public void WrathDragInput_OnRelease_OutsideArena_CancelsCast()
        {
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);
            var targetPoint = new BattlePoint(20f, 25f);

            Assert.IsFalse(validator.IsValid(targetPoint),
                "Point outside arena should be invalid and cancel cast.");
        }

        [Test]
        public void WrathDragInput_OnBoundary_IsValid()
        {
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);

            Assert.IsTrue(validator.IsValid(new BattlePoint(15f, 20f)));
            Assert.IsTrue(validator.IsValid(new BattlePoint(-15f, -20f)));
        }

        [Test]
        public void WrathDragInput_JustOutside_IsInvalid()
        {
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);

            Assert.IsFalse(validator.IsValid(new BattlePoint(15.01f, 0f)));
            Assert.IsFalse(validator.IsValid(new BattlePoint(0f, 20.01f)));
        }

        [Test]
        public void WrathCast_Integration_InsideArena_Succeeds()
        {
            var wrathService = new WrathService();
            var wrathConfig = new WrathConfig(
                chargePerKill: 20, maxCharge: 100, radius: 4f, damage: 80, impactDelaySeconds: 0.35f);
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);

            var useCase = new ZMediaTask.Application.Battle.TryCastWrathUseCase(
                ZMediaTask.Domain.Army.ArmySide.Left, wrathConfig, wrathService, validator);

            var meter = new WrathMeter(100, 100);
            var result = useCase.TryCast(
                meter,
                ZMediaTask.Domain.Army.ArmySide.Left,
                new BattlePoint(3f, 2f),
                currentTimeSec: 5f);

            Assert.IsTrue(result.Success, "Cast should succeed when inside arena.");
            Assert.IsTrue(result.Command.HasValue);
        }

        [Test]
        public void WrathCast_Integration_OutsideArena_Fails()
        {
            var wrathService = new WrathService();
            var wrathConfig = new WrathConfig(
                chargePerKill: 20, maxCharge: 100, radius: 4f, damage: 80, impactDelaySeconds: 0.35f);
            var validator = new ArenaBoundsWrathTargetValidator(TestBounds);

            var useCase = new ZMediaTask.Application.Battle.TryCastWrathUseCase(
                ZMediaTask.Domain.Army.ArmySide.Left, wrathConfig, wrathService, validator);

            var meter = new WrathMeter(100, 100);
            var result = useCase.TryCast(
                meter,
                ZMediaTask.Domain.Army.ArmySide.Left,
                new BattlePoint(50f, 50f),
                currentTimeSec: 5f);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(
                ZMediaTask.Application.Battle.WrathCastFailureReason.InvalidTarget,
                result.Failure);
        }
    }
}
