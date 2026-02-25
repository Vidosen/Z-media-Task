using System;
using System.Collections.Generic;
using NUnit.Framework;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.EditMode.Application.Battle
{
    public class AutoBattleStepProcessorTests
    {
        private static readonly AttackConfig TestAttackConfig = new(attackRange: 1.5f, baseAttackDelay: 1f);
        private static readonly MovementConfig TestMovementConfig = new(
            meleeRange: 1.5f,
            repathDistanceThreshold: 2f,
            steeringRadius: 1.2f,
            slotRadius: 1f);
        private static readonly WrathConfig TestWrathConfig = new(
            chargePerKill: 20,
            maxCharge: 100,
            radius: 4f,
            damage: 80,
            impactDelaySeconds: 0.35f);

        [Test]
        public void Step_MovesUnitsCloserToEnemies()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, -5f, 0f, hp: 100, atk: 10, speed: 5f),
                MakeUnit(2, ArmySide.Right, 5f, 0f, hp: 100, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.5f, currentTimeSec: 0.5f);

            var result = processor.Step(input);

            var leftPos = result.Units[0].Movement.Position;
            var rightPos = result.Units[1].Movement.Position;

            // Units should be closer together than starting positions
            var initialDistance = DistanceMetrics.Distance(
                new BattlePoint(-5f, 0f), new BattlePoint(5f, 0f));
            var newDistance = DistanceMetrics.Distance(leftPos, rightPos);

            Assert.Less(newDistance, initialDistance, "Units should move closer after a step.");
        }

        [Test]
        public void Step_AttackKillsUnit_WhenInRange()
        {
            var processor = CreateProcessor();
            // Place units within attack range
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 200, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 50, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            // Unit 2 should be dead (50 HP - 200 ATK = 0)
            Assert.IsFalse(result.Units[1].Combat.IsAlive, "Target should be killed.");
            Assert.AreEqual(0, result.Units[1].Combat.CurrentHp);
        }

        [Test]
        public void Step_ChargesWrathMeter_OnEnemyKill()
        {
            var processor = CreateProcessor();
            // Left kills Right -> wrath charges for player (Left)
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 200, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 50, atk: 10, speed: 5f)
            };
            var wrathMeters = new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, 100),
                [ArmySide.Right] = new WrathMeter(0, 100)
            };
            var context = new BattleContext(units, 0f, null, wrathMeters);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            Assert.AreEqual(20, result.WrathMeters[ArmySide.Left].CurrentCharge,
                "Wrath should charge by 20 for enemy kill.");
        }

        [Test]
        public void Step_DoesNotChargeWrath_OnFriendlyKill()
        {
            // Only enemy kills charge wrath. Friendly fire from wrath AoE
            // is handled elsewhere; here we test that step processor only charges
            // for actual combat kills of enemies.
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Right, 0f, 0f, hp: 100, atk: 200, speed: 5f),
                MakeUnit(2, ArmySide.Left, 1f, 0f, hp: 50, atk: 10, speed: 5f)
            };
            var wrathMeters = new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, 100),
                [ArmySide.Right] = new WrathMeter(0, 100)
            };
            var context = new BattleContext(units, 0f, null, wrathMeters);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            // Right killed Left -> Left's wrath does not charge (Right is not the owner side)
            Assert.AreEqual(0, result.WrathMeters[ArmySide.Left].CurrentCharge,
                "Wrath should not charge when the player's unit dies.");
        }

        [Test]
        public void Step_SyncsMovementPositionToCombatPosition()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, -3f, 0f, hp: 100, atk: 10, speed: 10f),
                MakeUnit(2, ArmySide.Right, 3f, 0f, hp: 100, atk: 10, speed: 10f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.5f, currentTimeSec: 0.5f);

            var result = processor.Step(input);

            for (var i = 0; i < result.Units.Count; i++)
            {
                var u = result.Units[i];
                Assert.AreEqual(u.Movement.Position.X, u.Combat.Position.X, 0.001f,
                    $"Unit {u.UnitId} movement/combat X positions should be synced.");
                Assert.AreEqual(u.Movement.Position.Z, u.Combat.Position.Z, 0.001f,
                    $"Unit {u.UnitId} movement/combat Z positions should be synced.");
            }
        }

        [Test]
        public void Step_SyncsCombatAliveToMovement_WhenUnitDies()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 999, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 10, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            Assert.IsFalse(result.Units[1].Combat.IsAlive);
            Assert.IsFalse(result.Units[1].Movement.IsAlive,
                "Movement.IsAlive should be synced to Combat.IsAlive.");
        }

        [Test]
        public void Step_EmitsUnitDamagedEvent_WhenAttackLands()
        {
            var processor = CreateProcessor();
            // Place units within attack range so an attack lands
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 30, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 100, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            processor.Step(input);

            var stepEvents = processor.LastStepEvents;
            Assert.IsTrue(stepEvents.Count > 0, "Should emit at least one UnitDamaged event.");

            var found = false;
            for (var i = 0; i < stepEvents.Count; i++)
            {
                var evt = stepEvents[i];
                if (evt.Kind != BattleEventKind.UnitDamaged) continue;

                found = true;
                Assert.IsTrue(evt.UnitId.HasValue, "UnitDamaged event should have a UnitId.");
                Assert.IsTrue(evt.DamageApplied.HasValue, "UnitDamaged event should have DamageApplied.");
                Assert.Greater(evt.DamageApplied.Value, 0, "DamageApplied should be > 0.");
                Assert.IsTrue(evt.Position.HasValue, "UnitDamaged event should have a Position.");
            }

            Assert.IsTrue(found, "Should contain at least one UnitDamaged event.");
        }

        [Test]
        public void Step_AdvancesElapsedTime()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, -5f, 0f, hp: 100, atk: 10, speed: 5f),
                MakeUnit(2, ArmySide.Right, 5f, 0f, hp: 100, atk: 10, speed: 5f)
            };
            var context = new BattleContext(units, 1.0f, null, MakeDefaultWrathMeters());
            var input = new BattleStepInput(context, deltaTimeSec: 0.5f, currentTimeSec: 1.5f);

            var result = processor.Step(input);

            Assert.AreEqual(1.5f, result.ElapsedTimeSec, 0.001f);
        }

        private static AutoBattleStepProcessor CreateProcessor()
        {
            var targetSelector = new NearestTargetSelector();
            var pathfinder = new DirectPathfinder();
            var steeringService = new SpatialHashSteeringService();
            var slotAllocator = new RingSlotAllocator();
            var movementService = new MovementService(targetSelector, pathfinder, steeringService, slotAllocator);
            var healthService = new HealthService();
            var cooldownTracker = new CooldownTracker();
            var attackService = new AttackService(cooldownTracker, healthService);
            var wrathService = new WrathService(healthService);
            var onUnitKilled = new OnUnitKilledUseCase(ArmySide.Left, TestWrathConfig, wrathService);

            return new AutoBattleStepProcessor(
                movementService, attackService, onUnitKilled, TestAttackConfig, TestMovementConfig);
        }

        private static BattleUnitRuntime MakeUnit(
            int unitId, ArmySide side, float x, float z, int hp, int atk, float speed)
        {
            var pos = new BattlePoint(x, z);
            var movement = new MovementAgentState(
                unitId, hp > 0, speed, pos, null, Array.Empty<BattlePoint>(), null);
            var combat = new CombatUnitState(unitId, pos, hp, atk, attackSpeed: 1, nextAttackTimeSec: 0f);
            return new BattleUnitRuntime(unitId, side, UnitShape.Cube, UnitSize.Small, UnitColor.Blue, movement, combat);
        }

        private static BattleContext MakeContext(BattleUnitRuntime[] units)
        {
            return new BattleContext(units, 0f, null, MakeDefaultWrathMeters());
        }

        private static Dictionary<ArmySide, WrathMeter> MakeDefaultWrathMeters()
        {
            return new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, 100),
                [ArmySide.Right] = new WrathMeter(0, 100)
            };
        }
    }
}
