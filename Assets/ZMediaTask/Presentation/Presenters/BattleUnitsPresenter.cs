using System.Collections.Generic;
using UnityEngine;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Traits;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class BattleUnitsPresenter : MonoBehaviour
    {
        private const float SpawnOffsetX = 8f;
        private const float SpawnSpacingZ = 1.5f;

        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private GameObject _spherePrefab;
        [SerializeField] private Transform _unitsParent;

        private readonly Dictionary<int, GameObject> _unitObjects = new();

        public void SpawnUnits(BattleContext context)
        {
            ClearUnits();

            for (var i = 0; i < context.Units.Count; i++)
            {
                var unit = context.Units[i];
                var prefab = GetPrefabForShape(unit.Shape);
                if (prefab == null)
                {
                    continue;
                }

                var position = new Vector3(unit.Movement.Position.X, 0.5f, unit.Movement.Position.Z);
                var go = Instantiate(prefab, position, Quaternion.identity, _unitsParent);
                go.name = $"Unit_{unit.UnitId}_{unit.Side}";

                ApplyVisualStyle(go, unit.Side, unit.Size);
                _unitObjects[unit.UnitId] = go;
            }
        }

        public void SpawnPreview(ArmyPair armies)
        {
            ClearUnits();

            SpawnPreviewArmy(armies.Left, ArmySide.Left);
            SpawnPreviewArmy(armies.Right, ArmySide.Right);
        }

        public void SyncPositions(BattleContext context)
        {
            for (var i = 0; i < context.Units.Count; i++)
            {
                var unit = context.Units[i];
                if (!_unitObjects.TryGetValue(unit.UnitId, out var go))
                {
                    continue;
                }

                if (!unit.Combat.IsAlive)
                {
                    go.SetActive(false);
                    continue;
                }

                go.transform.position = new Vector3(
                    unit.Movement.Position.X,
                    0.5f,
                    unit.Movement.Position.Z);
            }
        }

        public void ClearUnits()
        {
            foreach (var go in _unitObjects.Values)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }

            _unitObjects.Clear();
        }

        private void SpawnPreviewArmy(ZMediaTask.Domain.Army.Army army, ArmySide side)
        {
            var unitCount = army.Units.Count;
            for (var i = 0; i < unitCount; i++)
            {
                var armyUnit = army.Units[i];
                var prefab = GetPrefabForShape(armyUnit.Shape);
                if (prefab == null)
                {
                    continue;
                }

                var x = side == ArmySide.Left ? -SpawnOffsetX : SpawnOffsetX;
                var centerOffset = (unitCount - 1) * 0.5f;
                var z = (i - centerOffset) * SpawnSpacingZ;
                var position = new Vector3(x, 0.5f, z);

                var previewId = -(side == ArmySide.Left ? i + 1 : unitCount + i + 1);
                var go = Instantiate(prefab, position, Quaternion.identity, _unitsParent);
                go.name = $"Preview_{previewId}_{side}";

                ApplyVisualStyle(go, side, armyUnit.Size);
                _unitObjects[previewId] = go;
            }
        }

        private GameObject GetPrefabForShape(UnitShape shape)
        {
            return shape == UnitShape.Sphere ? _spherePrefab : _cubePrefab;
        }

        private void ApplyVisualStyle(GameObject go, ArmySide side, UnitSize size)
        {
            var color = side == ArmySide.Left
                ? new Color(0.2f, 0.4f, 0.9f)
                : new Color(0.9f, 0.2f, 0.2f);

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            var scale = size == UnitSize.Big ? 1.0f : 0.6f;
            go.transform.localScale = Vector3.one * scale;
        }

        private void OnDestroy()
        {
            ClearUnits();
        }
    }
}
