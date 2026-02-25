using System;
using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct MovementTickContext
    {
        public MovementTickContext(
            float deltaTime,
            IReadOnlyList<MovementAgentState> allies,
            IReadOnlyList<TargetableUnit> enemies,
            MovementConfig config)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be >= 0.");
            }

            DeltaTime = deltaTime;
            Allies = allies ?? throw new ArgumentNullException(nameof(allies));
            Enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            Config = config;
        }

        public float DeltaTime { get; }

        public IReadOnlyList<MovementAgentState> Allies { get; }

        public IReadOnlyList<TargetableUnit> Enemies { get; }

        public MovementConfig Config { get; }
    }
}
