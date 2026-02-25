using System;
using System.Collections.Generic;
using NUnit.Framework;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Tests.EditMode.Presentation
{
    public class DeathTrackerTests
    {
        [Test]
        public void DetectNewDeaths_ReturnsNewlyDeadUnitIds()
        {
            var tracker = new DeathTracker();
            var units = new List<BattleUnitRuntime>
            {
                MakeUnit(1, alive: true),
                MakeUnit(2, alive: false),
                MakeUnit(3, alive: false)
            };

            var newlyDead = tracker.DetectNewDeaths(units);

            Assert.AreEqual(2, newlyDead.Count);
            Assert.IsTrue(ContainsId(newlyDead, 2));
            Assert.IsTrue(ContainsId(newlyDead, 3));
        }

        [Test]
        public void DetectNewDeaths_DoesNotReturnAlreadyTrackedDeaths()
        {
            var tracker = new DeathTracker();
            var units = new List<BattleUnitRuntime>
            {
                MakeUnit(1, alive: true),
                MakeUnit(2, alive: false)
            };

            tracker.DetectNewDeaths(units);
            var secondCall = tracker.DetectNewDeaths(units);

            Assert.AreEqual(0, secondCall.Count, "Already-tracked deaths should not be returned again.");
        }

        [Test]
        public void Reset_ClearsTracking()
        {
            var tracker = new DeathTracker();
            var units = new List<BattleUnitRuntime>
            {
                MakeUnit(1, alive: false)
            };

            tracker.DetectNewDeaths(units);
            tracker.Reset();
            var afterReset = tracker.DetectNewDeaths(units);

            Assert.AreEqual(1, afterReset.Count, "After reset, previously tracked deaths should be returned again.");
            Assert.IsTrue(ContainsId(afterReset, 1));
        }

        private static BattleUnitRuntime MakeUnit(int unitId, bool alive)
        {
            var hp = alive ? 100 : 0;
            var pos = new BattlePoint(0f, 0f);
            var movement = new MovementAgentState(
                unitId, alive, 5f, pos, null, Array.Empty<BattlePoint>(), null);
            var combat = new CombatUnitState(unitId, pos, hp, 10, 1, 0f);
            return new BattleUnitRuntime(
                unitId, ArmySide.Left, UnitShape.Cube, UnitSize.Small, UnitColor.Blue, movement, combat);
        }

        private static bool ContainsId(System.Collections.Generic.IReadOnlyList<int> list, int id)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == id) return true;
            }
            return false;
        }
    }
}
