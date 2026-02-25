using System;
using System.Collections.Generic;
using System.Linq;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    public sealed class ScriptableObjectUnitTraitWeightCatalog : IUnitTraitWeightCatalog
    {
        private readonly IReadOnlyDictionary<UnitShape, int> _shapeWeights;
        private readonly IReadOnlyDictionary<UnitSize, int> _sizeWeights;
        private readonly IReadOnlyDictionary<UnitColor, int> _colorWeights;

        public ScriptableObjectUnitTraitWeightCatalog(UnitTraitWeightCatalogAsset catalogAsset)
        {
            if (catalogAsset == null)
            {
                throw new ArgumentNullException(nameof(catalogAsset));
            }

            UnitTraitWeightCatalogValidator.Validate(catalogAsset);

            _shapeWeights = catalogAsset.ShapeWeights.ToDictionary(entry => entry.Shape, entry => entry.Weight);
            _sizeWeights = catalogAsset.SizeWeights.ToDictionary(entry => entry.Size, entry => entry.Weight);
            _colorWeights = catalogAsset.ColorWeights.ToDictionary(entry => entry.Color, entry => entry.Weight);
        }

        public int GetShapeWeight(UnitShape shape)
        {
            return GetOrThrow(_shapeWeights, shape, "shape");
        }

        public int GetSizeWeight(UnitSize size)
        {
            return GetOrThrow(_sizeWeights, size, "size");
        }

        public int GetColorWeight(UnitColor color)
        {
            return GetOrThrow(_colorWeights, color, "color");
        }

        private static int GetOrThrow<TKey>(IReadOnlyDictionary<TKey, int> dictionary, TKey key, string section)
        {
            if (dictionary.TryGetValue(key, out var weight))
            {
                return weight;
            }

            throw new KeyNotFoundException(
                $"Trait weight for {section} '{key}' was not found in the catalog.");
        }
    }
}
