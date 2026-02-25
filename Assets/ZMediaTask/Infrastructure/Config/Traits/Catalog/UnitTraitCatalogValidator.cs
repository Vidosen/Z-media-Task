using System;
using System.Collections.Generic;
using System.Linq;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    public static class UnitTraitCatalogValidator
    {
        public static void Validate(UnitTraitCatalogAsset catalogAsset)
        {
            if (catalogAsset == null)
            {
                throw new ArgumentNullException(nameof(catalogAsset));
            }

            ValidateEntries(
                catalogAsset.ShapeModifiers.Select(entry => entry.Shape),
                Enum.GetValues(typeof(UnitShape)).Cast<UnitShape>().ToArray(),
                "Shape");
            ValidateEntries(
                catalogAsset.SizeModifiers.Select(entry => entry.Size),
                Enum.GetValues(typeof(UnitSize)).Cast<UnitSize>().ToArray(),
                "Size");
            ValidateEntries(
                catalogAsset.ColorModifiers.Select(entry => entry.Color),
                Enum.GetValues(typeof(UnitColor)).Cast<UnitColor>().ToArray(),
                "Color");
        }

        private static void ValidateEntries<TEnum>(
            IEnumerable<TEnum> keys,
            IReadOnlyCollection<TEnum> requiredValues,
            string sectionName)
            where TEnum : struct, Enum
        {
            var keyList = keys.ToList();
            var duplicates = keyList
                .GroupBy(key => key)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.ToString())
                .OrderBy(value => value)
                .ToArray();
            var missing = requiredValues
                .Except(keyList)
                .Select(value => value.ToString())
                .OrderBy(value => value)
                .ToArray();

            if (duplicates.Length == 0 && missing.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $"{sectionName} trait catalog is invalid. Missing: [{string.Join(", ", missing)}]; Duplicates: [{string.Join(", ", duplicates)}].");
        }
    }
}
