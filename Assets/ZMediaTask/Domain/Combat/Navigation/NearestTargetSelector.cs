using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public sealed class NearestTargetSelector : ITargetSelector
    {
        public int? SelectTarget(
            TargetableUnit self,
            IReadOnlyList<TargetableUnit> enemies,
            int? currentTargetId)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            if (currentTargetId.HasValue && TryGetAliveEnemyById(enemies, currentTargetId.Value, out _))
            {
                return currentTargetId.Value;
            }

            if (TryFindNearestAliveEnemy(self, enemies, out var nearestEnemy))
            {
                return nearestEnemy.UnitId;
            }

            return null;
        }

        private static bool TryGetAliveEnemyById(
            IReadOnlyList<TargetableUnit> enemies,
            int targetId,
            out TargetableUnit enemy)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }

                if (candidate.UnitId == targetId)
                {
                    enemy = candidate;
                    return true;
                }
            }

            enemy = default;
            return false;
        }

        private static bool TryFindNearestAliveEnemy(
            TargetableUnit self,
            IReadOnlyList<TargetableUnit> enemies,
            out TargetableUnit nearestEnemy)
        {
            var bestDistance = float.MaxValue;
            var hasAlive = false;
            nearestEnemy = default;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }

                var distance = DistanceMetrics.SqrDistance(self.Position, candidate.Position);
                if (!hasAlive || distance < bestDistance)
                {
                    hasAlive = true;
                    bestDistance = distance;
                    nearestEnemy = candidate;
                }
            }

            return hasAlive;
        }
    }
}
