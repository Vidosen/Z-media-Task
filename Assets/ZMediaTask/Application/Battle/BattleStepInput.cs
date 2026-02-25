using System;

namespace ZMediaTask.Application.Battle
{
    public readonly struct BattleStepInput
    {
        public BattleStepInput(BattleContext context, float deltaTimeSec, float currentTimeSec)
        {
            if (deltaTimeSec < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSec), "Delta time must be >= 0.");
            }

            Context = context;
            DeltaTimeSec = deltaTimeSec;
            CurrentTimeSec = currentTimeSec;
        }

        public BattleContext Context { get; }

        public float DeltaTimeSec { get; }

        public float CurrentTimeSec { get; }
    }
}
