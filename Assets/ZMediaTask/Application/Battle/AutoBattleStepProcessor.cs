using System;
using System.Collections.Generic;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class AutoBattleStepProcessor : IBattleStepProcessor
    {
        private readonly MovementService _movementService;
        private readonly AttackService _attackService;
        private readonly OnUnitKilledUseCase _onUnitKilledUseCase;
        private readonly AttackConfig _attackConfig;
        private readonly MovementConfig _movementConfig;

        public AutoBattleStepProcessor(
            MovementService movementService,
            AttackService attackService,
            OnUnitKilledUseCase onUnitKilledUseCase,
            AttackConfig attackConfig,
            MovementConfig movementConfig)
        {
            _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _onUnitKilledUseCase = onUnitKilledUseCase ?? throw new ArgumentNullException(nameof(onUnitKilledUseCase));
            _attackConfig = attackConfig;
            _movementConfig = movementConfig;
        }

        public BattleContext Step(BattleStepInput input)
        {
            var units = CopyUnits(input.Context.Units);
            var currentTime = input.CurrentTimeSec;
            var deltaTime = input.DeltaTimeSec;

            UpdateMovement(units, deltaTime);
            SyncMovementToCombatPositions(units);

            var wrathMeters = CopyWrathMeters(input.Context.WrathMeters);
            ProcessAttacks(units, currentTime, wrathMeters);
            SyncCombatAliveToMovement(units);

            var newElapsed = input.Context.ElapsedTimeSec + deltaTime;
            return new BattleContext(units, newElapsed, null, wrathMeters);
        }

        private void UpdateMovement(BattleUnitRuntime[] units, float deltaTime)
        {
            var leftMovement = new List<MovementAgentState>();
            var rightMovement = new List<MovementAgentState>();
            var leftTargetable = new List<TargetableUnit>();
            var rightTargetable = new List<TargetableUnit>();

            for (var i = 0; i < units.Length; i++)
            {
                var u = units[i];
                var ms = u.Movement;
                var tu = new TargetableUnit(u.UnitId, ms.Position, ms.IsAlive);

                if (u.Side == ArmySide.Left)
                {
                    leftMovement.Add(ms);
                    leftTargetable.Add(tu);
                }
                else
                {
                    rightMovement.Add(ms);
                    rightTargetable.Add(tu);
                }
            }

            for (var i = 0; i < units.Length; i++)
            {
                var u = units[i];
                if (!u.Movement.IsAlive)
                {
                    continue;
                }

                var allies = u.Side == ArmySide.Left ? leftMovement : rightMovement;
                var enemies = u.Side == ArmySide.Left ? rightTargetable : leftTargetable;
                var ctx = new MovementTickContext(deltaTime, allies, enemies, _movementConfig);
                var updated = _movementService.Tick(u.Movement, ctx);
                units[i] = u.WithMovement(updated);
            }
        }

        private static void SyncMovementToCombatPositions(BattleUnitRuntime[] units)
        {
            for (var i = 0; i < units.Length; i++)
            {
                var u = units[i];
                if (!u.Movement.IsAlive)
                {
                    continue;
                }

                var newCombat = u.Combat.WithPosition(u.Movement.Position);
                units[i] = u.WithCombat(newCombat);
            }
        }

        private void ProcessAttacks(
            BattleUnitRuntime[] units,
            float currentTime,
            Dictionary<ArmySide, WrathMeter> wrathMeters)
        {
            var sortedIndices = GetSortedIndicesByUnitId(units);

            for (var s = 0; s < sortedIndices.Length; s++)
            {
                var attackerIdx = sortedIndices[s];
                var attacker = units[attackerIdx];
                if (!attacker.Combat.IsAlive)
                {
                    continue;
                }

                var targetIdx = FindTarget(units, attacker);
                if (targetIdx < 0)
                {
                    continue;
                }

                var target = units[targetIdx];
                var result = _attackService.TryAttack(attacker.Combat, target.Combat, currentTime, _attackConfig);
                if (!result.Success)
                {
                    continue;
                }

                units[attackerIdx] = attacker.WithCombat(result.AttackerAfter);
                units[targetIdx] = target.WithCombat(result.TargetAfter);

                if (!result.TargetAfter.IsAlive)
                {
                    if (wrathMeters.TryGetValue(ArmySide.Left, out var meter))
                    {
                        wrathMeters[ArmySide.Left] = _onUnitKilledUseCase.OnUnitKilled(
                            meter, attacker.Side, target.Side);
                    }
                }
            }
        }

        private static int FindTarget(BattleUnitRuntime[] units, BattleUnitRuntime attacker)
        {
            var bestIdx = -1;
            var bestDistSqr = float.MaxValue;

            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate.Side == attacker.Side || !candidate.Combat.IsAlive)
                {
                    continue;
                }

                var distSqr = DistanceMetrics.SqrDistance(attacker.Combat.Position, candidate.Combat.Position);
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    bestIdx = i;
                }
            }

            return bestIdx;
        }

        private static void SyncCombatAliveToMovement(BattleUnitRuntime[] units)
        {
            for (var i = 0; i < units.Length; i++)
            {
                var u = units[i];
                if (u.Movement.IsAlive == u.Combat.IsAlive)
                {
                    continue;
                }

                var updated = new MovementAgentState(
                    u.Movement.UnitId,
                    u.Combat.IsAlive,
                    u.Movement.Speed,
                    u.Movement.Position,
                    u.Movement.TargetId,
                    u.Movement.CurrentPath,
                    u.Movement.LastPathTargetPosition);
                units[i] = u.WithMovement(updated);
            }
        }

        private static int[] GetSortedIndicesByUnitId(BattleUnitRuntime[] units)
        {
            var indices = new int[units.Length];
            for (var i = 0; i < units.Length; i++)
            {
                indices[i] = i;
            }

            Array.Sort(indices, (a, b) => units[a].UnitId.CompareTo(units[b].UnitId));
            return indices;
        }

        private static BattleUnitRuntime[] CopyUnits(IReadOnlyList<BattleUnitRuntime> units)
        {
            var copy = new BattleUnitRuntime[units.Count];
            for (var i = 0; i < units.Count; i++)
            {
                copy[i] = units[i];
            }

            return copy;
        }

        private static Dictionary<ArmySide, WrathMeter> CopyWrathMeters(
            IReadOnlyDictionary<ArmySide, WrathMeter> source)
        {
            var copy = new Dictionary<ArmySide, WrathMeter>();
            foreach (var pair in source)
            {
                copy[pair.Key] = pair.Value;
            }

            return copy;
        }
    }
}
