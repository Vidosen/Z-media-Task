using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public interface IPathQueryService
    {
        IReadOnlyList<BattlePoint> QueryPath(BattlePoint from, BattlePoint to);
    }
}
