using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    internal readonly struct PendingWrathImpact
    {
        public PendingWrathImpact(WrathCastCommand command)
        {
            Command = command;
        }

        public WrathCastCommand Command { get; }
    }
}
