using System;
using System.Collections.Generic;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public readonly struct BattleContext
    {
        private readonly IReadOnlyList<BattleUnitRuntime> _units;
        private readonly IReadOnlyDictionary<ArmySide, WrathMeter> _wrathMeters;

        public BattleContext(IReadOnlyList<BattleUnitRuntime> units, float elapsedTimeSec, ArmySide? winnerSide)
            : this(units, elapsedTimeSec, winnerSide, EmptyWrathMeters)
        {
        }

        public BattleContext(
            IReadOnlyList<BattleUnitRuntime> units,
            float elapsedTimeSec,
            ArmySide? winnerSide,
            IReadOnlyDictionary<ArmySide, WrathMeter> wrathMeters)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            if (elapsedTimeSec < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedTimeSec), "Elapsed time must be >= 0.");
            }

            if (wrathMeters == null)
            {
                throw new ArgumentNullException(nameof(wrathMeters));
            }

            var copied = new BattleUnitRuntime[units.Count];
            for (var i = 0; i < units.Count; i++)
            {
                copied[i] = units[i];
            }

            var wrathMetersCopy = new Dictionary<ArmySide, WrathMeter>();
            foreach (var pair in wrathMeters)
            {
                wrathMetersCopy[pair.Key] = pair.Value;
            }

            _units = copied;
            _wrathMeters = wrathMetersCopy;
            ElapsedTimeSec = elapsedTimeSec;
            WinnerSide = winnerSide;
        }

        private static IReadOnlyDictionary<ArmySide, WrathMeter> EmptyWrathMeters { get; }
            = new Dictionary<ArmySide, WrathMeter>();

        public static BattleContext Empty { get; } = new(Array.Empty<BattleUnitRuntime>(), 0f, null);

        public IReadOnlyList<BattleUnitRuntime> Units => _units ?? Array.Empty<BattleUnitRuntime>();

        public float ElapsedTimeSec { get; }

        public ArmySide? WinnerSide { get; }

        public IReadOnlyDictionary<ArmySide, WrathMeter> WrathMeters => _wrathMeters ?? EmptyWrathMeters;
    }
}
