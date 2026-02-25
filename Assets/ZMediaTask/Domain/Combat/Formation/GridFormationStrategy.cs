using System;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Domain.Combat
{
    public sealed class GridFormationStrategy : IFormationStrategy
    {
        private readonly int _columns;
        private readonly float _rowSpacing;
        private readonly float _columnSpacing;

        public GridFormationStrategy(int columns = 5, float rowSpacing = 1.5f, float columnSpacing = 1.5f)
        {
            if (columns < 1) throw new ArgumentOutOfRangeException(nameof(columns));
            _columns = columns;
            _rowSpacing = rowSpacing;
            _columnSpacing = columnSpacing;
        }

        public BattlePoint ComputePosition(ArmySide side, int index, int totalUnits, float spawnOffsetX)
        {
            var row = index / _columns;
            var col = index % _columns;
            var totalRows = (totalUnits + _columns - 1) / _columns;
            var colsInThisRow = (row < totalRows - 1) ? _columns : totalUnits - row * _columns;

            var colCenter = (colsInThisRow - 1) * 0.5f;
            var z = (col - colCenter) * _columnSpacing;

            var rowCenter = (totalRows - 1) * 0.5f;
            var depthSign = side == ArmySide.Left ? -1f : 1f;
            var x = (side == ArmySide.Left ? -spawnOffsetX : spawnOffsetX)
                    + (row - rowCenter) * _rowSpacing * depthSign;

            return new BattlePoint(x, z);
        }
    }
}
