using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public sealed class WrathService
    {
        private readonly HealthService _healthService;

        public WrathService()
            : this(new HealthService())
        {
        }

        public WrathService(HealthService healthService)
        {
            _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        }

        public WrathMeter AccumulateOnEnemyKill(WrathMeter meter, int chargePerKill)
        {
            if (chargePerKill < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chargePerKill), "Charge per kill must be >= 0.");
            }

            return meter.WithCurrentCharge(meter.CurrentCharge + chargePerKill);
        }

        public bool CanCast(WrathMeter meter)
        {
            return meter.CanCast;
        }

        public WrathMeter Consume(WrathMeter meter)
        {
            return meter.WithCurrentCharge(0);
        }

        public WrathApplyResult ApplyAoe(
            IReadOnlyList<CombatUnitState> units,
            IReadOnlySet<int> affectedUnitIds,
            int damage)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            if (affectedUnitIds == null)
            {
                throw new ArgumentNullException(nameof(affectedUnitIds));
            }

            var effectiveDamage = damage < 0 ? 0 : damage;
            var unitsAfter = new CombatUnitState[units.Count];
            var killed = new List<int>();

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (!affectedUnitIds.Contains(unit.UnitId))
                {
                    unitsAfter[i] = unit;
                    continue;
                }

                var result = _healthService.ApplyDamage(unit, effectiveDamage);
                unitsAfter[i] = result.UnitAfter;
                if (result.DiedNow)
                {
                    killed.Add(unit.UnitId);
                }
            }

            return new WrathApplyResult(unitsAfter, killed);
        }
    }
}
