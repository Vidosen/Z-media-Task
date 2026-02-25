using System;
using System.Collections.Generic;
using UnityEngine;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    [CreateAssetMenu(
        fileName = "UnitTraitWeightCatalog",
        menuName = "ZMediaTask/Config/Unit Trait Weight Catalog")]
    public sealed class UnitTraitWeightCatalogAsset : ScriptableObject
    {
        [Serializable]
        public struct ShapeWeightEntry
        {
            public UnitShape Shape;
            public int Weight;
        }

        [Serializable]
        public struct SizeWeightEntry
        {
            public UnitSize Size;
            public int Weight;
        }

        [Serializable]
        public struct ColorWeightEntry
        {
            public UnitColor Color;
            public int Weight;
        }

        [SerializeField]
        private List<ShapeWeightEntry> _shapeWeights = new();

        [SerializeField]
        private List<SizeWeightEntry> _sizeWeights = new();

        [SerializeField]
        private List<ColorWeightEntry> _colorWeights = new();

        public List<ShapeWeightEntry> ShapeWeights => _shapeWeights;

        public List<SizeWeightEntry> SizeWeights => _sizeWeights;

        public List<ColorWeightEntry> ColorWeights => _colorWeights;
    }
}
