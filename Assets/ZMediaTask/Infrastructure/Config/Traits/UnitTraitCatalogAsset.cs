using System;
using System.Collections.Generic;
using UnityEngine;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Infrastructure.Config.Traits
{
    [CreateAssetMenu(
        fileName = "UnitTraitCatalog",
        menuName = "ZMediaTask/Config/Unit Trait Catalog")]
    public sealed class UnitTraitCatalogAsset : ScriptableObject
    {
        [Serializable]
        public struct ShapeModifierEntry
        {
            public UnitShape Shape;
            public StatModifier Modifier;
        }

        [Serializable]
        public struct SizeModifierEntry
        {
            public UnitSize Size;
            public StatModifier Modifier;
        }

        [Serializable]
        public struct ColorModifierEntry
        {
            public UnitColor Color;
            public StatModifier Modifier;
        }

        [SerializeField]
        private List<ShapeModifierEntry> _shapeModifiers = new();

        [SerializeField]
        private List<SizeModifierEntry> _sizeModifiers = new();

        [SerializeField]
        private List<ColorModifierEntry> _colorModifiers = new();

        public List<ShapeModifierEntry> ShapeModifiers => _shapeModifiers;

        public List<SizeModifierEntry> SizeModifiers => _sizeModifiers;

        public List<ColorModifierEntry> ColorModifiers => _colorModifiers;
    }
}
