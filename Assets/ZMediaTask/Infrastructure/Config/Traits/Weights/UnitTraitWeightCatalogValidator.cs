using System;
using System.Collections.Generic;
using System.Linq;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    public static class UnitTraitWeightCatalogValidator
    {
        public static void Validate(UnitTraitWeightCatalogAsset catalogAsset)
        {
            if (catalogAsset == null)
            {
                throw new ArgumentNullException(nameof(catalogAsset));
            }

            ValidateEntries(
                catalogAsset.ShapeWeights,
                entry => entry.Shape,
                entry => entry.Weight,
                Enum.GetValues(typeof(UnitShape)).Cast<UnitShape>().ToArray(),
                "Shape");
            ValidateEntries(
                catalogAsset.SizeWeights,
                entry => entry.Size,
                entry => entry.Weight,
                Enum.GetValues(typeof(UnitSize)).Cast<UnitSize>().ToArray(),
                "Size");
            ValidateEntries(
                catalogAsset.ColorWeights,
                entry => entry.Color,
                entry => entry.Weight,
                Enum.GetValues(typeof(UnitColor)).Cast<UnitColor>().ToArray(),
                "Color");
        }

        private static void ValidateEntries<TEntry, TEnum>(
            IReadOnlyList<TEntry> entries,
            Func<TEntry, TEnum> keySelector,
            Func<TEntry, int> weightSelector,
            IReadOnlyCollection<TEnum> requiredValues,
            string sectionName)
            where TEnum : struct, Enum
        {
            var keyList = entries.Select(keySelector).ToList();
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
            var negativeWeightKeys = entries
                .Where(entry => weightSelector(entry) < 0)
                .Select(entry => keySelector(entry).ToString())
                .OrderBy(value => value)
                .ToArray();
            var totalWeight = entries.Sum(weightSelector);

            if (duplicates.Length > 0 || missing.Length > 0 || negativeWeightKeys.Length > 0)
            {
                throw new InvalidOperationException(
                    $"{sectionName} weight catalog is invalid. Missing: [{string.Join(", ", missing)}]; Duplicates: [{string.Join(", ", duplicates)}]; Negative: [{string.Join(", ", negativeWeightKeys)}].");
            }

            if (totalWeight <= 0)
            {
                throw new InvalidOperationException(
                    $"{sectionName} total weight must be greater than zero.");
            }
        }
    }
}
