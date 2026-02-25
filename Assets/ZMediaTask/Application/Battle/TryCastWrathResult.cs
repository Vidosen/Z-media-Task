using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public readonly struct TryCastWrathResult
    {
        public TryCastWrathResult(
            bool success,
            WrathMeter meterAfter,
            WrathCastCommand? command,
            WrathCastFailureReason? failure)
        {
            Success = success;
            MeterAfter = meterAfter;
            Command = command;
            Failure = failure;
        }

        public bool Success { get; }

        public WrathMeter MeterAfter { get; }

        public WrathCastCommand? Command { get; }

        public WrathCastFailureReason? Failure { get; }
    }
}
