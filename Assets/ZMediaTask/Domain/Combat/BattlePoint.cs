namespace ZMediaTask.Domain.Combat
{
    public readonly struct BattlePoint
    {
        public BattlePoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public float X { get; }

        public float Z { get; }
    }
}
