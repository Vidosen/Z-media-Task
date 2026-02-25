using System;
using ZMediaTask.Application.Army;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Application.Battle
{
    public sealed class BattleContextFactory
    {
        private const float SpawnOffsetX = 8f;
        private const float SpawnSpacingZ = 1.5f;

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

        private static int AddArmyUnits(
            ZMediaTask.Domain.Army.Army army,
            BattleUnitRuntime[] destination,
            int startIndex,
            ref int nextUnitId)
        {
            var index = startIndex;
            var unitCount = army.Units.Count;
            for (var i = 0; i < unitCount; i++)
            {
                var armyUnit = army.Units[i];
                var unitId = nextUnitId++;
                var stats = armyUnit.Stats;
                var hp = Math.Max(0, stats.HP);
                var speed = Math.Max(0, stats.SPEED);
                var attack = Math.Max(0, stats.ATK);
                var attackSpeed = Math.Max(0, stats.ATKSPD);
                var spawnPosition = BuildSpawnPosition(army.Side, i, unitCount);

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

        private static BattlePoint BuildSpawnPosition(ArmySide side, int index, int total)
        {
            var x = side == ArmySide.Left ? -SpawnOffsetX : SpawnOffsetX;
            var centerOffset = (total - 1) * 0.5f;
            var z = (index - centerOffset) * SpawnSpacingZ;
            return new BattlePoint(x, z);
        }
    }
}
