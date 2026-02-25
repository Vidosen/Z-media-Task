using System;
using System.Collections.Generic;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class DistanceUnitQueryInRadius : IUnitQueryInRadius
    {
        public IReadOnlySet<int> Query(IReadOnlyList<BattleUnitRuntime> units, BattlePoint center, float radius)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            var result = new HashSet<int>();
            if (radius < 0f)
            {
                return new ReadOnlySet<int>(result);
            }

            var radiusSqr = radius * radius;
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var dx = unit.Combat.Position.X - center.X;
                var dz = unit.Combat.Position.Z - center.Z;
                var sqrDistance = (dx * dx) + (dz * dz);

                if (sqrDistance <= radiusSqr)
                {
                    result.Add(unit.UnitId);
                }
            }

            return new ReadOnlySet<int>(result);
        }
    }
}
