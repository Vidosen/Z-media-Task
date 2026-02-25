using System;

namespace ZMediaTask.Domain.Combat
{
    public readonly struct CombatUnitState
    {
        public CombatUnitState(
            int unitId,
            BattlePoint position,
            int currentHp,
            int attack,
            int attackSpeed,
            float nextAttackTimeSec)
        {
            if (currentHp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP must be >= 0.");
            }

            if (attack < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attack), "Attack must be >= 0.");
            }

            if (attackSpeed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackSpeed), "Attack speed must be >= 0.");
            }

            UnitId = unitId;
            Position = position;
            CurrentHp = currentHp;
            Attack = attack;
            AttackSpeed = attackSpeed;
            NextAttackTimeSec = nextAttackTimeSec;
        }

        public int UnitId { get; }

        public BattlePoint Position { get; }

        public int CurrentHp { get; }

        public int Attack { get; }

        public int AttackSpeed { get; }

        public float NextAttackTimeSec { get; }

        public bool IsAlive => CurrentHp > 0;

        public CombatUnitState WithCurrentHp(int currentHp)
        {
            return new CombatUnitState(
                UnitId,
                Position,
                currentHp,
                Attack,
                AttackSpeed,
                NextAttackTimeSec);
        }

        public CombatUnitState WithPosition(BattlePoint position)
        {
            return new CombatUnitState(
                UnitId,
                position,
                CurrentHp,
                Attack,
                AttackSpeed,
                NextAttackTimeSec);
        }

        public CombatUnitState WithNextAttackTimeSec(float nextAttackTimeSec)
        {
            return new CombatUnitState(
                UnitId,
                Position,
                CurrentHp,
                Attack,
                AttackSpeed,
                nextAttackTimeSec);
        }
    }
}
