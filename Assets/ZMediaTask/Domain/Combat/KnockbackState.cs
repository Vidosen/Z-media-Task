namespace ZMediaTask.Domain.Combat
{
    public readonly struct KnockbackState
    {
        public KnockbackState(BattlePoint velocity)
        {
            Velocity = velocity;
        }

        public BattlePoint Velocity { get; }

        public bool HasVelocity => Velocity.X != 0f || Velocity.Z != 0f;

        public KnockbackState WithAddedImpulse(BattlePoint impulse)
        {
            return new KnockbackState(DistanceMetrics.Add(Velocity, impulse));
        }
    }
}
