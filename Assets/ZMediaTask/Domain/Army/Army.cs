using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMediaTask.Domain.Army
{
    public sealed class Army
    {
        private readonly IReadOnlyList<ArmyUnit> _units;

        public Army(ArmySide side, IReadOnlyList<ArmyUnit> units)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            Side = side;
            _units = units.ToArray();
        }

        public ArmySide Side { get; }

        public IReadOnlyList<ArmyUnit> Units => _units;
    }
}
