using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct WrathConfig
    {
        public WrathConfig(int chargePerKill, int maxCharge, float radius, int damage, float impactDelaySeconds)
        {
            if (chargePerKill < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chargePerKill), "Charge per kill must be >= 0.");
            }

            if (maxCharge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCharge), "Max charge must be >= 0.");
            }

            if (radius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be >= 0.");
            }

            if (impactDelaySeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(impactDelaySeconds), "Impact delay must be >= 0.");
            }

            ChargePerKill = chargePerKill;
            MaxCharge = maxCharge;
            Radius = radius;
            Damage = damage;
            ImpactDelaySeconds = impactDelaySeconds;
        }

        public int ChargePerKill { get; }

        public int MaxCharge { get; }

        public float Radius { get; }

        public int Damage { get; }

        public float ImpactDelaySeconds { get; }
    }
}
