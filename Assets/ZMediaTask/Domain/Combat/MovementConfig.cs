using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct MovementConfig
    {
        public MovementConfig(float meleeRange, float repathDistanceThreshold, float steeringRadius, float slotRadius)
        {
            if (meleeRange < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(meleeRange), "Melee range must be >= 0.");
            }

            if (repathDistanceThreshold < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(repathDistanceThreshold),
                    "Repath distance threshold must be >= 0.");
            }

            if (steeringRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(steeringRadius), "Steering radius must be >= 0.");
            }

            if (slotRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(slotRadius), "Slot radius must be >= 0.");
            }

            MeleeRange = meleeRange;
            RepathDistanceThreshold = repathDistanceThreshold;
            SteeringRadius = steeringRadius;
            SlotRadius = slotRadius;
        }

        public float MeleeRange { get; }

        public float RepathDistanceThreshold { get; }

        public float SteeringRadius { get; }

        public float SlotRadius { get; }
    }
}
