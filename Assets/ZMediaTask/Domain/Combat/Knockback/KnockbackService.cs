namespace ZMediaTask.Domain.Combat
{
    public sealed class KnockbackService
    {
        public KnockbackState ApplyImpulse(
            KnockbackState current,
            BattlePoint attackerPos,
            BattlePoint targetPos,
            float impulseStrength)
        {
            var direction = DistanceMetrics.Direction(attackerPos, targetPos);
            var impulse = DistanceMetrics.Scale(direction, impulseStrength);
            return current.WithAddedImpulse(impulse);
        }

        public KnockbackState Decay(
            KnockbackState current,
            float deltaTime,
            float decaySpeed,
            float minThreshold)
        {
            if (!current.HasVelocity)
            {
                return current;
            }

            var magnitude = DistanceMetrics.Magnitude(current.Velocity);
            var reduction = decaySpeed * deltaTime;

            if (magnitude <= reduction || magnitude <= minThreshold)
            {
                return default;
            }

            var newMagnitude = magnitude - reduction;
            var normalized = DistanceMetrics.Normalize(current.Velocity);
            return new KnockbackState(DistanceMetrics.Scale(normalized, newMagnitude));
        }

        public BattlePoint ComputeDisplacement(KnockbackState state, float deltaTime)
        {
            return DistanceMetrics.Scale(state.Velocity, deltaTime);
        }
    }
}
