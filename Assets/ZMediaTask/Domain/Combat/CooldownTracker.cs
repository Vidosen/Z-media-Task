using System;

namespace ZMediaTask.Domain.Combat
{
    public sealed class CooldownTracker
    {
        public static bool IsReady(float currentTimeSec, float nextAttackTimeSec)
        {
            return currentTimeSec >= nextAttackTimeSec;
        }

        public static float ComputeNextAttackTime(float currentTimeSec, int attackSpeed, float baseAttackDelay)
        {
            if (attackSpeed < 0)
                throw new ArgumentOutOfRangeException(nameof(attackSpeed), "Attack speed must be >= 0.");

            if (baseAttackDelay < 0f)
                throw new ArgumentOutOfRangeException(nameof(baseAttackDelay), "Base attack delay must be >= 0.");

            return currentTimeSec + (baseAttackDelay * attackSpeed);
        }
    }
}
