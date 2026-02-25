using System;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Application.Army
{
    public sealed class ArmyRandomizationUseCase
    {
        private static readonly StatBlock DefaultBaseStats = new(100, 10, 10, 1);

        private readonly ArmyFactory _armyFactory;
        private readonly IRandomProvider _randomProvider;

        public ArmyRandomizationUseCase(ArmyFactory armyFactory, IRandomProvider randomProvider)
        {
            _armyFactory = armyFactory ?? throw new ArgumentNullException(nameof(armyFactory));
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        public ArmyPair RandomizeBoth(int seed, int unitsPerArmy = 20)
        {
            if (unitsPerArmy < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitsPerArmy), "Units per army must be >= 0.");
            }

            _randomProvider.Reset(seed);

            var left = _armyFactory.Create(ArmySide.Left, unitsPerArmy, DefaultBaseStats, _randomProvider);
            var right = _armyFactory.Create(ArmySide.Right, unitsPerArmy, DefaultBaseStats, _randomProvider);

            return new ArmyPair(left, right);
        }
    }
}
