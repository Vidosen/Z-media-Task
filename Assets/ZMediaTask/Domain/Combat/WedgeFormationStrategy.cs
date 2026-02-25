using ZMediaTask.Domain.Army;

namespace ZMediaTask.Domain.Combat
{
    public sealed class WedgeFormationStrategy : IFormationStrategy
    {
        private readonly float _depthSpacing;
        private readonly float _widthSpacing;

        public WedgeFormationStrategy(float depthSpacing = 1.2f, float widthSpacing = 1.0f)
        {
            _depthSpacing = depthSpacing;
            _widthSpacing = widthSpacing;
        }

        public BattlePoint ComputePosition(ArmySide side, int index, int totalUnits, float spawnOffsetX)
        {
            var baseX = side == ArmySide.Left ? -spawnOffsetX : spawnOffsetX;

            if (index == 0)
            {
                return new BattlePoint(baseX, 0f);
            }

            var depthSign = side == ArmySide.Left ? -1f : 1f;
            var rowFromTip = (index + 1) / 2;
            var isLeftSide = index % 2 == 1;

            var x = baseX + rowFromTip * _depthSpacing * depthSign;
            var z = rowFromTip * _widthSpacing * (isLeftSide ? -1f : 1f);

            return new BattlePoint(x, z);
        }
    }
}
