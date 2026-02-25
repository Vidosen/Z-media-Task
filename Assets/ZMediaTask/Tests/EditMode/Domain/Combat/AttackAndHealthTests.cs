using NUnit.Framework;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class AttackAndHealthTests
    {
        [Test]
        public void AttackService_AppliesAtkDamage()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 2, nextAttackTimeSec: 0f);
            var target = Unit(2, 0.5f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 0f, config);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10, result.DamageApplied);
            Assert.AreEqual(40, result.TargetAfter.CurrentHp);
            Assert.AreEqual(2f, result.AttackerAfter.NextAttackTimeSec);
            Assert.IsNull(result.Failure);
        }

        [Test]
        public void AttackService_RespectsAtkSpdCooldown()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 2, nextAttackTimeSec: 3f);
            var target = Unit(2, 0.5f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 2f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AttackFailureReason.CooldownNotReady, result.Failure);
            Assert.AreEqual(0, result.DamageApplied);
        }

        [Test]
        public void AttackService_DoesNotAttackWhenOutOfRange()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);
            var target = Unit(2, 5f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 0f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AttackFailureReason.OutOfRange, result.Failure);
            Assert.AreEqual(50, result.TargetAfter.CurrentHp);
            Assert.AreEqual(0f, result.AttackerAfter.NextAttackTimeSec);
        }

        [Test]
        public void HealthService_UnitDiesAtZeroHp()
        {
            var healthService = new HealthService();
            var unit = Unit(1, 0f, 0f, hp: 5, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);

            var result = healthService.ApplyDamage(unit, 5);

            Assert.AreEqual(0, result.UnitAfter.CurrentHp);
            Assert.IsTrue(result.DiedNow);
            Assert.IsTrue(healthService.IsDead(result.UnitAfter));
        }

        [Test]
        public void AttackService_DoesNotChangeCooldown_WhenCooldownNotReady()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 2, nextAttackTimeSec: 5f);
            var target = Unit(2, 0.5f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 1f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(5f, result.AttackerAfter.NextAttackTimeSec);
        }

        [Test]
        public void AttackService_DoesNotChangeCooldown_WhenOutOfRange()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 3, nextAttackTimeSec: 0f);
            var target = Unit(2, 10f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 2f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 3f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0f, result.AttackerAfter.NextAttackTimeSec);
        }

        [Test]
        public void AttackService_DoesNotAttack_WhenAttackerDead()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 0, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);
            var target = Unit(2, 0.5f, 0f, hp: 50, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 0f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AttackFailureReason.AttackerDead, result.Failure);
            Assert.AreEqual(50, result.TargetAfter.CurrentHp);
        }

        [Test]
        public void AttackService_DoesNotAttack_WhenTargetDead()
        {
            var service = CreateAttackService();
            var attacker = Unit(1, 0f, 0f, hp: 100, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);
            var target = Unit(2, 0.5f, 0f, hp: 0, attack: 5, attackSpeed: 1, nextAttackTimeSec: 0f);
            var config = new AttackConfig(attackRange: 1f, baseAttackDelay: 1f);

            var result = service.TryAttack(attacker, target, currentTimeSec: 0f, config);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AttackFailureReason.TargetDead, result.Failure);
            Assert.AreEqual(0, result.DamageApplied);
        }

        [Test]
        public void CooldownTracker_ComputeNextAttackTime_UsesBaseDelayTimesAttackSpeed()
        {
            var tracker = new CooldownTracker();

            var nextTime = tracker.ComputeNextAttackTime(1.5f, attackSpeed: 4, baseAttackDelay: 0.75f);

            Assert.AreEqual(4.5f, nextTime, 0.0001f);
        }

        [Test]
        public void HealthService_ApplyDamage_ClampsHpToZero()
        {
            var healthService = new HealthService();
            var unit = Unit(1, 0f, 0f, hp: 3, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);

            var result = healthService.ApplyDamage(unit, 10);

            Assert.AreEqual(0, result.UnitAfter.CurrentHp);
            Assert.AreEqual(3, result.DamageApplied);
            Assert.IsTrue(result.DiedNow);
        }

        [Test]
        public void HealthService_IsDead_ReturnsTrue_WhenHpZero()
        {
            var healthService = new HealthService();
            var unit = Unit(1, 0f, 0f, hp: 0, attack: 10, attackSpeed: 1, nextAttackTimeSec: 0f);

            Assert.IsTrue(healthService.IsDead(unit));
        }

        private static AttackService CreateAttackService()
        {
            return new AttackService(new CooldownTracker(), new HealthService());
        }

        private static CombatUnitState Unit(
            int id,
            float x,
            float z,
            int hp,
            int attack,
            int attackSpeed,
            float nextAttackTimeSec)
        {
            return new CombatUnitState(
                id,
                new BattlePoint(x, z),
                hp,
                attack,
                attackSpeed,
                nextAttackTimeSec);
        }
    }
}
