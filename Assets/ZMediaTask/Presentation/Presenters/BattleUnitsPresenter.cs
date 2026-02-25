using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Domain.Traits;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class BattleUnitsPresenter : MonoBehaviour
    {
        private const float DeathImpulseForce = 5f;
        private const float FadeStartDelay = 0.5f;
        private const float FadeDuration = 1.0f;

        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private GameObject _spherePrefab;
        [SerializeField] private Transform _unitsParent;
        [SerializeField] private Shader _unitFlashShader;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Dictionary<int, GameObject> _unitObjects = new();
        private readonly Dictionary<int, Renderer> _unitRenderers = new();
        private readonly HashSet<int> _dyingUnitIds = new();
        private DeathTracker _deathTracker;
        private DamageFlashTracker _flashTracker;

        public void SpawnUnits(BattleContext context)
        {
            ClearUnits();
            _deathTracker = new DeathTracker();
            _flashTracker = new DamageFlashTracker();

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

                var renderer = go.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    _unitRenderers[unit.UnitId] = renderer;
                }
            }
        }

        public void SpawnPreview(BattleContext previewContext)
        {
            ClearUnits();

            for (var i = 0; i < previewContext.Units.Count; i++)
            {
                var unit = previewContext.Units[i];
                var prefab = GetPrefabForShape(unit.Shape);
                if (prefab == null)
                {
                    continue;
                }

                var previewId = -(i + 1);
                var position = new Vector3(unit.Movement.Position.X, 0.5f, unit.Movement.Position.Z);
                var go = Instantiate(prefab, position, Quaternion.identity, _unitsParent);
                go.name = $"Preview_{previewId}_{unit.Side}";

                ApplyVisualStyle(go, unit.Side, unit.Size);
                _unitObjects[previewId] = go;
            }
        }

        public void SyncPositions(BattleContext context)
        {
            var newlyDead = _deathTracker?.DetectNewDeaths(context.Units);

            for (var i = 0; i < context.Units.Count; i++)
            {
                var unit = context.Units[i];
                if (!_unitObjects.TryGetValue(unit.UnitId, out var go))
                {
                    continue;
                }

                if (_dyingUnitIds.Contains(unit.UnitId))
                {
                    continue;
                }

                if (!unit.Combat.IsAlive)
                {
                    if (newlyDead != null && ContainsId(newlyDead, unit.UnitId))
                    {
                        RunDeathSequence(go, unit, context.Units);
                    }
                    else
                    {
                        go.SetActive(false);
                    }
                    continue;
                }

                go.transform.position = new Vector3(
                    unit.Movement.Position.X, 0.5f, unit.Movement.Position.Z);
            }
        }

        public void ClearUnits()
        {
            DOTween.Kill(this);
            foreach (var go in _unitObjects.Values)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }

            _unitObjects.Clear();
            _unitRenderers.Clear();
            _dyingUnitIds.Clear();
            _deathTracker?.Reset();
            _deathTracker = null;
            _flashTracker?.Clear();
            _flashTracker = null;
        }

        public void FlashUnit(int unitId)
        {
            if (_flashTracker != null && _unitRenderers.TryGetValue(unitId, out var renderer))
            {
                _flashTracker.Flash(unitId, renderer);
            }
        }

        private void RunDeathSequence(GameObject go, BattleUnitRuntime deadUnit, IReadOnlyList<BattleUnitRuntime> allUnits)
        {
            _dyingUnitIds.Add(deadUnit.UnitId);

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;

            var impulseDir = ComputeDeathImpulseDirection(deadUnit, allUnits);
            rb.AddForce(impulseDir * DeathImpulseForce, ForceMode.Impulse);

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                SetMaterialTransparent(renderer.material);
            }

            var sequence = DOTween.Sequence();
            sequence.SetId(this);
            sequence.AppendInterval(FadeStartDelay);

            if (renderer != null)
            {
                var mat = renderer.material;
                sequence.Append(
                    DOTween.To(
                        () => mat.GetColor(BaseColorId).a,
                        a =>
                        {
                            var c = mat.GetColor(BaseColorId);
                            mat.SetColor(BaseColorId, new Color(c.r, c.g, c.b, a));
                        },
                        0f, FadeDuration));
            }
            else
            {
                sequence.AppendInterval(FadeDuration);
            }

            sequence.AppendCallback(() =>
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            });
        }

        private static Vector3 ComputeDeathImpulseDirection(BattleUnitRuntime deadUnit, IReadOnlyList<BattleUnitRuntime> allUnits)
        {
            var deadPos = new Vector3(deadUnit.Movement.Position.X, 0f, deadUnit.Movement.Position.Z);
            var nearestDistSqr = float.MaxValue;
            var nearestPos = Vector3.zero;
            var foundEnemy = false;

            for (var i = 0; i < allUnits.Count; i++)
            {
                var other = allUnits[i];
                if (other.Side == deadUnit.Side || !other.Combat.IsAlive)
                {
                    continue;
                }

                var otherPos = new Vector3(other.Movement.Position.X, 0f, other.Movement.Position.Z);
                var distSqr = (otherPos - deadPos).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestPos = otherPos;
                    foundEnemy = true;
                }
            }

            Vector3 awayDir;
            if (foundEnemy && nearestDistSqr > 0.001f)
            {
                awayDir = (deadPos - nearestPos).normalized;
            }
            else
            {
                awayDir = deadUnit.Side == ArmySide.Left ? Vector3.left : Vector3.right;
            }

            return (awayDir + Vector3.up).normalized;
        }

        private static void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        private static bool ContainsId(IReadOnlyList<int> list, int id)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == id) return true;
            }
            return false;
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
                if (_unitFlashShader != null)
                {
                    var mat = new Material(_unitFlashShader);
                    mat.SetColor(BaseColorId, color);
                    renderer.material = mat;
                }
                else
                {
                    renderer.material.color = color;
                }
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
