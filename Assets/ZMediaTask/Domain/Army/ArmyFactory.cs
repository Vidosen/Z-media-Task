using System;
using System.Collections.Generic;
using System.Linq;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Domain.Army
{
    public sealed class ArmyFactory
    {
        private static readonly UnitShape[] Shapes = Enum.GetValues(typeof(UnitShape)).Cast<UnitShape>().ToArray();
        private static readonly UnitSize[] Sizes = Enum.GetValues(typeof(UnitSize)).Cast<UnitSize>().ToArray();
        private static readonly UnitColor[] Colors = Enum.GetValues(typeof(UnitColor)).Cast<UnitColor>().ToArray();

        private readonly StatsCalculator _statsCalculator;
        private readonly IUnitTraitWeightCatalog _weights;

        public ArmyFactory(StatsCalculator statsCalculator, IUnitTraitWeightCatalog weights)
        {
            _statsCalculator = statsCalculator ?? throw new ArgumentNullException(nameof(statsCalculator));
            _weights = weights ?? throw new ArgumentNullException(nameof(weights));
        }

        public Army Create(ArmySide side, int unitCount, StatBlock baseStats, IRandomProvider randomProvider)
        {
            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitCount), "Unit count must be >= 0.");
            }

            if (randomProvider == null)
            {
                throw new ArgumentNullException(nameof(randomProvider));
            }

            var units = new List<ArmyUnit>(unitCount);
            for (var index = 0; index < unitCount; index++)
            {
                var shape = PickWeighted(Shapes, _weights.GetShapeWeight, randomProvider, "shape");
                var size = PickWeighted(Sizes, _weights.GetSizeWeight, randomProvider, "size");
                var color = PickWeighted(Colors, _weights.GetColorWeight, randomProvider, "color");
                var stats = _statsCalculator.Calculate(baseStats, shape, size, color);

                units.Add(new ArmyUnit(shape, size, color, stats));
            }

            return new Army(side, units);
        }

        private static TEnum PickWeighted<TEnum>(
            IReadOnlyList<TEnum> values,
            Func<TEnum, int> weightAccessor,
            IRandomProvider randomProvider,
            string sectionName)
        {
            var totalWeight = 0;
            for (var i = 0; i < values.Count; i++)
            {
                var weight = weightAccessor(values[i]);
                if (weight < 0)
                {
                    throw new InvalidOperationException(
                        $"Weight for {sectionName} '{values[i]}' cannot be negative.");
                }

                totalWeight += weight;
            }

            if (totalWeight <= 0)
            {
                throw new InvalidOperationException(
                    $"Total weight for {sectionName} must be greater than zero.");
            }

            var roll = randomProvider.NextInt(0, totalWeight);
            var cumulative = 0;

            for (var i = 0; i < values.Count; i++)
            {
                cumulative += weightAccessor(values[i]);
                if (roll < cumulative)
                {
                    return values[i];
                }
            }

            return values[values.Count - 1];
        }
    }
}
