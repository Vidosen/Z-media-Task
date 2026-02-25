using System;

namespace ZMediaTask.Domain.Combat
{
    public sealed class HealthService
    {
        public bool IsDead(CombatUnitState unit)
        {
            return !unit.IsAlive;
        }

        public DamageResult ApplyDamage(CombatUnitState unit, int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be >= 0.");
            }

            var beforeHp = unit.CurrentHp;
            var afterHp = beforeHp - damage;
            if (afterHp < 0)
            {
                afterHp = 0;
            }

            var appliedDamage = beforeHp - afterHp;
            var afterUnit = unit.WithCurrentHp(afterHp);
            var diedNow = beforeHp > 0 && afterHp == 0;

            return new DamageResult(afterUnit, appliedDamage, diedNow);
        }
    }
}
