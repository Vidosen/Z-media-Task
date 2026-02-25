using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct WrathMeter
    {
        public WrathMeter(int currentCharge, int maxCharge)
        {
            if (maxCharge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCharge), "Max charge must be >= 0.");
            }

            if (currentCharge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentCharge), "Current charge must be >= 0.");
            }

            CurrentCharge = currentCharge > maxCharge ? maxCharge : currentCharge;
            MaxCharge = maxCharge;
        }

        public int CurrentCharge { get; }

        public int MaxCharge { get; }

        public bool CanCast => CurrentCharge >= MaxCharge;

        public float Normalized => MaxCharge <= 0 ? 1f : (float)CurrentCharge / MaxCharge;

        public WrathMeter WithCurrentCharge(int currentCharge)
        {
            return new WrathMeter(currentCharge, MaxCharge);
        }
    }
}
