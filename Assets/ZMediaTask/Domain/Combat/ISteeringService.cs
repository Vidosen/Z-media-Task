using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public interface ISteeringService
    {
        BattlePoint ComputeSeparationOffset(
            BattlePoint selfPosition,
            IReadOnlyList<BattlePoint> neighborPositions,
            float steeringRadius);
    }
}
