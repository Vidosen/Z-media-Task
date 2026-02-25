using ZMediaTask.Domain.Random;

namespace ZMediaTask.Infrastructure.Random
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private System.Random _random = new(0);

        public void Reset(int seed)
        {
            _random = new System.Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
