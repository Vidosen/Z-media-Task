using NUnit.Framework;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Tests.EditMode.Application.Battle
{
    public class BattleLoopServiceTests
    {
        [Test]
        public void BattleLoop_FinishesWhenArmyDestroyed()
        {
            var processor = new DestroyUnitsStepProcessor(ArmySide.Right);
            var loop = CreateLoop(processor);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();

            loop.Tick(0.5f, 0.5f);

            Assert.AreEqual(BattlePhase.Finished, loop.StateMachine.Current);
        }

        [Test]
        public void BattleLoop_ProducesWinnerSide()
        {
            var processor = new DestroyUnitsStepProcessor(ArmySide.Right);
            var loop = CreateLoop(processor);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();

            loop.Tick(0.5f, 0.5f);

            Assert.AreEqual(ArmySide.Left, loop.Context.WinnerSide);
        }

        [Test]
        public void BattleLoop_DoesNotAdvanceWhenStateNotRunning()
        {
            var processor = new TrackingStepProcessor();
            var loop = CreateLoop(processor);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));

            loop.Tick(1f, 1f);

            Assert.AreEqual(0, processor.CallCount);
            Assert.AreEqual(0f, loop.Context.ElapsedTimeSec);
            Assert.AreEqual(BattlePhase.Preparation, loop.StateMachine.Current);
        }

        [Test]
        public void BattleLoop_WhenBothArmiesDestroyed_ProducesDraw()
        {
            var processor = new DestroyUnitsStepProcessor(sideToDestroy: null);
            var loop = CreateLoop(processor);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));
            loop.Start();

            loop.Tick(0.25f, 1f);

            Assert.AreEqual(BattlePhase.Finished, loop.StateMachine.Current);
            Assert.IsNull(loop.Context.WinnerSide);
        }

        [Test]
        public void BattleStateMachine_AllowsOnlyValidTransitions()
        {
            var stateMachine = new BattleStateMachine();

            Assert.AreEqual(BattlePhase.Preparation, stateMachine.Current);
            Assert.IsFalse(stateMachine.Finish());
            Assert.IsFalse(stateMachine.Reset());

            Assert.IsTrue(stateMachine.Start());
            Assert.AreEqual(BattlePhase.Running, stateMachine.Current);
            Assert.IsFalse(stateMachine.Start());
            Assert.IsFalse(stateMachine.Reset());

            Assert.IsTrue(stateMachine.Finish());
            Assert.AreEqual(BattlePhase.Finished, stateMachine.Current);
            Assert.IsFalse(stateMachine.Finish());
            Assert.IsFalse(stateMachine.Start());

            Assert.IsTrue(stateMachine.Reset());
            Assert.AreEqual(BattlePhase.Preparation, stateMachine.Current);
        }

        [Test]
        public void BattleLoop_Tick_UpdatesContextOnlyInRunning()
        {
            var processor = new DestroyUnitsStepProcessor(ArmySide.Right);
            var loop = CreateLoop(processor);
            loop.Initialize(CreateArmies(leftHp: 100, rightHp: 100));

            loop.Tick(0.4f, 0.4f);
            Assert.AreEqual(0, processor.CallCount);
            Assert.AreEqual(0f, loop.Context.ElapsedTimeSec);

            loop.Start();
            loop.Tick(0.4f, 0.4f);
            Assert.AreEqual(1, processor.CallCount);
            Assert.AreEqual(0.4f, loop.Context.ElapsedTimeSec, 0.0001f);
            Assert.AreEqual(BattlePhase.Finished, loop.StateMachine.Current);

            loop.Tick(0.4f, 0.8f);
            Assert.AreEqual(1, processor.CallCount);
            Assert.AreEqual(0.4f, loop.Context.ElapsedTimeSec, 0.0001f);
        }

        [Test]
        public void BattleContextFactory_CarriesTraitsFromArmyUnit()
        {
            var factory = new BattleContextFactory(new LineFormationStrategy());
            var left = new ZMediaTask.Domain.Army.Army(ArmySide.Left, new[]
            {
                new ArmyUnit(UnitShape.Sphere, UnitSize.Big, UnitColor.Red,
                    new StatBlock(100, 10, 10, 1))
            });
            var right = new ZMediaTask.Domain.Army.Army(ArmySide.Right, new[]
            {
                new ArmyUnit(UnitShape.Cube, UnitSize.Small, UnitColor.Blue,
                    new StatBlock(80, 15, 8, 2))
            });
            var context = factory.Create(new ArmyPair(left, right));

            Assert.AreEqual(UnitShape.Sphere, context.Units[0].Shape);
            Assert.AreEqual(UnitSize.Big, context.Units[0].Size);
            Assert.AreEqual(UnitColor.Red, context.Units[0].Color);

            Assert.AreEqual(UnitShape.Cube, context.Units[1].Shape);
            Assert.AreEqual(UnitSize.Small, context.Units[1].Size);
            Assert.AreEqual(UnitColor.Blue, context.Units[1].Color);
        }

        [Test]
        public void BattleStepProcessor_Step_DoesNotMutateInput()
        {
            var processor = new BattleStepProcessor();
            var contextFactory = new BattleContextFactory(new LineFormationStrategy());
            var context = contextFactory.Create(CreateArmies(leftHp: 100, rightHp: 100));
            var input = new BattleStepInput(context, 0.5f, 10f);

            var next = processor.Step(input);

            Assert.AreEqual(0f, context.ElapsedTimeSec);
            Assert.AreEqual(0.5f, next.ElapsedTimeSec, 0.0001f);
            Assert.AreEqual(context.Units.Count, next.Units.Count);
            Assert.AreNotSame(context.Units, next.Units);
            Assert.AreEqual(context.Units[0].Combat.CurrentHp, next.Units[0].Combat.CurrentHp);
            Assert.AreEqual(context.Units[0].Movement.IsAlive, next.Units[0].Movement.IsAlive);
        }

        private static BattleLoopService CreateLoop(IBattleStepProcessor processor)
        {
            return new BattleLoopService(new BattleContextFactory(new LineFormationStrategy()), processor);
        }

        private static ArmyPair CreateArmies(int leftHp, int rightHp)
        {
            return new ArmyPair(
                new ZMediaTask.Domain.Army.Army(ArmySide.Left, new[] { CreateUnit(leftHp) }),
                new ZMediaTask.Domain.Army.Army(ArmySide.Right, new[] { CreateUnit(rightHp) }));
        }

        private static ArmyUnit CreateUnit(int hp)
        {
            return new ArmyUnit(
                UnitShape.Cube,
                UnitSize.Small,
                UnitColor.Blue,
                new StatBlock(hp, atk: 10, speed: 10, atkspd: 1));
        }

        private static BattleUnitRuntime Kill(BattleUnitRuntime unit)
        {
            var movement = new MovementAgentState(
                unit.UnitId,
                isAlive: false,
                unit.Movement.Speed,
                unit.Movement.Position,
                unit.Movement.TargetId,
                unit.Movement.CurrentPath,
                unit.Movement.LastPathTargetPosition);
            var combat = unit.Combat.WithCurrentHp(0);
            return unit.WithMovement(movement).WithCombat(combat);
        }

        private sealed class TrackingStepProcessor : IBattleStepProcessor
        {
            public int CallCount { get; private set; }

            public BattleContext Step(BattleStepInput input)
            {
                CallCount++;
                return new BattleContext(
                    input.Context.Units,
                    input.Context.ElapsedTimeSec + input.DeltaTimeSec,
                    input.Context.WinnerSide);
            }
        }

        private sealed class DestroyUnitsStepProcessor : IBattleStepProcessor
        {
            private readonly ArmySide? _sideToDestroy;

            public DestroyUnitsStepProcessor(ArmySide? sideToDestroy)
            {
                _sideToDestroy = sideToDestroy;
            }

            public int CallCount { get; private set; }

            public BattleContext Step(BattleStepInput input)
            {
                CallCount++;

                var units = new BattleUnitRuntime[input.Context.Units.Count];
                for (var i = 0; i < input.Context.Units.Count; i++)
                {
                    var source = input.Context.Units[i];
                    units[i] = _sideToDestroy == null || source.Side == _sideToDestroy.Value
                        ? Kill(source)
                        : source;
                }

                return new BattleContext(
                    units,
                    input.Context.ElapsedTimeSec + input.DeltaTimeSec,
                    input.Context.WinnerSide);
            }
        }
    }
}
