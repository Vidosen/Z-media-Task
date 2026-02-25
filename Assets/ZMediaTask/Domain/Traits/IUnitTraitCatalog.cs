namespace ZMediaTask.Domain.Traits
{
    public interface IUnitTraitCatalog
    {
        StatModifier GetShapeModifier(UnitShape shape);

        StatModifier GetSizeModifier(UnitSize size);

        StatModifier GetColorModifier(UnitColor color);
    }
}
