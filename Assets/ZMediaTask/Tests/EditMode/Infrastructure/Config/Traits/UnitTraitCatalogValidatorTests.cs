using System;
using NUnit.Framework;
using UnityEngine;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Infrastructure.Config.Traits;

namespace ZMediaTask.Tests.EditMode.Infrastructure.Config.Traits
{
    public class UnitTraitCatalogValidatorTests
    {
        [Test]
        public void TraitCatalogValidator_WhenMissingEnumValue_Throws()
        {
            var asset = CreateCompleteAsset();
            try
            {
                asset.ColorModifiers.RemoveAt(0);

                var exception = Assert.Throws<InvalidOperationException>(() => UnitTraitCatalogValidator.Validate(asset));

                StringAssert.Contains("Missing", exception!.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void TraitCatalogValidator_WhenDuplicateEnumValue_Throws()
        {
            var asset = CreateCompleteAsset();
            try
            {
                asset.ShapeModifiers.Add(new UnitTraitCatalogAsset.ShapeModifierEntry
                {
                    Shape = UnitShape.Cube,
                    Modifier = StatModifier.Zero
                });

                var exception = Assert.Throws<InvalidOperationException>(() => UnitTraitCatalogValidator.Validate(asset));

                StringAssert.Contains("Duplicates", exception!.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void TraitCatalogValidator_WhenCompleteCatalog_DoesNotThrow()
        {
            var asset = CreateCompleteAsset();
            try
            {
                Assert.DoesNotThrow(() => UnitTraitCatalogValidator.Validate(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        private static UnitTraitCatalogAsset CreateCompleteAsset()
        {
            var asset = ScriptableObject.CreateInstance<UnitTraitCatalogAsset>();

            foreach (UnitShape shape in Enum.GetValues(typeof(UnitShape)))
            {
                asset.ShapeModifiers.Add(new UnitTraitCatalogAsset.ShapeModifierEntry
                {
                    Shape = shape,
                    Modifier = StatModifier.Zero
                });
            }

            foreach (UnitSize size in Enum.GetValues(typeof(UnitSize)))
            {
                asset.SizeModifiers.Add(new UnitTraitCatalogAsset.SizeModifierEntry
                {
                    Size = size,
                    Modifier = StatModifier.Zero
                });
            }

            foreach (UnitColor color in Enum.GetValues(typeof(UnitColor)))
            {
                asset.ColorModifiers.Add(new UnitTraitCatalogAsset.ColorModifierEntry
                {
                    Color = color,
                    Modifier = StatModifier.Zero
                });
            }

            return asset;
        }
    }
}
