using System;

namespace ZMediaTask.Domain.Combat
{
    public static class DistanceMetrics
    {
        public static float SqrDistance(BattlePoint a, BattlePoint b)
        {
            var dx = a.X - b.X;
            var dz = a.Z - b.Z;
            return (dx * dx) + (dz * dz);
        }

        public static float Distance(BattlePoint a, BattlePoint b)
        {
            return (float)Math.Sqrt(SqrDistance(a, b));
        }

        public static BattlePoint Direction(BattlePoint from, BattlePoint to)
        {
            return Normalize(Subtract(to, from));
        }

        public static BattlePoint Normalize(BattlePoint vector)
        {
            var magnitude = Magnitude(vector);
            if (magnitude <= 0f)
            {
                return new BattlePoint(0f, 0f);
            }

            return Scale(vector, 1f / magnitude);
        }

        public static BattlePoint MoveTowards(BattlePoint current, BattlePoint target, float maxDistanceDelta)
        {
            if (maxDistanceDelta <= 0f)
            {
                return current;
            }

            var toTarget = Subtract(target, current);
            var distance = Magnitude(toTarget);
            if (distance <= maxDistanceDelta || distance <= 0f)
            {
                return target;
            }

            var direction = Scale(toTarget, 1f / distance);
            return Add(current, Scale(direction, maxDistanceDelta));
        }

        public static BattlePoint Add(BattlePoint a, BattlePoint b)
        {
            return new BattlePoint(a.X + b.X, a.Z + b.Z);
        }

        public static BattlePoint Subtract(BattlePoint a, BattlePoint b)
        {
            return new BattlePoint(a.X - b.X, a.Z - b.Z);
        }

        public static BattlePoint Scale(BattlePoint value, float factor)
        {
            return new BattlePoint(value.X * factor, value.Z * factor);
        }

        public static float Magnitude(BattlePoint value)
        {
            return (float)Math.Sqrt((value.X * value.X) + (value.Z * value.Z));
        }
    }
}
