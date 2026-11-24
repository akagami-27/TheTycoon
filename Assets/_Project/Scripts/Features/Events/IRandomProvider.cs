namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Abstraction for random number generation.
    /// Allows deterministic unit testing for weighted event selection.
    /// </summary>
    public interface IRandomProvider
    {
        int Next(int minInclusive, int maxExclusive);
    }

    /// <summary>
    /// Standard implementation wrapping System.Random.
    /// </summary>
    public class SystemRandomProvider : IRandomProvider
    {
        private readonly System.Random _random;

        public SystemRandomProvider()
        {
            _random = new System.Random();
        }

        public SystemRandomProvider(int seed)
        {
            _random = new System.Random(seed);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
