using System.Collections.Generic;
using ZMediaTask.Application.Battle;

namespace ZMediaTask.Presentation.Services
{
    public sealed class DeathTracker
    {
        private readonly HashSet<int> _deadUnitIds = new();
        private readonly List<int> _newlyDead = new();

        public IReadOnlyList<int> DetectNewDeaths(IReadOnlyList<BattleUnitRuntime> units)
        {
            _newlyDead.Clear();

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (!unit.Combat.IsAlive && _deadUnitIds.Add(unit.UnitId))
                {
                    _newlyDead.Add(unit.UnitId);
                }
            }

            return _newlyDead;
        }

        public void Reset()
        {
            _deadUnitIds.Clear();
            _newlyDead.Clear();
        }
    }
}
