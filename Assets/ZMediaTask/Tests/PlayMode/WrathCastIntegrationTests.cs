using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.PlayMode
{
    public class WrathCastIntegrationTests : PlayModeTestFixture
    {
        [UnityTest]
        public IEnumerator PlayMode_WrathDragAndCast_DealsFriendlyFireInRadius()
        {
            // 2v2 with high HP so nobody dies from normal combat during this test
            var armies = new ArmyPair(
                new Domain.Army.Army(ArmySide.Left, new[]
                {
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(5000, 1, 0, 1)),
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(5000, 1, 0, 1))
                }),
                new Domain.Army.Army(ArmySide.Right, new[]
                {
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(5000, 1, 0, 1)),
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(5000, 1, 0, 1))
                }));

            BattleLoop.Initialize(armies);
            BattleLoop.Start();

            // Force wrath meter to full
            BattleLoop.SetWrathMeter(ArmySide.Left, new WrathMeter(100, 100));

            // Enqueue wrath cast with radius=20 (covers all units) and impactTimeSec=0 (instant)
            var accepted = BattleLoop.EnqueueWrathCast(new WrathCastCommand(
                casterSide: ArmySide.Left,
                center: new BattlePoint(0f, 0f),
                radius: 20f,
                damage: 80,
                castTimeSec: 0f,
                impactTimeSec: 0f));

            Assert.IsTrue(accepted, "Wrath cast should be accepted while battle is running.");

            // Tick to apply the wrath impact
            BattleLoop.Tick(deltaTimeSec: 0.02f, currentTimeSec: 0f);

            // All 4 units should have taken 80 damage (friendly fire included)
            var allUnits = BattleLoop.Context.Units;
            Assert.AreEqual(4, allUnits.Count, "Should have 4 units.");

            foreach (var unit in allUnits)
            {
                Assert.Less(unit.Combat.CurrentHp, 5000,
                    $"Unit {unit.UnitId} ({unit.Side}) should have taken wrath damage.");
            }

            // Verify the wrath impact event was emitted
            var impactEvents = BattleLoop.LastTickEvents
                .Where(e => e.Kind == BattleEventKind.WrathImpactApplied)
                .ToList();
            Assert.AreEqual(1, impactEvents.Count, "Should have exactly one wrath impact event.");
            Assert.AreEqual(4, impactEvents[0].AffectedCount, "All 4 units should be affected.");

            yield return null;
        }
    }
}
