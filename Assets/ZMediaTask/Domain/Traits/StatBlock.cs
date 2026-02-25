namespace ZMediaTask.Domain.Traits
{
    public readonly struct StatBlock
    {
        public StatBlock(int hp, int atk, int speed, int atkspd)
        {
            HP = hp;
            ATK = atk;
            SPEED = speed;
            ATKSPD = atkspd;
        }

        public int HP { get; }

        public int ATK { get; }

        public int SPEED { get; }

        public int ATKSPD { get; }

        public StatBlock ClampMin(int minValue)
        {
            return new StatBlock(
                HP < minValue ? minValue : HP,
                ATK < minValue ? minValue : ATK,
                SPEED < minValue ? minValue : SPEED,
                ATKSPD < minValue ? minValue : ATKSPD);
        }

        public static StatBlock operator +(StatBlock block, StatModifier modifier)
        {
            return new StatBlock(
                block.HP + modifier.HP,
                block.ATK + modifier.ATK,
                block.SPEED + modifier.SPEED,
                block.ATKSPD + modifier.ATKSPD);
        }
    }
}
