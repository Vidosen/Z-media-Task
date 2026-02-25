using System;
namespace ZMediaTask.Application.Army
{
    public readonly struct ArmyPair
    {
        public ArmyPair(ZMediaTask.Domain.Army.Army left, ZMediaTask.Domain.Army.Army right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public ZMediaTask.Domain.Army.Army Left { get; }

        public ZMediaTask.Domain.Army.Army Right { get; }
    }
}
