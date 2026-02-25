using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct KnockbackConfig
    {
        public KnockbackConfig(float impulseStrength, float decaySpeed, float minVelocityThreshold)
        {
            if (impulseStrength < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(impulseStrength),
                    "Impulse strength must be >= 0.");
            }

            if (decaySpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(decaySpeed),
                    "Decay speed must be >= 0.");
            }

            if (minVelocityThreshold < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minVelocityThreshold),
                    "Min velocity threshold must be >= 0.");
            }

            ImpulseStrength = impulseStrength;
            DecaySpeed = decaySpeed;
            MinVelocityThreshold = minVelocityThreshold;
        }

        public float ImpulseStrength { get; }

        public float DecaySpeed { get; }

        public float MinVelocityThreshold { get; }
    }
}
