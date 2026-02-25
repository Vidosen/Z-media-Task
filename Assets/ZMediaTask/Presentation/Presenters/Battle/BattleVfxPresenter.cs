using System.Collections.Generic;
using UnityEngine;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class BattleVfxPresenter : MonoBehaviour
    {
        private const int HitVfxPrewarmCount = 10;
        private const int LightningVfxPrewarmCount = 3;
        private const int TelegraphPrewarmCount = 1;

        private const float HitVfxReturnDelay = 1f;
        private const float LightningVfxReturnDelay = 2f;

        [SerializeField] private GameObject _hitVfxLeftPrefab;
        [SerializeField] private GameObject _hitVfxRightPrefab;
        [SerializeField] private GameObject _lightningVfxPrefab;
        [SerializeField] private GameObject _telegraphPrefab;
        [SerializeField] private VfxPool _vfxPool;

        public void Initialize()
        {
            if (_vfxPool == null) return;

            _vfxPool.Prewarm(_hitVfxLeftPrefab, HitVfxPrewarmCount);
            _vfxPool.Prewarm(_hitVfxRightPrefab, HitVfxPrewarmCount);
            _vfxPool.Prewarm(_lightningVfxPrefab, LightningVfxPrewarmCount);
            _vfxPool.Prewarm(_telegraphPrefab, TelegraphPrewarmCount);
        }

        public void ProcessEvents(IReadOnlyList<BattleEvent> events)
        {
            if (_vfxPool == null) return;

            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                switch (evt.Kind)
                {
                    case BattleEventKind.UnitDamaged:
                        HandleUnitDamaged(evt);
                        break;
                    case BattleEventKind.WrathCastStarted:
                        HandleWrathCastStarted(evt);
                        break;
                    case BattleEventKind.WrathImpactApplied:
                        HandleWrathImpactApplied(evt);
                        break;
                }
            }
        }

        public void ClearAll()
        {
            if (_vfxPool != null)
            {
                _vfxPool.ClearAll();
            }
        }

        private void HandleUnitDamaged(BattleEvent evt)
        {
            if (!evt.Position.HasValue) return;

            var prefab = evt.Side == ArmySide.Left ? _hitVfxLeftPrefab : _hitVfxRightPrefab;
            if (prefab == null) return;

            var pos = new Vector3(evt.Position.Value.X, 0.5f, evt.Position.Value.Z);
            var instance = _vfxPool.Get(prefab, pos, Quaternion.identity);
            _vfxPool.ReturnAfterDelay(prefab, instance, HitVfxReturnDelay);
        }

        private void HandleWrathCastStarted(BattleEvent evt)
        {
            if (!evt.Cast.HasValue || _telegraphPrefab == null) return;

            var cast = evt.Cast.Value;
            var pos = new Vector3(cast.Center.X, 0.01f, cast.Center.Z);
            var instance = _vfxPool.Get(_telegraphPrefab, pos, Quaternion.identity);

            var diameter = cast.Radius * 2f;
            instance.transform.localScale = new Vector3(diameter, 0.01f, diameter);

            var telegraphDuration = cast.ImpactTimeSec - cast.CastTimeSec;
            if (telegraphDuration < 0.1f) telegraphDuration = 0.5f;

            _vfxPool.ReturnAfterDelay(_telegraphPrefab, instance, telegraphDuration);
        }

        private void HandleWrathImpactApplied(BattleEvent evt)
        {
            if (!evt.Cast.HasValue || _lightningVfxPrefab == null) return;

            var cast = evt.Cast.Value;
            var pos = new Vector3(cast.Center.X, 0f, cast.Center.Z);
            var instance = _vfxPool.Get(_lightningVfxPrefab, pos, Quaternion.identity);
            _vfxPool.ReturnAfterDelay(_lightningVfxPrefab, instance, LightningVfxReturnDelay);
        }
    }
}
