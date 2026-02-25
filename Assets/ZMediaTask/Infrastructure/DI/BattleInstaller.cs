using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Random;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Infrastructure.Config.Combat;
using ZMediaTask.Infrastructure.Config.Traits;
using ZMediaTask.Infrastructure.Random;

namespace ZMediaTask.Infrastructure.DI
{
    public sealed class BattleInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private UnitTraitCatalogAsset _traitCatalogAsset;
        [SerializeField] private UnitTraitWeightCatalogAsset _weightCatalogAsset;
        [SerializeField] private CombatGameplayConfigAsset _combatConfigAsset;

        public void InstallBindings(ContainerBuilder builder)
        {
            var traitCatalog = new ScriptableObjectUnitTraitCatalog(_traitCatalogAsset);
            var weightCatalog = new ScriptableObjectUnitTraitWeightCatalog(_weightCatalogAsset);

            var movementConfig = _combatConfigAsset.BuildMovementConfig();
            var attackConfig = _combatConfigAsset.BuildAttackConfig();
            var wrathConfig = _combatConfigAsset.BuildWrathConfig();
            var arenaBounds = _combatConfigAsset.BuildArenaBounds();

            builder.RegisterValue(movementConfig);
            builder.RegisterValue(attackConfig);
            builder.RegisterValue(wrathConfig);
            builder.RegisterValue(arenaBounds);

            builder.RegisterFactory<IRandomProvider>(
                _ => new SystemRandomProvider(), Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterFactory<IUnitTraitCatalog>(
                _ => traitCatalog, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<IUnitTraitWeightCatalog>(
                _ => weightCatalog, Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterFactory<StatsCalculator>(
                c => new StatsCalculator(c.Resolve<IUnitTraitCatalog>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<ArmyFactory>(
                c => new ArmyFactory(c.Resolve<StatsCalculator>(), c.Resolve<IUnitTraitWeightCatalog>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<ArmyRandomizationUseCase>(
                c => new ArmyRandomizationUseCase(c.Resolve<ArmyFactory>(), c.Resolve<IRandomProvider>()),
                Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(HealthService), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(CooldownTracker), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<AttackService>(
                c => new AttackService(c.Resolve<CooldownTracker>(), c.Resolve<HealthService>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<WrathService>(
                c => new WrathService(c.Resolve<HealthService>()),
                Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(NearestTargetSelector),
                new[] { typeof(ITargetSelector) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(DirectPathfinder),
                new[] { typeof(IPathfinder) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<ISteeringService>(
                _ => new SpatialHashSteeringService(),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(RingSlotAllocator),
                new[] { typeof(ISlotAllocator) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<MovementService>(
                c => new MovementService(
                    c.Resolve<ITargetSelector>(),
                    c.Resolve<IPathfinder>(),
                    c.Resolve<ISteeringService>(),
                    c.Resolve<ISlotAllocator>()),
                Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterFactory<IWrathTargetValidator>(
                _ => new ArenaBoundsWrathTargetValidator(arenaBounds),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<OnUnitKilledUseCase>(
                c => new OnUnitKilledUseCase(ArmySide.Left, wrathConfig, c.Resolve<WrathService>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<TryCastWrathUseCase>(
                c => new TryCastWrathUseCase(
                    ArmySide.Left, wrathConfig, c.Resolve<WrathService>(),
                    c.Resolve<IWrathTargetValidator>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(DistanceUnitQueryInRadius),
                new[] { typeof(IUnitQueryInRadius) }, Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterFactory<IBattleStepProcessor>(
                c => new AutoBattleStepProcessor(
                    c.Resolve<MovementService>(),
                    c.Resolve<AttackService>(),
                    c.Resolve<OnUnitKilledUseCase>(),
                    attackConfig,
                    movementConfig),
                Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(BattleContextFactory), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory<BattleLoopService>(
                c => new BattleLoopService(
                    c.Resolve<BattleContextFactory>(),
                    c.Resolve<IBattleStepProcessor>(),
                    c.Resolve<IUnitQueryInRadius>(),
                    c.Resolve<WrathService>(),
                    wrathConfig),
                Lifetime.Singleton, Resolution.Lazy);
        }
    }
}
