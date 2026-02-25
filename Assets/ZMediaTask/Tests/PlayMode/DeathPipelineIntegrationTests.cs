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
    public class DeathPipelineIntegrationTests : PlayModeTestFixture
    {
        [UnityTest]
        [Ignore("Iteration 10 — ragdoll, fade VFX, and despawn pipeline not yet implemented.")]
        public IEnumerator PlayMode_DeathRagdoll_Fades_AndDespawns()
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

            // TODO (Iteration 10): Verify ragdoll activation, fade-out VFX, and despawn timing

            yield return null;
        }
    }
}
