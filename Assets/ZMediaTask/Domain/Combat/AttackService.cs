using System;

namespace ZMediaTask.Domain.Combat
{
    public sealed class AttackService
    {
        private readonly HealthService _healthService;

        public AttackService(HealthService healthService)
        {
            _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        }

        public bool CanAttack(CombatUnitState attacker, CombatUnitState target, float currentTimeSec,
            AttackConfig config)
        {
            return !GetFailureReason(attacker, target, currentTimeSec, config).HasValue;
        }

        public AttackResult TryAttack(CombatUnitState attacker, CombatUnitState target, float currentTimeSec,
            AttackConfig config)
        {
            var failure = GetFailureReason(attacker, target, currentTimeSec, config);
            if (failure.HasValue) return new AttackResult(failure, attacker, target, 0);

            var damageResult = _healthService.ApplyDamage(target, attacker.Attack);
            var nextAttackTime = CooldownTracker.ComputeNextAttackTime(
                currentTimeSec,
                attacker.AttackSpeed,
                config.BaseAttackDelay);
            var attackerAfter = attacker.WithNextAttackTimeSec(nextAttackTime);

            return new AttackResult(
                null,
                attackerAfter,
                damageResult.UnitAfter,
                damageResult.DamageApplied);
        }

        private AttackFailureReason? GetFailureReason(CombatUnitState attacker, CombatUnitState target,
            float currentTimeSec, AttackConfig config)
        {
            if (!attacker.IsAlive) return AttackFailureReason.AttackerDead;

            if (!target.IsAlive) return AttackFailureReason.TargetDead;

            if (!IsInRange(attacker, target, config.AttackRange)) return AttackFailureReason.OutOfRange;

            if (!CooldownTracker.IsReady(currentTimeSec, attacker.NextAttackTimeSec))
                return AttackFailureReason.CooldownNotReady;

            return null;
        }

        private static bool IsInRange(CombatUnitState attacker, CombatUnitState target, float range)
        {
            return DistanceMetrics.Distance(attacker.Position, target.Position) <= range;
        }
    }
}