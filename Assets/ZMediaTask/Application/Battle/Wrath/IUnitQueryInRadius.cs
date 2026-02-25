using System.Collections.Generic;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public interface IUnitQueryInRadius
    {
        IReadOnlySet<int> Query(IReadOnlyList<BattleUnitRuntime> units, BattlePoint center, float radius);
    }
}
