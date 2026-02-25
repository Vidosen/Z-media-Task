using System;
using System.Collections.Generic;
using System.Linq;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    public sealed class ScriptableObjectUnitTraitCatalog : IUnitTraitCatalog
    {
        private readonly IReadOnlyDictionary<UnitShape, StatModifier> _shapeModifiers;
        private readonly IReadOnlyDictionary<UnitSize, StatModifier> _sizeModifiers;
        private readonly IReadOnlyDictionary<UnitColor, StatModifier> _colorModifiers;

        public ScriptableObjectUnitTraitCatalog(UnitTraitCatalogAsset catalogAsset)
        {
            UnitTraitCatalogValidator.Validate(catalogAsset);

            _shapeModifiers = catalogAsset.ShapeModifiers.ToDictionary(entry => entry.Shape, entry => entry.Modifier);
            _sizeModifiers = catalogAsset.SizeModifiers.ToDictionary(entry => entry.Size, entry => entry.Modifier);
            _colorModifiers = catalogAsset.ColorModifiers.ToDictionary(entry => entry.Color, entry => entry.Modifier);
        }

        public StatModifier GetShapeModifier(UnitShape shape)
        {
            return GetOrThrow(_shapeModifiers, shape, "shape");
        }

        public StatModifier GetSizeModifier(UnitSize size)
        {
            return GetOrThrow(_sizeModifiers, size, "size");
        }

        public StatModifier GetColorModifier(UnitColor color)
        {
            return GetOrThrow(_colorModifiers, color, "color");
        }

        private static StatModifier GetOrThrow<TKey>(
            IReadOnlyDictionary<TKey, StatModifier> dictionary,
            TKey key,
            string section)
        {
            if (dictionary.TryGetValue(key, out var modifier))
            {
                return modifier;
            }

            throw new KeyNotFoundException(
                $"Trait modifier for {section} '{key}' was not found in the catalog.");
        }
    }
}
