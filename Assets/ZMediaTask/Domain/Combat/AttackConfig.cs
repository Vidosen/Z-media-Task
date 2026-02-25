using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct AttackConfig
    {
        public AttackConfig(float attackRange, float baseAttackDelay)
        {
            if (attackRange < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(attackRange), "Attack range must be >= 0.");
            }

            if (baseAttackDelay < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAttackDelay), "Base attack delay must be >= 0.");
            }

            AttackRange = attackRange;
            BaseAttackDelay = baseAttackDelay;
        }

        public float AttackRange { get; }

        public float BaseAttackDelay { get; }
    }
}
