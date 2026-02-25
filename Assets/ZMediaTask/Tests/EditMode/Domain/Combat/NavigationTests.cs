using System;
using System.Collections.Generic;
using NUnit.Framework;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Tests.EditMode.Domain.Combat
{
    public class NavigationTests
    {
        [Test]
        public void DirectPathfinder_ReturnsSingleWaypoint_TargetPosition()
        {
            var pathfinder = new DirectPathfinder();
            var target = new BattlePoint(5f, -2f);

            var path = pathfinder.BuildPath(new BattlePoint(0f, 0f), target);

            Assert.AreEqual(1, path.Count);
            AssertPoint(path[0], target.X, target.Z);
        }

        [Test]
        public void MovementService_FollowsWaypoint_WithSpeedPerSecond()
        {
            var service = CreateMovementService(
                steeringService: new ZeroSteeringService(),
                slotAllocator: new TargetSlotAllocator());
            var self = CreateSelf(position: new BattlePoint(0f, 0f), speed: 4f);
            var enemies = new[] { Enemy(10, 10f, 0f, true) };
            var context = CreateContext(
                deltaTime: 0.5f,
                allies: new[] { self },
                enemies: enemies,
                config: new MovementConfig(0.1f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            AssertPoint(next.Position, 2f, 0f);
        }

        [Test]
        public void MovementService_StopsAtMeleeRange()
        {
            var service = CreateMovementService(
                steeringService: new ZeroSteeringService(),
                slotAllocator: new TargetSlotAllocator());
            var self = CreateSelf(position: new BattlePoint(0.4f, 0f), speed: 10f, targetId: 10);
            var context = CreateContext(
                deltaTime: 1f,
                allies: new[] { self },
                enemies: new[] { Enemy(10, 0f, 0f, true) },
                config: new MovementConfig(0.5f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            AssertPoint(next.Position, self.Position.X, self.Position.Z);
            Assert.AreEqual(10, next.TargetId);
        }

        [Test]
        public void SteeringService_AvoidsNeighbors_WithSpatialHash()
        {
            var steering = new SpatialHashSteeringService();
            var self = new BattlePoint(0f, 0f);
            var neighbors = new List<BattlePoint>
            {
                new(0.3f, 0f),
                new(0.35f, 0.1f),
                new(5f, 5f)
            };

            var offset = steering.ComputeSeparationOffset(self, neighbors, 1f);

            var magnitude = (float)Math.Sqrt((offset.X * offset.X) + (offset.Z * offset.Z));
            Assert.Less(offset.X, 0f);
            Assert.Greater(magnitude, 0f);
        }

        [Test]
        public void SlotAllocator_AssignsUniqueSlots_AroundTarget()
        {
            var allocator = new RingSlotAllocator();
            var target = new BattlePoint(0f, 0f);
            var attackerIds = new[] { 40, 10, 30, 20 };
            var seen = new HashSet<string>();

            foreach (var id in attackerIds)
            {
                var slot = allocator.GetSlotPosition(target, id, attackerIds, 2f);
                var key = $"{Math.Round(slot.X, 4)}:{Math.Round(slot.Z, 4)}";
                Assert.IsTrue(seen.Add(key));
            }
        }

        [Test]
        public void MovementService_Repaths_WhenTargetMovedBeyondThreshold()
        {
            var recordingPathfinder = new RecordingPathfinder();
            var service = CreateMovementService(pathfinder: recordingPathfinder);
            var self = new MovementAgentState(
                unitId: 1,
                isAlive: true,
                speed: 5f,
                position: new BattlePoint(0f, 0f),
                targetId: 10,
                currentPath: new[] { new BattlePoint(1f, 0f) },
                lastPathTargetPosition: new BattlePoint(0f, 0f));
            var context = CreateContext(
                deltaTime: 1f,
                allies: new[] { self },
                enemies: new[] { Enemy(10, 2f, 0f, true) },
                config: new MovementConfig(0.1f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            Assert.AreEqual(1, recordingPathfinder.CallCount);
            AssertPoint(next.LastPathTargetPosition!.Value, 2f, 0f);
        }

        [Test]
        public void MovementService_DoesNothing_WhenNoTarget()
        {
            var service = CreateMovementService();
            var self = new MovementAgentState(
                unitId: 1,
                isAlive: true,
                speed: 5f,
                position: new BattlePoint(2f, 3f),
                targetId: 10,
                currentPath: new[] { new BattlePoint(4f, 5f) },
                lastPathTargetPosition: new BattlePoint(4f, 5f));
            var context = CreateContext(
                deltaTime: 1f,
                allies: new[] { self },
                enemies: Array.Empty<TargetableUnit>(),
                config: new MovementConfig(0.1f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            AssertPoint(next.Position, 2f, 3f);
            Assert.IsNull(next.TargetId);
            Assert.AreEqual(0, next.CurrentPath.Count);
            Assert.IsNull(next.LastPathTargetPosition);
        }

        [Test]
        public void MovementService_KeepsCurrentTarget_WhenStillAlive()
        {
            var service = CreateMovementService();
            var self = CreateSelf(position: new BattlePoint(0f, 0f), speed: 1f, targetId: 10);
            var context = CreateContext(
                deltaTime: 1f,
                allies: new[] { self },
                enemies: new[]
                {
                    Enemy(10, 8f, 0f, true),
                    Enemy(20, 1f, 0f, true)
                },
                config: new MovementConfig(0.1f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            Assert.AreEqual(10, next.TargetId);
        }

        [Test]
        public void MovementService_WhenDeltaTimeZero_DoesNotMove()
        {
            var service = CreateMovementService();
            var self = CreateSelf(position: new BattlePoint(0f, 0f), speed: 3f);
            var context = CreateContext(
                deltaTime: 0f,
                allies: new[] { self },
                enemies: new[] { Enemy(10, 5f, 0f, true) },
                config: new MovementConfig(0.1f, 0.5f, 1f, 0f));

            var next = service.Tick(self, context);

            AssertPoint(next.Position, 0f, 0f);
        }

        [Test]
        public void MovementService_UsesSlotDestination_ForPathBuilding()
        {
            var recordingPathfinder = new RecordingPathfinder();
            var slotDestination = new BattlePoint(3f, 4f);
            var service = CreateMovementService(
                pathfinder: recordingPathfinder,
                slotAllocator: new FixedSlotAllocator(slotDestination));
            var self = CreateSelf(position: new BattlePoint(0f, 0f), speed: 1f);
            var context = CreateContext(
                deltaTime: 1f,
                allies: new[] { self },
                enemies: new[] { Enemy(10, 8f, 0f, true) },
                config: new MovementConfig(0.1f, 0.5f, 1f, 1f));

            service.Tick(self, context);

            Assert.IsTrue(recordingPathfinder.LastTo.HasValue);
            AssertPoint(recordingPathfinder.LastTo!.Value, slotDestination.X, slotDestination.Z);
        }

        private static MovementService CreateMovementService(
            IPathfinder pathfinder = null,
            ISteeringService steeringService = null,
            ISlotAllocator slotAllocator = null)
        {
            return new MovementService(
                new NearestTargetSelector(),
                pathfinder ?? new DirectPathfinder(),
                steeringService ?? new ZeroSteeringService(),
                slotAllocator ?? new TargetSlotAllocator());
        }

        private static MovementAgentState CreateSelf(BattlePoint position, float speed, int? targetId = null)
        {
            return new MovementAgentState(
                unitId: 1,
                isAlive: true,
                speed: speed,
                position: position,
                targetId: targetId,
                currentPath: Array.Empty<BattlePoint>(),
                lastPathTargetPosition: null);
        }

        private static MovementTickContext CreateContext(
            float deltaTime,
            IReadOnlyList<MovementAgentState> allies,
            IReadOnlyList<TargetableUnit> enemies,
            MovementConfig config)
        {
            return new MovementTickContext(deltaTime, allies, enemies, config);
        }

        private static TargetableUnit Enemy(int id, float x, float z, bool isAlive)
        {
            return new TargetableUnit(id, new BattlePoint(x, z), isAlive);
        }

        private static void AssertPoint(BattlePoint point, float expectedX, float expectedZ, float epsilon = 0.0001f)
        {
            Assert.That(point.X, Is.EqualTo(expectedX).Within(epsilon));
            Assert.That(point.Z, Is.EqualTo(expectedZ).Within(epsilon));
        }

        private sealed class RecordingPathfinder : IPathfinder
        {
            public int CallCount { get; private set; }

            public BattlePoint? LastTo { get; private set; }

            public IReadOnlyList<BattlePoint> BuildPath(BattlePoint from, BattlePoint to)
            {
                CallCount++;
                LastTo = to;
                return new[] { to };
            }
        }

        private sealed class ZeroSteeringService : ISteeringService
        {
            public BattlePoint ComputeSeparationOffset(
                BattlePoint selfPosition,
                IReadOnlyList<BattlePoint> neighborPositions,
                float steeringRadius)
            {
                return new BattlePoint(0f, 0f);
            }
        }

        private sealed class TargetSlotAllocator : ISlotAllocator
        {
            public BattlePoint GetSlotPosition(
                BattlePoint targetPosition,
                int unitId,
                IReadOnlyList<int> attackerIds,
                float slotRadius)
            {
                return targetPosition;
            }
        }

        private sealed class FixedSlotAllocator : ISlotAllocator
        {
            private readonly BattlePoint _slot;

            public FixedSlotAllocator(BattlePoint slot)
            {
                _slot = slot;
            }

            public BattlePoint GetSlotPosition(
                BattlePoint targetPosition,
                int unitId,
                IReadOnlyList<int> attackerIds,
                float slotRadius)
            {
                return _slot;
            }
        }
    }
}
