namespace ZMediaTask.Domain.Combat
{
    public readonly struct TargetableUnit
    {
        public TargetableUnit(int unitId, BattlePoint position, bool isAlive)
        {
            UnitId = unitId;
            Position = position;
            IsAlive = isAlive;
        }

        public int UnitId { get; }

        public BattlePoint Position { get; }

        public bool IsAlive { get; }
    }
}
