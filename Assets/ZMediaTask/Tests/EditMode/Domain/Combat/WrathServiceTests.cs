using NUnit.Framework;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class WrathServiceTests
    {
        [Test]
        public void WrathService_ApplyAoe_ClampsHpToZero()
        {
            var service = new WrathService(new HealthService());
            var units = new[]
            {
                Unit(1, hp: 5),
                Unit(2, hp: 10)
            };
            var affected = new System.Collections.Generic.ReadOnlySet<int>(new[] { 1 });

            var result = service.ApplyAoe(units, affected, damage: 50);

            Assert.AreEqual(0, result.UnitsAfter[0].CurrentHp);
            Assert.AreEqual(10, result.UnitsAfter[1].CurrentHp);
            Assert.AreEqual(1, result.KilledUnitIds.Count);
            Assert.AreEqual(1, result.KilledUnitIds[0]);
        }

        private static CombatUnitState Unit(int id, int hp)
        {
            return new CombatUnitState(
                id,
                new BattlePoint(0f, 0f),
                hp,
                attack: 10,
                attackSpeed: 1,
                nextAttackTimeSec: 0f);
        }
    }
}
