using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct WrathApplyResult
    {
        private readonly IReadOnlyList<CombatUnitState> _unitsAfter;
        private readonly IReadOnlyList<int> _killedUnitIds;

        public WrathApplyResult(IReadOnlyList<CombatUnitState> unitsAfter, IReadOnlyList<int> killedUnitIds)
        {
            if (unitsAfter == null)
            {
                throw new ArgumentNullException(nameof(unitsAfter));
            }

            if (killedUnitIds == null)
            {
                throw new ArgumentNullException(nameof(killedUnitIds));
            }

            var unitsCopy = new CombatUnitState[unitsAfter.Count];
            for (var i = 0; i < unitsAfter.Count; i++)
            {
                unitsCopy[i] = unitsAfter[i];
            }

            var killedCopy = new int[killedUnitIds.Count];
            for (var i = 0; i < killedUnitIds.Count; i++)
            {
                killedCopy[i] = killedUnitIds[i];
            }

            _unitsAfter = unitsCopy;
            _killedUnitIds = killedCopy;
        }

        public IReadOnlyList<CombatUnitState> UnitsAfter => _unitsAfter ?? Array.Empty<CombatUnitState>();

        public IReadOnlyList<int> KilledUnitIds => _killedUnitIds ?? Array.Empty<int>();
    }
}
