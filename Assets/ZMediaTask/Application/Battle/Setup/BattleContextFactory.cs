using System;
using ZMediaTask.Application.Army;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class BattleContextFactory
    {
        private IFormationStrategy _leftFormationStrategy;
        private IFormationStrategy _rightFormationStrategy;
        private readonly float _spawnOffsetX;

        public BattleContextFactory(IFormationStrategy formationStrategy, float spawnOffsetX = 8f)
            : this(formationStrategy, formationStrategy, spawnOffsetX)
        {
        }

        public BattleContextFactory(
            IFormationStrategy leftFormationStrategy,
            IFormationStrategy rightFormationStrategy,
            float spawnOffsetX = 8f)
        {
            _leftFormationStrategy = leftFormationStrategy
                ?? throw new ArgumentNullException(nameof(leftFormationStrategy));
            _rightFormationStrategy = rightFormationStrategy
                ?? throw new ArgumentNullException(nameof(rightFormationStrategy));
            _spawnOffsetX = spawnOffsetX;
        }
        
        public void SetFormationStrategy(ArmySide side, IFormationStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            if (side == ArmySide.Left)
            {
                _leftFormationStrategy = strategy;
                return;
            }

            _rightFormationStrategy = strategy;
        }

        public BattleContext Create(ArmyPair armies)
        {
            var totalCount = armies.Left.Units.Count + armies.Right.Units.Count;
            var units = new BattleUnitRuntime[totalCount];
            var nextUnitId = 1;
            var writeIndex = 0;

            writeIndex = AddArmyUnits(armies.Left, units, writeIndex, ref nextUnitId);
            AddArmyUnits(armies.Right, units, writeIndex, ref nextUnitId);

            return new BattleContext(units, 0f, null);
        }

        private int AddArmyUnits(
            ZMediaTask.Domain.Army.Army army,
            BattleUnitRuntime[] destination,
            int startIndex,
            ref int nextUnitId)
        {
            var index = startIndex;
            var unitCount = army.Units.Count;
            var formationStrategy = GetFormationStrategy(army.Side);
            for (var i = 0; i < unitCount; i++)
            {
                var armyUnit = army.Units[i];
                var unitId = nextUnitId++;
                var stats = armyUnit.Stats;
                var hp = Math.Max(0, stats.HP);
                var speed = Math.Max(0, stats.SPEED);
                var attack = Math.Max(0, stats.ATK);
                var attackSpeed = Math.Max(0, stats.ATKSPD);
                var spawnPosition = formationStrategy.ComputePosition(army.Side, i, unitCount, _spawnOffsetX);

                var movement = new MovementAgentState(
                    unitId,
                    hp > 0,
                    speed,
                    spawnPosition,
                    targetId: null,
                    currentPath: Array.Empty<BattlePoint>(),
                    lastPathTargetPosition: null);
                var combat = new CombatUnitState(
                    unitId,
                    spawnPosition,
                    hp,
                    attack,
                    attackSpeed,
                    nextAttackTimeSec: 0f);

                destination[index++] = new BattleUnitRuntime(
                    unitId, army.Side, armyUnit.Shape, armyUnit.Size, armyUnit.Color, movement, combat);
            }

            return index;
        }

        private IFormationStrategy GetFormationStrategy(ArmySide side)
        {
            return side == ArmySide.Left ? _leftFormationStrategy : _rightFormationStrategy;
        }
    }
}
