using System;
using NUnit.Framework;
using UnityEngine;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Infrastructure.Config.Traits;

namespace ZMediaTask.Tests.EditMode.Infrastructure.Config.Traits
{
    public class UnitTraitWeightCatalogValidatorTests
    {
        [Test]
        public void UnitTraitWeightCatalogValidator_WhenEmptyCatalog_Throws()
        {
            var asset = ScriptableObject.CreateInstance<UnitTraitWeightCatalogAsset>();
            try
            {
                var exception = Assert.Throws<InvalidOperationException>(
                    () => UnitTraitWeightCatalogValidator.Validate(asset));

                StringAssert.Contains("Missing", exception.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void UnitTraitWeightCatalogValidator_WhenSectionHasNoPositiveWeights_Throws()
        {
            var asset = CreateCompleteAsset(weight: 1);
            try
            {
                for (var i = 0; i < asset.ShapeWeights.Count; i++)
                {
                    var entry = asset.ShapeWeights[i];
                    entry.Weight = 0;
                    asset.ShapeWeights[i] = entry;
                }

                var exception = Assert.Throws<InvalidOperationException>(
                    () => UnitTraitWeightCatalogValidator.Validate(asset));

                StringAssert.Contains("greater than zero", exception.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void UnitTraitWeightCatalogValidator_WhenCompleteCatalog_DoesNotThrow()
        {
            var asset = CreateCompleteAsset(weight: 1);
            try
            {
                Assert.DoesNotThrow(() => UnitTraitWeightCatalogValidator.Validate(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        private static UnitTraitWeightCatalogAsset CreateCompleteAsset(int weight)
        {
            var asset = ScriptableObject.CreateInstance<UnitTraitWeightCatalogAsset>();

            foreach (UnitShape shape in Enum.GetValues(typeof(UnitShape)))
            {
                asset.ShapeWeights.Add(new UnitTraitWeightCatalogAsset.ShapeWeightEntry
                {
                    Shape = shape,
                    Weight = weight
                });
            }

            foreach (UnitSize size in Enum.GetValues(typeof(UnitSize)))
            {
                asset.SizeWeights.Add(new UnitTraitWeightCatalogAsset.SizeWeightEntry
                {
                    Size = size,
                    Weight = weight
                });
            }

            foreach (UnitColor color in Enum.GetValues(typeof(UnitColor)))
            {
                asset.ColorWeights.Add(new UnitTraitWeightCatalogAsset.ColorWeightEntry
                {
                    Color = color,
                    Weight = weight
                });
            }

            return asset;
        }
    }
}
