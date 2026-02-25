using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMediaTask.Domain.Combat
{
    public sealed class RingSlotAllocator : ISlotAllocator
    {
        public BattlePoint GetSlotPosition(
            BattlePoint targetPosition,
            int unitId,
            IReadOnlyList<int> attackerIds,
            float slotRadius)
        {
            if (attackerIds == null)
            {
                throw new ArgumentNullException(nameof(attackerIds));
            }

            if (slotRadius <= 0f)
            {
                return targetPosition;
            }

            var sortedIds = attackerIds
                .Distinct()
                .OrderBy(id => id)
                .ToArray();

            if (sortedIds.Length == 0)
            {
                sortedIds = new[] { unitId };
            }

            var index = Array.IndexOf(sortedIds, unitId);
            if (index < 0)
            {
                var merged = sortedIds.Concat(new[] { unitId }).OrderBy(id => id).ToArray();
                sortedIds = merged;
                index = Array.IndexOf(sortedIds, unitId);
            }

            var angleStep = (float)(2d * Math.PI / sortedIds.Length);
            var angle = angleStep * index;
            var x = targetPosition.X + ((float)Math.Cos(angle) * slotRadius);
            var z = targetPosition.Z + ((float)Math.Sin(angle) * slotRadius);

            return new BattlePoint(x, z);
        }
    }
}
