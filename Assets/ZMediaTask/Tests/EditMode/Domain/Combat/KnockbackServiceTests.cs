using NUnit.Framework;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class KnockbackServiceTests
    {
        private KnockbackService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new KnockbackService();
        }

        [Test]
        public void ApplyImpulse_PushesAwayFromAttacker()
        {
            var state = default(KnockbackState);
            var attackerPos = new BattlePoint(0f, 0f);
            var targetPos = new BattlePoint(2f, 0f);

            var result = _service.ApplyImpulse(state, attackerPos, targetPos, 1f);

            Assert.Greater(result.Velocity.X, 0f, "Should push in positive X (away from attacker).");
            Assert.AreEqual(0f, result.Velocity.Z, 0.001f);
        }

        [Test]
        public void ApplyImpulse_StacksWithExisting()
        {
            var initial = new KnockbackState(new BattlePoint(1f, 0f));
            var attackerPos = new BattlePoint(0f, 0f);
            var targetPos = new BattlePoint(2f, 0f);

            var result = _service.ApplyImpulse(initial, attackerPos, targetPos, 0.5f);

            Assert.Greater(result.Velocity.X, 1f, "Should stack on top of existing velocity.");
        }

        [Test]
        public void ApplyImpulse_SamePosition_ReturnsZero()
        {
            var state = default(KnockbackState);
            var pos = new BattlePoint(3f, 3f);

            var result = _service.ApplyImpulse(state, pos, pos, 1f);

            Assert.AreEqual(0f, result.Velocity.X, 0.001f);
            Assert.AreEqual(0f, result.Velocity.Z, 0.001f);
        }

        [Test]
        public void Decay_ReducesMagnitude()
        {
            var state = new KnockbackState(new BattlePoint(2f, 0f));

            var result = _service.Decay(state, 0.1f, 5f, 0.001f);

            var mag = DistanceMetrics.Magnitude(result.Velocity);
            Assert.Less(mag, 2f, "Magnitude should decrease after decay.");
            Assert.Greater(mag, 0f, "Should not snap to zero yet.");
        }

        [Test]
        public void Decay_SnapsToZero_BelowThreshold()
        {
            var state = new KnockbackState(new BattlePoint(0.0005f, 0f));

            var result = _service.Decay(state, 0.01f, 1f, 0.001f);

            Assert.IsFalse(result.HasVelocity, "Should snap to zero when below threshold.");
        }

        [Test]
        public void Decay_SnapsToZero_WhenReductionExceedsMagnitude()
        {
            var state = new KnockbackState(new BattlePoint(0.1f, 0f));

            var result = _service.Decay(state, 1f, 5f, 0.001f);

            Assert.IsFalse(result.HasVelocity,
                "Should snap to zero when decay reduction exceeds magnitude.");
        }

        [Test]
        public void Decay_NoopOnDefault()
        {
            var state = default(KnockbackState);

            var result = _service.Decay(state, 0.1f, 5f, 0.001f);

            Assert.IsFalse(result.HasVelocity);
        }

        [Test]
        public void ComputeDisplacement_ScalesByDeltaTime()
        {
            var state = new KnockbackState(new BattlePoint(4f, 2f));

            var displacement = _service.ComputeDisplacement(state, 0.5f);

            Assert.AreEqual(2f, displacement.X, 0.001f);
            Assert.AreEqual(1f, displacement.Z, 0.001f);
        }

        [Test]
        public void KnockbackState_Default_HasNoVelocity()
        {
            var state = default(KnockbackState);

            Assert.IsFalse(state.HasVelocity);
            Assert.AreEqual(0f, state.Velocity.X, 0.001f);
            Assert.AreEqual(0f, state.Velocity.Z, 0.001f);
        }
    }
}
