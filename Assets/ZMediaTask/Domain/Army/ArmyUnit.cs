using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Domain.Army
{
    public readonly struct ArmyUnit
    {
        public ArmyUnit(UnitShape shape, UnitSize size, UnitColor color, StatBlock stats)
        {
            Shape = shape;
            Size = size;
            Color = color;
            Stats = stats;
        }

        public UnitShape Shape { get; }

        public UnitSize Size { get; }

        public UnitColor Color { get; }

        public StatBlock Stats { get; }
    }
}
