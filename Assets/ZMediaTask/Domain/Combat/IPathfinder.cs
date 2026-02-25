using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public interface IPathfinder
    {
        IReadOnlyList<BattlePoint> BuildPath(BattlePoint from, BattlePoint to);
    }
}
