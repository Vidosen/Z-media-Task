namespace ZMediaTask.Domain.Traits
{
    public sealed class StatsCalculator
    {
        private readonly IUnitTraitCatalog _traitCatalog;

        public StatsCalculator(IUnitTraitCatalog traitCatalog)
        {
            _traitCatalog = traitCatalog;
        }

        public StatBlock Calculate(StatBlock baseStats, UnitShape shape, UnitSize size, UnitColor color)
        {
            var modifier = StatModifier.Combine(
                _traitCatalog.GetShapeModifier(shape),
                _traitCatalog.GetSizeModifier(size),
                _traitCatalog.GetColorModifier(color));

            return (baseStats + modifier).ClampMin(0);
        }
    }
}
