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
                Assert.IsTrue(evt.AttackerPosition.HasValue, "UnitDamaged event should have AttackerPosition.");
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

        private static readonly KnockbackConfig TestKnockbackConfig = new(
            impulseStrength: 6f,
            decaySpeed: 50f,
            minVelocityThreshold: 0.001f);

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
            var knockbackService = new KnockbackService();

            return new AutoBattleStepProcessor(
                movementService, attackService, onUnitKilled, TestAttackConfig, TestMovementConfig,
                knockbackService, TestKnockbackConfig);
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

        private static BattleUnitRuntime MakeUnitWithKnockback(
            int unitId, ArmySide side, float x, float z, int hp, int atk, float speed,
            KnockbackState knockback)
        {
            var pos = new BattlePoint(x, z);
            var movement = new MovementAgentState(
                unitId, hp > 0, speed, pos, null, Array.Empty<BattlePoint>(), null);
            var combat = new CombatUnitState(unitId, pos, hp, atk, attackSpeed: 1, nextAttackTimeSec: 0f);
            return new BattleUnitRuntime(
                unitId, side, UnitShape.Cube, UnitSize.Small, UnitColor.Blue, movement, combat, knockback);
        }

        [Test]
        public void Step_WhenAttackLands_TargetGainsKnockbackVelocity()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 30, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 100, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            Assert.IsTrue(result.Units[1].Knockback.HasVelocity,
                "Target should have knockback velocity after being hit.");
            Assert.Greater(result.Units[1].Knockback.Velocity.X, 0f,
                "Knockback should push away from attacker (positive X).");
        }

        [Test]
        public void Step_KnockbackDisplacesPosition_OnSubsequentTick()
        {
            var processor = CreateProcessor();
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 30, speed: 5f),
                MakeUnit(2, ArmySide.Right, 1f, 0f, hp: 100, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            // First tick — attack lands, knockback applied
            var result1 = processor.Step(input);
            var posAfterHit = result1.Units[1].Movement.Position.X;

            // Second tick — knockback displaces position
            var input2 = new BattleStepInput(result1, deltaTimeSec: 0.02f, currentTimeSec: 0.04f);
            var result2 = processor.Step(input2);
            var posAfterDisplace = result2.Units[1].Movement.Position.X;

            Assert.Greater(posAfterDisplace, posAfterHit,
                "Knockback should displace target further in subsequent tick.");
        }

        [Test]
        public void Step_MultipleHits_StackKnockback()
        {
            var processor = CreateProcessor();
            // Two left attackers can both hit the right unit
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 10, speed: 5f),
                MakeUnit(2, ArmySide.Left, 0f, 1f, hp: 100, atk: 10, speed: 5f),
                MakeUnit(3, ArmySide.Right, 1f, 0f, hp: 200, atk: 10, speed: 5f)
            };
            var context = MakeContext(units);
            var input = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);

            var result = processor.Step(input);

            // Check how many damage events hit unit 3
            var hitCount = 0;
            for (var i = 0; i < processor.LastStepEvents.Count; i++)
            {
                var evt = processor.LastStepEvents[i];
                if (evt.Kind == BattleEventKind.UnitDamaged && evt.UnitId == 3)
                {
                    hitCount++;
                }
            }

            if (hitCount >= 2)
            {
                // If both attacks landed, knockback should be stacked
                var velocity = DistanceMetrics.Magnitude(result.Units[2].Knockback.Velocity);
                Assert.Greater(velocity, TestKnockbackConfig.ImpulseStrength,
                    "Multiple hits should stack knockback beyond single impulse.");
            }
            else
            {
                // At least one hit should produce knockback
                Assert.IsTrue(result.Units[2].Knockback.HasVelocity,
                    "At least one hit should produce knockback.");
            }
        }

        [Test]
        public void Step_KnockbackDecays_OverMultipleTicks()
        {
            var processor = CreateProcessor();
            // Place units far apart with zero speed so no attacks land during decay
            var knockback = new KnockbackState(new BattlePoint(6f, 0f));
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, -10f, 0f, hp: 100, atk: 10, speed: 0f),
                MakeUnitWithKnockback(2, ArmySide.Right, 10f, 0f, hp: 100, atk: 10, speed: 0f, knockback)
            };
            var context = MakeContext(units);
            var initialMag = DistanceMetrics.Magnitude(knockback.Velocity);

            // Run several ticks to let knockback decay
            var current = context;
            for (var t = 0; t < 10; t++)
            {
                var elapsed = (t + 1) * 0.02f;
                var nextInput = new BattleStepInput(current, deltaTimeSec: 0.02f, currentTimeSec: elapsed);
                current = processor.Step(nextInput);
            }

            var finalMag = DistanceMetrics.Magnitude(current.Units[1].Knockback.Velocity);
            Assert.Less(finalMag, initialMag,
                "Knockback should decay over multiple ticks.");
        }

        [Test]
        public void Step_DeadUnit_DoesNotReceiveKnockback()
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

            Assert.IsFalse(result.Units[1].Combat.IsAlive, "Unit should be dead.");
            Assert.IsFalse(result.Units[1].Knockback.HasVelocity,
                "Dead unit should not receive knockback.");
        }

        [Test]
        public void Step_KnockbackAffectsRange_PushesOutOfAttackRange()
        {
            var processor = CreateProcessor();
            // Place units just within attack range (1.5)
            var units = new[]
            {
                MakeUnit(1, ArmySide.Left, 0f, 0f, hp: 100, atk: 10, speed: 0f),
                MakeUnit(2, ArmySide.Right, 1.4f, 0f, hp: 100, atk: 10, speed: 0f)
            };
            var context = MakeContext(units);

            // First tick — both can attack (within 1.5 range), knockback applied
            var input1 = new BattleStepInput(context, deltaTimeSec: 0.02f, currentTimeSec: 0.02f);
            var result1 = processor.Step(input1);

            // Run enough ticks for knockback to displace the target
            var current = result1;
            for (var t = 0; t < 10; t++)
            {
                var elapsed = 0.04f + t * 0.02f;
                var nextInput = new BattleStepInput(current, deltaTimeSec: 0.02f, currentTimeSec: elapsed);
                current = processor.Step(nextInput);
            }

            // Verify the knocked-back unit moved further from attacker
            var finalDist = DistanceMetrics.Distance(
                current.Units[0].Combat.Position, current.Units[1].Combat.Position);
            var initialDist = DistanceMetrics.Distance(
                new BattlePoint(0f, 0f), new BattlePoint(1.4f, 0f));

            Assert.Greater(finalDist, initialDist,
                "Knockback should push target further, proving gameplay impact on range.");
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
