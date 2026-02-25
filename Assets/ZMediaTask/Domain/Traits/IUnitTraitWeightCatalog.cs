namespace ZMediaTask.Domain.Traits
{
    public interface IUnitTraitWeightCatalog
    {
        int GetShapeWeight(UnitShape shape);

        int GetSizeWeight(UnitSize size);

        int GetColorWeight(UnitColor color);
    }
}
