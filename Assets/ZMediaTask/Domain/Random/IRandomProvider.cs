namespace ZMediaTask.Domain.Random
{
    public interface IRandomProvider
    {
        void Reset(int seed);

        int NextInt(int minInclusive, int maxExclusive);
    }
}
