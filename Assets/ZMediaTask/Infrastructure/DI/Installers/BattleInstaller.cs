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
        [SerializeField] private FormationConfigAsset _formationConfigAsset;

        public void InstallBindings(ContainerBuilder builder)
        {
            var traitCatalog = new ScriptableObjectUnitTraitCatalog(_traitCatalogAsset);
            var weightCatalog = new ScriptableObjectUnitTraitWeightCatalog(_weightCatalogAsset);

            var movementConfig = _combatConfigAsset.BuildMovementConfig();
            var attackConfig = _combatConfigAsset.BuildAttackConfig();
            var wrathConfig = _combatConfigAsset.BuildWrathConfig();
            var knockbackConfig = _combatConfigAsset.BuildKnockbackConfig();
            var arenaBounds = _combatConfigAsset.BuildArenaBounds();

            builder.RegisterValue(movementConfig);
            builder.RegisterValue(attackConfig);
            builder.RegisterValue(wrathConfig);
            builder.RegisterValue(knockbackConfig);
            builder.RegisterValue(arenaBounds);
            builder.RegisterValue(_formationConfigAsset);


            builder.RegisterType(typeof(SystemRandomProvider),new[]{ typeof(IRandomProvider) }, Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterValue(traitCatalog, new[]{ typeof(IUnitTraitCatalog) });
            builder.RegisterValue(weightCatalog, new[]{ typeof(IUnitTraitWeightCatalog) });

            builder.RegisterType(typeof(StatsCalculator), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(ArmyFactory), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(ArmyRandomizationUseCase), Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(HealthService), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(AttackService), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(WrathService), Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(NearestTargetSelector),
                new[] { typeof(ITargetSelector) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(DirectPathfinder),
                new[] { typeof(IPathfinder) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(SpatialHashSteeringService),new[]{ typeof(ISteeringService) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(RingSlotAllocator),
                new[] { typeof(ISlotAllocator) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(MovementService), Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(ArenaBoundsWrathTargetValidator),new[]{ typeof(IWrathTargetValidator) }, Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory(
                c => new OnUnitKilledUseCase(ArmySide.Left, wrathConfig, c.Resolve<WrathService>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterFactory(c => new TryCastWrathUseCase(
                    ArmySide.Left, wrathConfig, c.Resolve<WrathService>(),
                    c.Resolve<IWrathTargetValidator>()),
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(DistanceUnitQueryInRadius),
                new[] { typeof(IUnitQueryInRadius) }, Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(KnockbackService), Lifetime.Singleton, Resolution.Lazy);

            builder.RegisterType(typeof(AutoBattleStepProcessor),new[]{ typeof(IBattleStepProcessor) }, Lifetime.Singleton, Resolution.Lazy);
            
            builder.RegisterFactory(c =>
                {
                    var formationConfig = c.Resolve<FormationConfigAsset>();
                    return new BattleContextFactory(formationConfig.BuildFormationStrategy(), formationConfig.SpawnOffsetX);
                },
                Lifetime.Singleton, Resolution.Lazy);
            builder.RegisterType(typeof(BattleLoopService), Lifetime.Singleton, Resolution.Lazy);
        }
    }
}
