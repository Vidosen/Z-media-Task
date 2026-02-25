using System;
using System.Collections.Generic;
using NUnit.Framework;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class TargetSelectorTests
    {
        private static readonly TargetableUnit Self = new(1, new BattlePoint(0f, 0f), true);

        [Test]
        public void TargetSelector_ChoosesNearestAliveEnemy()
        {
            var selector = new NearestTargetSelector();
            var enemies = new List<TargetableUnit>
            {
                new(10, new BattlePoint(10f, 0f), true),
                new(20, new BattlePoint(2f, 0f), true),
                new(30, new BattlePoint(1f, 0f), false)
            };

            var result = selector.SelectTarget(Self, enemies, null);

            Assert.AreEqual(20, result);
        }

        [Test]
        public void TargetSelector_ReacquiresWhenTargetDies()
        {
            var selector = new NearestTargetSelector();
            var enemies = new List<TargetableUnit>
            {
                new(10, new BattlePoint(1f, 0f), false),
                new(20, new BattlePoint(3f, 0f), true),
                new(30, new BattlePoint(2f, 0f), true)
            };

            var result = selector.SelectTarget(Self, enemies, 10);

            Assert.AreEqual(30, result);
        }

        [Test]
        public void TargetSelector_ReturnsNoneWhenNoEnemies()
        {
            var selector = new NearestTargetSelector();

            var result = selector.SelectTarget(Self, Array.Empty<TargetableUnit>(), null);

            Assert.IsNull(result);
        }

        [Test]
        public void TargetSelector_UsesCurrentTarget_WhenCurrentTargetIsAlive()
        {
            var selector = new NearestTargetSelector();
            var enemies = new List<TargetableUnit>
            {
                new(10, new BattlePoint(1f, 0f), true),
                new(20, new BattlePoint(5f, 0f), true)
            };

            var result = selector.SelectTarget(Self, enemies, 20);

            Assert.AreEqual(20, result);
        }

        [Test]
        public void TargetSelector_WhenEqualDistance_PicksFirstInList()
        {
            var selector = new NearestTargetSelector();
            var enemies = new List<TargetableUnit>
            {
                new(10, new BattlePoint(2f, 0f), true),
                new(20, new BattlePoint(-2f, 0f), true)
            };

            var result = selector.SelectTarget(Self, enemies, null);

            Assert.AreEqual(10, result);
        }

        [Test]
        public void TargetSelector_WhenCurrentTargetMissing_ReacquiresNearestAlive()
        {
            var selector = new NearestTargetSelector();
            var enemies = new List<TargetableUnit>
            {
                new(10, new BattlePoint(3f, 0f), true),
                new(20, new BattlePoint(2f, 0f), true)
            };

            var result = selector.SelectTarget(Self, enemies, 999);

            Assert.AreEqual(20, result);
        }

        [Test]
        public void TargetSelector_WhenEnemiesIsNull_Throws()
        {
            var selector = new NearestTargetSelector();

            Assert.Throws<ArgumentNullException>(() => selector.SelectTarget(Self, null, null));
        }
    }
}
