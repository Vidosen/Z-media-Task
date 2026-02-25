using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMediaTask.Domain.Army
{
    public sealed class Army
    {
        public Army(ArmySide side, IReadOnlyList<ArmyUnit> units)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            Side = side;
            Units = units.ToArray();
        }

        public ArmySide Side { get; }

        public IReadOnlyList<ArmyUnit> Units { get; }
    }
}
