namespace ZMediaTask.Domain.Combat
{
    public enum AttackFailureReason
    {
        AttackerDead = 0,
        TargetDead = 1,
        OutOfRange = 2,
        CooldownNotReady = 3
    }
}
