using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMediaTask.Domain.Combat
{
    public sealed class MovementService
    {
        private readonly ITargetSelector _targetSelector;
        private readonly IPathfinder _pathfinder;
        private readonly ISteeringService _steeringService;
        private readonly ISlotAllocator _slotAllocator;

        public MovementService(
            ITargetSelector targetSelector,
            IPathfinder pathfinder,
            ISteeringService steeringService,
            ISlotAllocator slotAllocator)
        {
            _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
            _pathfinder = pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));
            _steeringService = steeringService ?? throw new ArgumentNullException(nameof(steeringService));
            _slotAllocator = slotAllocator ?? throw new ArgumentNullException(nameof(slotAllocator));
        }

        public MovementAgentState Tick(MovementAgentState self, MovementTickContext context)
        {
            if (!self.IsAlive || self.Speed <= 0f)
            {
                return self;
            }

            var targetId = _targetSelector.SelectTarget(
                new TargetableUnit(self.UnitId, self.Position, self.IsAlive),
                context.Enemies,
                self.TargetId);

            if (!targetId.HasValue)
            {
                return BuildState(self, self.Position, null, Array.Empty<BattlePoint>(), null);
            }

            if (!TryGetAliveEnemyById(context.Enemies, targetId.Value, out var target))
            {
                return BuildState(self, self.Position, null, Array.Empty<BattlePoint>(), null);
            }

            var attackerIds = BuildAttackerIds(context.Allies, targetId.Value, self.UnitId);
            var slotDestination = _slotAllocator.GetSlotPosition(
                target.Position,
                self.UnitId,
                attackerIds,
                context.Config.SlotRadius);

            if (DistanceMetrics.Distance(self.Position, target.Position) <= context.Config.MeleeRange)
            {
                return BuildState(
                    self,
                    self.Position,
                    targetId,
                    self.CurrentPath,
                    self.LastPathTargetPosition);
            }

            IReadOnlyList<BattlePoint> path = self.CurrentPath;
            var lastPathTargetPosition = self.LastPathTargetPosition;

            if (ShouldRepath(path, lastPathTargetPosition, target.Position, context.Config.RepathDistanceThreshold))
            {
                path = _pathfinder.BuildPath(self.Position, slotDestination) ?? Array.Empty<BattlePoint>();
                lastPathTargetPosition = target.Position;
            }

            var waypoint = path.Count > 0 ? path[0] : slotDestination;
            var neighborPositions = BuildNeighborPositions(context.Allies, self.UnitId);
            var separationOffset = _steeringService.ComputeSeparationOffset(
                self.Position,
                neighborPositions,
                context.Config.SteeringRadius);
            var moveTarget = DistanceMetrics.Add(waypoint, separationOffset);
            var maxDistance = self.Speed * context.DeltaTime;
            var position = DistanceMetrics.MoveTowards(self.Position, moveTarget, maxDistance);

            return BuildState(self, position, targetId, path, lastPathTargetPosition);
        }

        private static bool ShouldRepath(
            IReadOnlyList<BattlePoint> currentPath,
            BattlePoint? lastPathTargetPosition,
            BattlePoint targetPosition,
            float repathDistanceThreshold)
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                return true;
            }

            if (!lastPathTargetPosition.HasValue)
            {
                return true;
            }

            var thresholdSqr = repathDistanceThreshold * repathDistanceThreshold;
            var movedSqr = DistanceMetrics.SqrDistance(lastPathTargetPosition.Value, targetPosition);
            return movedSqr > thresholdSqr;
        }

        private static bool TryGetAliveEnemyById(
            IReadOnlyList<TargetableUnit> enemies,
            int targetId,
            out TargetableUnit enemy)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (candidate.UnitId == targetId && candidate.IsAlive)
                {
                    enemy = candidate;
                    return true;
                }
            }

            enemy = default;
            return false;
        }

        private static IReadOnlyList<int> BuildAttackerIds(
            IReadOnlyList<MovementAgentState> allies,
            int targetId,
            int selfUnitId)
        {
            var ids = new List<int>();
            for (var i = 0; i < allies.Count; i++)
            {
                var ally = allies[i];
                if (ally.IsAlive && ally.TargetId == targetId)
                {
                    ids.Add(ally.UnitId);
                }
            }

            if (!ids.Contains(selfUnitId))
            {
                ids.Add(selfUnitId);
            }

            return ids;
        }

        private static IReadOnlyList<BattlePoint> BuildNeighborPositions(
            IReadOnlyList<MovementAgentState> allies,
            int selfUnitId)
        {
            var neighbors = new List<BattlePoint>();
            for (var i = 0; i < allies.Count; i++)
            {
                var ally = allies[i];
                if (!ally.IsAlive || ally.UnitId == selfUnitId)
                {
                    continue;
                }

                neighbors.Add(ally.Position);
            }

            return neighbors;
        }

        private static MovementAgentState BuildState(
            MovementAgentState source,
            BattlePoint position,
            int? targetId,
            IReadOnlyList<BattlePoint> path,
            BattlePoint? lastPathTargetPosition)
        {
            return new MovementAgentState(
                source.UnitId,
                source.IsAlive,
                source.Speed,
                position,
                targetId,
                path,
                lastPathTargetPosition);
        }
    }
}
