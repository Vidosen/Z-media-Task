using System.Collections.Generic;
using NUnit.Framework;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class FormationStrategyTests
    {
        private const float SpawnOffsetX = 8f;
        private const int UnitCount = 20;

        [Test]
        public void LineFormation_SingleUnit_PlacedAtCenter()
        {
            var strategy = new LineFormationStrategy();
            var pos = strategy.ComputePosition(ArmySide.Left, 0, 1, SpawnOffsetX);

            Assert.AreEqual(-SpawnOffsetX, pos.X, 0.001f);
            Assert.AreEqual(0f, pos.Z, 0.001f);
        }

        [Test]
        public void LineFormation_TwoUnits_SymmetricAroundZ0()
        {
            var strategy = new LineFormationStrategy(spacing: 2f);

            var a = strategy.ComputePosition(ArmySide.Left, 0, 2, SpawnOffsetX);
            var b = strategy.ComputePosition(ArmySide.Left, 1, 2, SpawnOffsetX);

            Assert.AreEqual(-a.Z, b.Z, 0.001f);
        }

        [Test]
        public void LineFormation_LeftSide_NegativeX()
        {
            var strategy = new LineFormationStrategy();
            var pos = strategy.ComputePosition(ArmySide.Left, 0, 1, SpawnOffsetX);

            Assert.Less(pos.X, 0f);
        }

        [Test]
        public void LineFormation_RightSide_PositiveX()
        {
            var strategy = new LineFormationStrategy();
            var pos = strategy.ComputePosition(ArmySide.Right, 0, 1, SpawnOffsetX);

            Assert.Greater(pos.X, 0f);
        }

        [Test]
        public void LineFormation_MatchesLegacyBehavior()
        {
            var strategy = new LineFormationStrategy(spacing: 1.5f);
            var centerOffset = (UnitCount - 1) * 0.5f;

            for (var i = 0; i < UnitCount; i++)
            {
                var pos = strategy.ComputePosition(ArmySide.Left, i, UnitCount, SpawnOffsetX);
                var expectedZ = (i - centerOffset) * 1.5f;

                Assert.AreEqual(-SpawnOffsetX, pos.X, 0.001f);
                Assert.AreEqual(expectedZ, pos.Z, 0.001f);
            }
        }

        [Test]
        public void GridFormation_20Units_5Columns_Produces4Rows()
        {
            var strategy = new GridFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);
            var positions = ComputeAll(strategy, ArmySide.Left, UnitCount);

            Assert.AreEqual(UnitCount, positions.Count);
            AssertAllUnique(positions);
        }

        [Test]
        public void GridFormation_AllPositionsUnique()
        {
            var strategy = new GridFormationStrategy(columns: 5);
            var positions = ComputeAll(strategy, ArmySide.Left, UnitCount);

            AssertAllUnique(positions);
        }

        [Test]
        public void GridFormation_LeftArmy_RowsExtendAwayFromEnemy()
        {
            var strategy = new GridFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);

            var frontRow = strategy.ComputePosition(ArmySide.Left, 0, UnitCount, SpawnOffsetX);
            var backRow = strategy.ComputePosition(ArmySide.Left, 15, UnitCount, SpawnOffsetX);

            Assert.Greater(frontRow.X, backRow.X, "Back rows should extend further left (away from enemy)");
        }

        [Test]
        public void GridFormation_RightArmy_RowsExtendAwayFromEnemy()
        {
            var strategy = new GridFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);

            var frontRow = strategy.ComputePosition(ArmySide.Right, 0, UnitCount, SpawnOffsetX);
            var backRow = strategy.ComputePosition(ArmySide.Right, 15, UnitCount, SpawnOffsetX);

            Assert.Less(frontRow.X, backRow.X, "Back rows should extend further right (away from enemy)");
        }

        [Test]
        public void GridFormation_ColumnsSymmetricAroundZ0()
        {
            var strategy = new GridFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);

            var first = strategy.ComputePosition(ArmySide.Left, 0, UnitCount, SpawnOffsetX);
            var last = strategy.ComputePosition(ArmySide.Left, 4, UnitCount, SpawnOffsetX);

            Assert.AreEqual(-first.Z, last.Z, 0.001f);
        }

        [Test]
        public void WedgeFormation_FirstUnit_AtTip()
        {
            var strategy = new WedgeFormationStrategy();
            var tip = strategy.ComputePosition(ArmySide.Left, 0, UnitCount, SpawnOffsetX);

            Assert.AreEqual(-SpawnOffsetX, tip.X, 0.001f);
            Assert.AreEqual(0f, tip.Z, 0.001f);
        }

        [Test]
        public void WedgeFormation_UnitsAlternateLeftRight()
        {
            var strategy = new WedgeFormationStrategy();

            var unit1 = strategy.ComputePosition(ArmySide.Left, 1, UnitCount, SpawnOffsetX);
            var unit2 = strategy.ComputePosition(ArmySide.Left, 2, UnitCount, SpawnOffsetX);

            Assert.Less(unit1.Z, 0f, "Odd index should be on negative Z side");
            Assert.Greater(unit2.Z, 0f, "Even index should be on positive Z side");
        }

        [Test]
        public void WedgeFormation_SymmetricPairs()
        {
            var strategy = new WedgeFormationStrategy();

            var unit1 = strategy.ComputePosition(ArmySide.Left, 1, UnitCount, SpawnOffsetX);
            var unit2 = strategy.ComputePosition(ArmySide.Left, 2, UnitCount, SpawnOffsetX);

            Assert.AreEqual(unit1.X, unit2.X, 0.001f, "Pair should share same X depth");
            Assert.AreEqual(-unit1.Z, unit2.Z, 0.001f, "Pair should be symmetric on Z");
        }

        [Test]
        public void WedgeFormation_20Units_AllPositionsUnique()
        {
            var strategy = new WedgeFormationStrategy();
            var positions = ComputeAll(strategy, ArmySide.Left, UnitCount);

            AssertAllUnique(positions);
        }

        [Test]
        public void StaggeredFormation_OddRowsOffset()
        {
            var strategy = new StaggeredFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);

            var evenRowFirst = strategy.ComputePosition(ArmySide.Left, 0, UnitCount, SpawnOffsetX);
            var oddRowFirst = strategy.ComputePosition(ArmySide.Left, 5, UnitCount, SpawnOffsetX);

            Assert.AreNotEqual(evenRowFirst.Z, oddRowFirst.Z, "Odd row should be offset from even row");
        }

        [Test]
        public void StaggeredFormation_EvenRowsNotOffset()
        {
            var strategy = new StaggeredFormationStrategy(columns: 5, rowSpacing: 1.5f, columnSpacing: 1.5f);

            var row0First = strategy.ComputePosition(ArmySide.Left, 0, UnitCount, SpawnOffsetX);
            var row2First = strategy.ComputePosition(ArmySide.Left, 10, UnitCount, SpawnOffsetX);

            Assert.AreEqual(row0First.Z, row2First.Z, 0.001f, "Even rows should have same Z start");
        }

        [Test]
        public void StaggeredFormation_20Units_AllPositionsUnique()
        {
            var strategy = new StaggeredFormationStrategy(columns: 5);
            var positions = ComputeAll(strategy, ArmySide.Left, UnitCount);

            AssertAllUnique(positions);
        }

        [Test]
        public void AllStrategies_ProduceUniquePositions_For20Units()
        {
            var strategies = new IFormationStrategy[]
            {
                new LineFormationStrategy(),
                new GridFormationStrategy(columns: 5),
                new WedgeFormationStrategy(),
                new StaggeredFormationStrategy(columns: 5)
            };

            foreach (var strategy in strategies)
            {
                var positions = ComputeAll(strategy, ArmySide.Left, UnitCount);
                AssertAllUnique(positions);
            }
        }

        [Test]
        public void Picker_NeverReturnsLine()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var strategy = FormationStrategyPicker.PickRandom(seed);
                Assert.IsNotInstanceOf<LineFormationStrategy>(strategy,
                    $"Seed {seed} produced Line formation");
            }
        }

        [Test]
        public void Picker_ProducesAllNonLineTypes()
        {
            var seenGrid = false;
            var seenWedge = false;
            var seenStaggered = false;

            for (var seed = 0; seed < 100; seed++)
            {
                var strategy = FormationStrategyPicker.PickRandom(seed);
                if (strategy is GridFormationStrategy) seenGrid = true;
                if (strategy is WedgeFormationStrategy) seenWedge = true;
                if (strategy is StaggeredFormationStrategy) seenStaggered = true;
            }

            Assert.IsTrue(seenGrid, "Picker never produced Grid");
            Assert.IsTrue(seenWedge, "Picker never produced Wedge");
            Assert.IsTrue(seenStaggered, "Picker never produced Staggered");
        }

        [Test]
        public void AllStrategies_LeftArmyNegativeX_RightArmyPositiveX()
        {
            var strategies = new IFormationStrategy[]
            {
                new LineFormationStrategy(),
                new GridFormationStrategy(columns: 5),
                new WedgeFormationStrategy(),
                new StaggeredFormationStrategy(columns: 5)
            };

            foreach (var strategy in strategies)
            {
                for (var i = 0; i < UnitCount; i++)
                {
                    var left = strategy.ComputePosition(ArmySide.Left, i, UnitCount, SpawnOffsetX);
                    var right = strategy.ComputePosition(ArmySide.Right, i, UnitCount, SpawnOffsetX);

                    Assert.Less(left.X, 0f, $"{strategy.GetType().Name} index {i} Left should have negative X");
                    Assert.Greater(right.X, 0f, $"{strategy.GetType().Name} index {i} Right should have positive X");
                }
            }
        }

        private static List<BattlePoint> ComputeAll(IFormationStrategy strategy, ArmySide side, int count)
        {
            var positions = new List<BattlePoint>(count);
            for (var i = 0; i < count; i++)
            {
                positions.Add(strategy.ComputePosition(side, i, count, SpawnOffsetX));
            }
            return positions;
        }

        private static void AssertAllUnique(IReadOnlyList<BattlePoint> positions)
        {
            for (var i = 0; i < positions.Count; i++)
            {
                for (var j = i + 1; j < positions.Count; j++)
                {
                    var dx = positions[i].X - positions[j].X;
                    var dz = positions[i].Z - positions[j].Z;
                    var distSqr = dx * dx + dz * dz;
                    Assert.Greater(distSqr, 0.001f,
                        $"Positions [{i}] and [{j}] overlap: ({positions[i].X},{positions[i].Z}) vs ({positions[j].X},{positions[j].Z})");
                }
            }
        }
    }
}
