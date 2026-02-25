using ZMediaTask.Domain.Army;

namespace ZMediaTask.Domain.Combat
{
    public interface IFormationStrategy
    {
        BattlePoint ComputePosition(ArmySide side, int index, int totalUnits, float spawnOffsetX);
    }
}
