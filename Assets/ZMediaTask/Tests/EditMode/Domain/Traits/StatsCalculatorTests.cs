using NUnit.Framework;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.EditMode.Domain.Traits
{
    public class StatsCalculatorTests
    {
        private static readonly StatBlock BaseStats = new(100, 10, 10, 1);

        [Test]
        public void StatsCalculator_WhenCubeBigRed_ReturnsExpectedStats()
        {
            var calculator = new StatsCalculator(new OfficialTraitCatalog());

            var result = calculator.Calculate(BaseStats, UnitShape.Cube, UnitSize.Big, UnitColor.Red);

            Assert.AreEqual(450, result.HP);
            Assert.AreEqual(60, result.ATK);
            Assert.AreEqual(1, result.SPEED);
            Assert.AreEqual(1, result.ATKSPD);
        }

        [Test]
        public void StatsCalculator_WhenSphereSmallBlue_ReturnsExpectedStats()
        {
            var calculator = new StatsCalculator(new OfficialTraitCatalog());

            var result = calculator.Calculate(BaseStats, UnitShape.Sphere, UnitSize.Small, UnitColor.Blue);

            Assert.AreEqual(100, result.HP);
            Assert.AreEqual(15, result.ATK);
            Assert.AreEqual(20, result.SPEED);
            Assert.AreEqual(5, result.ATKSPD);
        }

        [Test]
        public void StatsCalculator_WhenNegativeResult_ClampsToMinAllowed()
        {
            var calculator = new StatsCalculator(new NegativeTraitCatalog());
            var baseStats = new StatBlock(0, 0, 0, 0);

            var result = calculator.Calculate(baseStats, UnitShape.Cube, UnitSize.Big, UnitColor.Red);

            Assert.AreEqual(0, result.HP);
            Assert.AreEqual(0, result.ATK);
            Assert.AreEqual(0, result.SPEED);
            Assert.AreEqual(0, result.ATKSPD);
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

        private sealed class NegativeTraitCatalog : IUnitTraitCatalog
        {
            public StatModifier GetShapeModifier(UnitShape shape)
            {
                return new StatModifier(-100, -1, -2, -3);
            }

            public StatModifier GetSizeModifier(UnitSize size)
            {
                return new StatModifier(-10, -20, -30, -40);
            }

            public StatModifier GetColorModifier(UnitColor color)
            {
                return new StatModifier(-50, -60, -70, -80);
            }
        }
    }
}
