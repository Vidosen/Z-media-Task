using System.Linq;
using NUnit.Framework;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.EditMode.Application.Battle
{
    public class WrathBattleLoopIntegrationTests
    {
        private static readonly WrathConfig Config = new(
            chargePerKill: 20,
            maxCharge: 100,
            radius: 20f,
            damage: 50,
            impactDelaySeconds: 0.5f);

        [Test]
        public void WrathCast_DealsDamage_ToBothSides_InRadius()
        {
            var loop = CreateLoop(Config);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();

            var accepted = loop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Left,
                center: new BattlePoint(0f, 0f),
                radius: 20f,
                damage: 30,
                castTimeSec: 0f,
                impactTimeSec: 0f));

            Assert.IsTrue(accepted);

            loop.Tick(deltaTimeSec: 0.1f, currentTimeSec: 0f);

            var leftHp = loop.Context.Units.Single(u => u.Side == ArmySide.Left).Combat.CurrentHp;
            var rightHp = loop.Context.Units.Single(u => u.Side == ArmySide.Right).Combat.CurrentHp;
            Assert.AreEqual(70, leftHp);
            Assert.AreEqual(70, rightHp);
        }

        [Test]
        public void WrathCast_AppliesDamage_OnImpactDelay_NotOnDragRelease()
        {
            var loop = CreateLoop(Config);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();

            loop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Left,
                center: new BattlePoint(0f, 0f),
                radius: 20f,
                damage: 40,
                castTimeSec: 0f,
                impactTimeSec: 1f));

            loop.Tick(deltaTimeSec: 0.1f, currentTimeSec: 0.5f);
            Assert.AreEqual(100, loop.Context.Units[0].Combat.CurrentHp);
            Assert.AreEqual(100, loop.Context.Units[1].Combat.CurrentHp);

            loop.Tick(deltaTimeSec: 0.1f, currentTimeSec: 1f);
            Assert.AreEqual(60, loop.Context.Units[0].Combat.CurrentHp);
            Assert.AreEqual(60, loop.Context.Units[1].Combat.CurrentHp);
        }

        [Test]
        public void BattleLoop_EnqueueWrathCast_ReturnsFalse_WhenNotRunning()
        {
            var loop = CreateLoop(Config);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));

            var accepted = loop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Left,
                center: new BattlePoint(0f, 0f),
                radius: 20f,
                damage: 40,
                castTimeSec: 0f,
                impactTimeSec: 0f));

            Assert.IsFalse(accepted);
        }

        [Test]
        public void BattleLoop_WrathImpact_EmitsWrathImpactAppliedEvent()
        {
            var loop = CreateLoop(Config);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();
            loop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Left,
                center: new BattlePoint(0f, 0f),
                radius: 20f,
                damage: 10,
                castTimeSec: 0f,
                impactTimeSec: 0f));

            loop.Tick(deltaTimeSec: 0.1f, currentTimeSec: 0f);

            var impactEvent = loop.LastTickEvents.Single(e => e.Kind == BattleEventKind.WrathImpactApplied);
            Assert.AreEqual(2, impactEvent.AffectedCount);
        }

        [Test]
        public void BattleLoop_WrathKill_EmitsUnitKilledEvent()
        {
            var loop = CreateLoop(Config);
            loop.Initialize(CreateArmies(leftHp: 5, rightHp: 100));
            loop.Start();
            loop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Right,
                center: new BattlePoint(-8f, 0f),
                radius: 1f,
                damage: 10,
                castTimeSec: 0f,
                impactTimeSec: 0f));

            loop.Tick(deltaTimeSec: 0.1f, currentTimeSec: 0f);

            var killEvent = loop.LastTickEvents.Single(e => e.Kind == BattleEventKind.UnitKilled);
            Assert.AreEqual(ArmySide.Left, killEvent.Side);
            Assert.IsTrue(killEvent.UnitId.HasValue);
        }

        private static BattleLoopService CreateLoop(WrathConfig config)
        {
            return new BattleLoopService(
                new BattleContextFactory(),
                new IdentityStepProcessor(),
                new DistanceUnitQueryInRadius(),
                new WrathService(),
                config);
        }

        private static ArmyPair CreateArmies(int leftHp, int rightHp)
        {
            return new ArmyPair(
                new ZMediaTask.Domain.Army.Army(ArmySide.Left, new[] { CreateUnit(leftHp) }),
                new ZMediaTask.Domain.Army.Army(ArmySide.Right, new[] { CreateUnit(rightHp) }));
        }

        private static ArmyUnit CreateUnit(int hp)
        {
            return new ArmyUnit(
                UnitShape.Cube,
                UnitSize.Small,
                UnitColor.Blue,
                new StatBlock(hp, atk: 10, speed: 10, atkspd: 1));
        }

        private sealed class IdentityStepProcessor : IBattleStepProcessor
        {
            public BattleContext Step(BattleStepInput input)
            {
                return new BattleContext(
                    input.Context.Units,
                    input.Context.ElapsedTimeSec + input.DeltaTimeSec,
                    input.Context.WinnerSide,
                    input.Context.WrathMeters);
            }
        }
    }
}
