using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public readonly struct BattleEvent
    {
        public BattleEvent(
            BattleEventKind kind,
            float timeSec,
            int? unitId,
            ArmySide? side,
            WrathCastCommand? cast,
            int? affectedCount)
        {
            Kind = kind;
            TimeSec = timeSec;
            UnitId = unitId;
            Side = side;
            Cast = cast;
            AffectedCount = affectedCount;
        }

        public BattleEventKind Kind { get; }

        public float TimeSec { get; }

        public int? UnitId { get; }

        public ArmySide? Side { get; }

        public WrathCastCommand? Cast { get; }

        public int? AffectedCount { get; }
    }
}
