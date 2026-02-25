using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Tests.PlayMode
{
    public class DeathPipelineIntegrationTests : PlayModeTestFixture
    {
        [UnityTest]
        public IEnumerator PlayMode_DeathTracker_DetectsDeathsAfterBattle()
        {
            // Arrange: 1v1 where left one-shots right
            var armies = new ArmyPair(
                new Domain.Army.Army(ArmySide.Left, new[]
                {
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(100, 999, 10, 1))
                }),
                new Domain.Army.Army(ArmySide.Right, new[]
                {
                    new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                        new StatBlock(10, 1, 10, 1))
                }));

            BattleLoop.Initialize(armies);
            BattleLoop.Start();

            TickUntilFinished(maxTicks: 50000);

            // Domain-level death verification
            Assert.AreEqual(BattlePhase.Finished, BattleLoop.StateMachine.Current);

            var deadUnits = BattleLoop.Context.Units.Where(u => !u.Combat.IsAlive).ToList();
            Assert.IsTrue(deadUnits.Count > 0, "At least one unit should be dead.");

            // DeathTracker verification
            var deathTracker = new DeathTracker();
            var newlyDead = deathTracker.DetectNewDeaths(BattleLoop.Context.Units);
            Assert.AreEqual(deadUnits.Count, newlyDead.Count,
                "DeathTracker should detect all dead units on first call.");

            // Second call should return no new deaths
            var secondCall = deathTracker.DetectNewDeaths(BattleLoop.Context.Units);
            Assert.AreEqual(0, secondCall.Count,
                "DeathTracker should not re-report already-tracked deaths.");

            yield return null;
        }
    }
}
