using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public interface ISlotAllocator
    {
        BattlePoint GetSlotPosition(
            BattlePoint targetPosition,
            int unitId,
            IReadOnlyList<int> attackerIds,
            float slotRadius);
    }
}
