using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Infrastructure.Config.Combat;
using ZMediaTask.Infrastructure.Random;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Tests.PlayMode
{
    public class PlayModeTestFixture
    {
        protected static readonly AttackConfig TestAttackConfig = new(attackRange: 1.5f, baseAttackDelay: 1f);
        protected static readonly MovementConfig TestMovementConfig = new(
            meleeRange: 1.5f,
            repathDistanceThreshold: 2f,
            steeringRadius: 1.2f,
            slotRadius: 1f);
        protected static readonly WrathConfig TestWrathConfig = new(
            chargePerKill: 20,
            maxCharge: 100,
            radius: 4f,
            damage: 80,
            impactDelaySeconds: 0.35f);
        protected static readonly ArenaBounds TestArenaBounds = new(-15f, 15f, -20f, 20f);

        protected IRandomProvider RandomProvider;
        protected StatsCalculator StatsCalculator;
        protected ArmyFactory ArmyFactory;
        protected ArmyRandomizationUseCase RandomizationUseCase;

        protected HealthService HealthService;
        protected CooldownTracker CooldownTracker;
        protected AttackService AttackService;
        protected WrathService WrathService;

        protected NearestTargetSelector TargetSelector;
        protected DirectPathfinder Pathfinder;
        protected SpatialHashSteeringService SteeringService;
        protected RingSlotAllocator SlotAllocator;
        protected MovementService MovementService;

        protected OnUnitKilledUseCase OnUnitKilledUseCase;
        protected TryCastWrathUseCase TryCastWrathUseCase;
        protected AutoBattleStepProcessor StepProcessor;
        protected BattleContextFactory ContextFactory;
        protected BattleLoopService BattleLoop;

        protected PreparationViewModel PreparationVm;
        protected BattleHudViewModel BattleHudVm;
        protected WrathViewModel WrathVm;
        protected ResultViewModel ResultVm;

        [SetUp]
        public virtual void SetUp()
        {
            var traitCatalog = new TestTraitCatalog();
            var weightCatalog = new TestWeightCatalog();

            RandomProvider = new SystemRandomProvider();
            StatsCalculator = new StatsCalculator(traitCatalog);
            ArmyFactory = new ArmyFactory(StatsCalculator, weightCatalog);
            RandomizationUseCase = new ArmyRandomizationUseCase(ArmyFactory, RandomProvider);

            HealthService = new HealthService();
            CooldownTracker = new CooldownTracker();
            AttackService = new AttackService(CooldownTracker, HealthService);
            WrathService = new WrathService(HealthService);

            TargetSelector = new NearestTargetSelector();
            Pathfinder = new DirectPathfinder();
            SteeringService = new SpatialHashSteeringService();
            SlotAllocator = new RingSlotAllocator();
            MovementService = new MovementService(TargetSelector, Pathfinder, SteeringService, SlotAllocator);

            OnUnitKilledUseCase = new OnUnitKilledUseCase(ArmySide.Left, TestWrathConfig, WrathService);
            var wrathValidator = new ArenaBoundsWrathTargetValidator(TestArenaBounds);
            TryCastWrathUseCase = new TryCastWrathUseCase(
                ArmySide.Left, TestWrathConfig, WrathService, wrathValidator);

            StepProcessor = new AutoBattleStepProcessor(
                MovementService, AttackService, OnUnitKilledUseCase, TestAttackConfig, TestMovementConfig);

            ContextFactory = new BattleContextFactory(new LineFormationStrategy());
            BattleLoop = new BattleLoopService(
                ContextFactory,
                StepProcessor,
                new DistanceUnitQueryInRadius(),
                WrathService,
                TestWrathConfig);

            PreparationVm = new PreparationViewModel(RandomizationUseCase);
            BattleHudVm = new BattleHudViewModel();
            WrathVm = new WrathViewModel();
            ResultVm = new ResultViewModel();
        }

        [TearDown]
        public virtual void TearDown()
        {
            PreparationVm?.Dispose();
            BattleHudVm?.Dispose();
            WrathVm?.Dispose();
            ResultVm?.Dispose();
        }

        protected void TickUntilFinished(int maxTicks = 50000, float tickInterval = 0.02f)
        {
            var currentTime = 0f;
            for (var i = 0; i < maxTicks; i++)
            {
                currentTime += tickInterval;
                BattleLoop.Tick(tickInterval, currentTime);

                if (BattleLoop.StateMachine.Current == BattlePhase.Finished)
                    return;
            }

            Assert.Fail($"Battle did not finish within {maxTicks} ticks.");
        }

        protected static BattleUnitRuntime MakeUnit(
            int unitId, ArmySide side, float x, float z, int hp, int atk, float speed, int atkspd = 1)
        {
            var pos = new BattlePoint(x, z);
            var movement = new MovementAgentState(
                unitId, hp > 0, speed, pos, null, System.Array.Empty<BattlePoint>(), null);
            var combat = new CombatUnitState(unitId, pos, hp, atk, atkspd, nextAttackTimeSec: 0f);
            return new BattleUnitRuntime(
                unitId, side, UnitShape.Cube, UnitSize.Small, UnitColor.Blue, movement, combat);
        }

        protected static Dictionary<ArmySide, WrathMeter> MakeDefaultWrathMeters()
        {
            return new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, 100),
                [ArmySide.Right] = new WrathMeter(0, 100)
            };
        }

        private sealed class TestTraitCatalog : IUnitTraitCatalog
        {
            public StatModifier GetShapeModifier(UnitShape shape)
            {
                return shape switch
                {
                    UnitShape.Cube => new StatModifier(100, 10, 0, 0),
                    UnitShape.Sphere => new StatModifier(50, 20, 0, 0),
                    _ => new StatModifier(0, 0, 0, 0)
                };
            }

            public StatModifier GetSizeModifier(UnitSize size)
            {
                return size switch
                {
                    UnitSize.Big => new StatModifier(50, 0, 0, 0),
                    UnitSize.Small => new StatModifier(-50, 0, 0, 0),
                    _ => new StatModifier(0, 0, 0, 0)
                };
            }

            public StatModifier GetColorModifier(UnitColor color)
            {
                return color switch
                {
                    UnitColor.Blue => new StatModifier(0, -15, 10, 4),
                    UnitColor.Green => new StatModifier(-100, 20, -5, 0),
                    UnitColor.Red => new StatModifier(200, 40, -9, 0),
                    _ => new StatModifier(0, 0, 0, 0)
                };
            }
        }

        private sealed class TestWeightCatalog : IUnitTraitWeightCatalog
        {
            public int GetShapeWeight(UnitShape shape) => 1;
            public int GetSizeWeight(UnitSize size) => 1;
            public int GetColorWeight(UnitColor color) => 1;
        }
    }
}
