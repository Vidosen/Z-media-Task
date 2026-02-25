using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Application.Battle
{
    public readonly struct BattleUnitRuntime
    {
        public BattleUnitRuntime(
            int unitId,
            ArmySide side,
            UnitShape shape,
            UnitSize size,
            UnitColor color,
            MovementAgentState movement,
            CombatUnitState combat)
        {
            UnitId = unitId;
            Side = side;
            Shape = shape;
            Size = size;
            Color = color;
            Movement = movement;
            Combat = combat;
        }

        public int UnitId { get; }

        public ArmySide Side { get; }

        public UnitShape Shape { get; }

        public UnitSize Size { get; }

        public UnitColor Color { get; }

        public MovementAgentState Movement { get; }

        public CombatUnitState Combat { get; }

        public BattleUnitRuntime WithMovement(MovementAgentState movement)
        {
            return new BattleUnitRuntime(UnitId, Side, Shape, Size, Color, movement, Combat);
        }

        public BattleUnitRuntime WithCombat(CombatUnitState combat)
        {
            return new BattleUnitRuntime(UnitId, Side, Shape, Size, Color, Movement, combat);
        }
    }
}
