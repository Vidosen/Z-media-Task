using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public sealed class SpatialHashSteeringService : ISteeringService
    {
        public BattlePoint ComputeSeparationOffset(
            BattlePoint selfPosition,
            IReadOnlyList<BattlePoint> neighborPositions,
            float steeringRadius)
        {
            if (neighborPositions == null)
            {
                throw new ArgumentNullException(nameof(neighborPositions));
            }

            if (steeringRadius <= 0f || neighborPositions.Count == 0)
            {
                return new BattlePoint(0f, 0f);
            }

            var cellSize = steeringRadius;
            var neighborMap = BuildSpatialMap(neighborPositions, cellSize);
            var selfCell = ToCell(selfPosition, cellSize);
            var steeringRadiusSqr = steeringRadius * steeringRadius;
            var cumulative = new BattlePoint(0f, 0f);
            var hasRepulsion = false;

            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var candidateCell = (selfCell.x + dx, selfCell.z + dz);
                    if (!neighborMap.TryGetValue(candidateCell, out var cellNeighbors))
                    {
                        continue;
                    }

                    for (var i = 0; i < cellNeighbors.Count; i++)
                    {
                        var neighbor = cellNeighbors[i];
                        var distanceSqr = DistanceMetrics.SqrDistance(selfPosition, neighbor);
                        if (distanceSqr <= 0f || distanceSqr > steeringRadiusSqr)
                        {
                            continue;
                        }

                        var distance = (float)Math.Sqrt(distanceSqr);
                        var strength = (steeringRadius - distance) / steeringRadius;
                        var away = DistanceMetrics.Direction(neighbor, selfPosition);
                        var push = DistanceMetrics.Scale(away, strength);

                        cumulative = DistanceMetrics.Add(cumulative, push);
                        hasRepulsion = true;
                    }
                }
            }

            if (!hasRepulsion)
            {
                return new BattlePoint(0f, 0f);
            }

            var normalized = DistanceMetrics.Normalize(cumulative);
            return normalized;
        }

        private static Dictionary<(int x, int z), List<BattlePoint>> BuildSpatialMap(
            IReadOnlyList<BattlePoint> neighbors,
            float cellSize)
        {
            var map = new Dictionary<(int x, int z), List<BattlePoint>>();
            for (var i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                var key = ToCell(neighbor, cellSize);

                if (!map.TryGetValue(key, out var bucket))
                {
                    bucket = new List<BattlePoint>();
                    map[key] = bucket;
                }

                bucket.Add(neighbor);
            }

            return map;
        }

        private static (int x, int z) ToCell(BattlePoint point, float cellSize)
        {
            var x = (int)Math.Floor(point.X / cellSize);
            var z = (int)Math.Floor(point.Z / cellSize);
            return (x, z);
        }
    }
}
