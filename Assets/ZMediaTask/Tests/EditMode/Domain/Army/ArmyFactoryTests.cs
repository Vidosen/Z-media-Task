using System;
using NUnit.Framework;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.EditMode.Domain.Army
{
    public class ArmyFactoryTests
    {
        private static readonly StatBlock BaseStats = new(100, 10, 10, 1);

        [Test]
        public void ArmyFactory_Generates20Units()
        {
            var factory = new ArmyFactory(
                new StatsCalculator(new OfficialTraitCatalog()),
                new UniformWeightCatalog());

            var army = factory.Create(ArmySide.Left, 20, BaseStats, new TestRandomProvider(12345));

            Assert.AreEqual(ArmySide.Left, army.Side);
            Assert.AreEqual(20, army.Units.Count);

            foreach (var unit in army.Units)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(UnitShape), unit.Shape));
                Assert.IsTrue(Enum.IsDefined(typeof(UnitSize), unit.Size));
                Assert.IsTrue(Enum.IsDefined(typeof(UnitColor), unit.Color));
                Assert.GreaterOrEqual(unit.Stats.HP, 0);
                Assert.GreaterOrEqual(unit.Stats.ATK, 0);
                Assert.GreaterOrEqual(unit.Stats.SPEED, 0);
                Assert.GreaterOrEqual(unit.Stats.ATKSPD, 0);
            }
        }

        [Test]
        public void ArmyFactory_WhenOnlyOnePositiveWeight_UsesOnlyWeightedTraits()
        {
            var factory = new ArmyFactory(
                new StatsCalculator(new OfficialTraitCatalog()),
                new SingleTraitWeightCatalog());

            var army = factory.Create(ArmySide.Right, 20, BaseStats, new TestRandomProvider(42));

            Assert.AreEqual(20, army.Units.Count);
            foreach (var unit in army.Units)
            {
                Assert.AreEqual(UnitShape.Sphere, unit.Shape);
                Assert.AreEqual(UnitSize.Big, unit.Size);
                Assert.AreEqual(UnitColor.Red, unit.Color);
            }
        }

        private sealed class UniformWeightCatalog : IUnitTraitWeightCatalog
        {
            public int GetShapeWeight(UnitShape shape) => 1;

            public int GetSizeWeight(UnitSize size) => 1;

            public int GetColorWeight(UnitColor color) => 1;
        }

        private sealed class SingleTraitWeightCatalog : IUnitTraitWeightCatalog
        {
            public int GetShapeWeight(UnitShape shape) => shape == UnitShape.Sphere ? 1 : 0;

            public int GetSizeWeight(UnitSize size) => size == UnitSize.Big ? 1 : 0;

            public int GetColorWeight(UnitColor color) => color == UnitColor.Red ? 1 : 0;
        }

        private sealed class TestRandomProvider : IRandomProvider
        {
            private Random _random;

            public TestRandomProvider(int seed)
            {
                _random = new Random(seed);
            }

            public void Reset(int seed)
            {
                _random = new Random(seed);
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                return _random.Next(minInclusive, maxExclusive);
            }
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
