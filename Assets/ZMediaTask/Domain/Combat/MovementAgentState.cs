using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct MovementAgentState
    {
        private readonly IReadOnlyList<BattlePoint> _currentPath;

        public MovementAgentState(
            int unitId,
            bool isAlive,
            float speed,
            BattlePoint position,
            int? targetId,
            IReadOnlyList<BattlePoint> currentPath,
            BattlePoint? lastPathTargetPosition)
        {
            if (speed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be >= 0.");
            }

            UnitId = unitId;
            IsAlive = isAlive;
            Speed = speed;
            Position = position;
            TargetId = targetId;
            _currentPath = currentPath == null ? Array.Empty<BattlePoint>() : currentPath.ToArray();
            LastPathTargetPosition = lastPathTargetPosition;
        }

        public int UnitId { get; }

        public bool IsAlive { get; }

        public float Speed { get; }

        public BattlePoint Position { get; }

        public int? TargetId { get; }

        public IReadOnlyList<BattlePoint> CurrentPath => _currentPath;

        public BattlePoint? LastPathTargetPosition { get; }

        public MovementAgentState WithPosition(BattlePoint position)
        {
            return new MovementAgentState(
                UnitId, IsAlive, Speed, position, TargetId, CurrentPath, LastPathTargetPosition);
        }
    }
}
