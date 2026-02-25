using System.Linq;
using NUnit.Framework;
using ZMediaTask.Application.Army;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Infrastructure.Random;

namespace ZMediaTask.Tests.EditMode.Application.Army
{
    public class ArmyRandomizationUseCaseTests
    {
        [Test]
        public void ArmyRandomizer_WithSameSeed_GeneratesSameComposition()
        {
            var useCase = CreateUseCase();

            var first = useCase.RandomizeBoth(12345);
            var second = useCase.RandomizeBoth(12345);

            Assert.AreEqual(ToComposition(first), ToComposition(second));
        }

        [Test]
        public void ArmyRandomizer_ChangesComposition_AfterNewSeed()
        {
            var useCase = CreateUseCase();

            var first = useCase.RandomizeBoth(12345);
            var second = useCase.RandomizeBoth(12346);

            Assert.AreNotEqual(ToComposition(first), ToComposition(second));
        }

        private static ArmyRandomizationUseCase CreateUseCase()
        {
            var statsCalculator = new StatsCalculator(new OfficialTraitCatalog());
            var factory = new ArmyFactory(statsCalculator, new UniformWeightCatalog());
            return new ArmyRandomizationUseCase(factory, new SystemRandomProvider());
        }

        private static string ToComposition(ArmyPair armyPair)
        {
            return string.Join("|", SerializeArmy(armyPair.Left).Concat(SerializeArmy(armyPair.Right)));
        }

        private static string[] SerializeArmy(ZMediaTask.Domain.Army.Army army)
        {
            return army.Units
                .Select(unit => $"{(int)unit.Shape}-{(int)unit.Size}-{(int)unit.Color}")
                .ToArray();
        }

        private sealed class UniformWeightCatalog : IUnitTraitWeightCatalog
        {
            public int GetShapeWeight(UnitShape shape) => 1;

            public int GetSizeWeight(UnitSize size) => 1;

            public int GetColorWeight(UnitColor color) => 1;
        }

        private sealed class OfficialTraitCatalog : IUnitTraitCatalog
        {
            public StatModifier GetShapeModifier(UnitShape shape)
            {
                return shape switch
                {
                    UnitShape.Cube => new StatModifier(100, 10, 0, 0),
                    UnitShape.Sphere => new StatModifier(50, 20, 0, 0),
                    _ => StatModifier.Zero
                };
            }

            public StatModifier GetSizeModifier(UnitSize size)
            {
                return size switch
                {
                    UnitSize.Small => new StatModifier(-50, 0, 0, 0),
                    UnitSize.Big => new StatModifier(50, 0, 0, 0),
                    _ => StatModifier.Zero
                };
            }

            public StatModifier GetColorModifier(UnitColor color)
            {
                return color switch
                {
                    UnitColor.Blue => new StatModifier(0, -15, 10, 4),
                    UnitColor.Green => new StatModifier(-100, 20, -5, 0),
                    UnitColor.Red => new StatModifier(200, 40, -9, 0),
                    _ => StatModifier.Zero
                };
            }
        }
    }
}
