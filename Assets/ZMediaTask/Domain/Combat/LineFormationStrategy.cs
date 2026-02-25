using ZMediaTask.Domain.Army;

namespace ZMediaTask.Domain.Combat
{
    public sealed class LineFormationStrategy : IFormationStrategy
    {
        private readonly float _spacing;

        public LineFormationStrategy(float spacing = 1.5f)
        {
            _spacing = spacing;
        }

        public BattlePoint ComputePosition(ArmySide side, int index, int totalUnits, float spawnOffsetX)
        {
            var x = side == ArmySide.Left ? -spawnOffsetX : spawnOffsetX;
            var centerOffset = (totalUnits - 1) * 0.5f;
            var z = (index - centerOffset) * _spacing;
            return new BattlePoint(x, z);
        }
    }
}
