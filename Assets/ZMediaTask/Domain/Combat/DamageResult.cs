namespace ZMediaTask.Domain.Combat
{
    public readonly struct DamageResult
    {
        public DamageResult(CombatUnitState unitAfter, int damageApplied, bool diedNow)
        {
            UnitAfter = unitAfter;
            DamageApplied = damageApplied;
            DiedNow = diedNow;
        }

        public CombatUnitState UnitAfter { get; }

        public int DamageApplied { get; }

        public bool DiedNow { get; }
    }
}
