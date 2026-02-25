using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public sealed class DirectPathfinder : IPathfinder
    {
        public IReadOnlyList<BattlePoint> BuildPath(BattlePoint from, BattlePoint to)
        {
            return new[] { to };
        }
    }
}
