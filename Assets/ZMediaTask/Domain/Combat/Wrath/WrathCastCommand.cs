using System;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct WrathCastCommand
    {
        public WrathCastCommand(
            ArmySide casterSide,
            BattlePoint center,
            float radius,
            int damage,
            float castTimeSec,
            float impactTimeSec)
        {
            if (radius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be >= 0.");
            }

            CasterSide = casterSide;
            Center = center;
            Radius = radius;
            Damage = damage;
            CastTimeSec = castTimeSec;
            ImpactTimeSec = impactTimeSec;
        }

        public ArmySide CasterSide { get; }

        public BattlePoint Center { get; }

        public float Radius { get; }

        public int Damage { get; }

        public float CastTimeSec { get; }

        public float ImpactTimeSec { get; }
    }
}
