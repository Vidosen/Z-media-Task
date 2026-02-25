using System;

namespace ZMediaTask.Domain.Traits
{
    [Serializable]
    public struct StatModifier
    {
        public static StatModifier Zero { get; } = new(0, 0, 0, 0);

        public StatModifier(int hp, int atk, int speed, int atkspd)
        {
            HP = hp;
            ATK = atk;
            SPEED = speed;
            ATKSPD = atkspd;
        }

        public int HP;

        public int ATK;

        public int SPEED;

        public int ATKSPD;

        public static StatModifier operator +(StatModifier left, StatModifier right)
        {
            return new StatModifier(
                left.HP + right.HP,
                left.ATK + right.ATK,
                left.SPEED + right.SPEED,
                left.ATKSPD + right.ATKSPD);
        }

        public static StatModifier Combine(params StatModifier[] modifiers)
        {
            var total = Zero;
            foreach (var modifier in modifiers)
            {
                total += modifier;
            }

            return total;
        }
    }
}
