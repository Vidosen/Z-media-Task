namespace ZMediaTask.Domain.Combat
{
    public readonly struct AttackResult
    {
        public AttackResult(
            AttackFailureReason? failure,
            CombatUnitState attackerAfter,
            CombatUnitState targetAfter,
            int damageApplied)
        {
            Failure = failure;
            AttackerAfter = attackerAfter;
            TargetAfter = targetAfter;
            DamageApplied = damageApplied;
        }

        public bool Success => !Failure.HasValue;

        public AttackFailureReason? Failure { get; }

        public CombatUnitState AttackerAfter { get; }

        public CombatUnitState TargetAfter { get; }

        public int DamageApplied { get; }
    }
}
