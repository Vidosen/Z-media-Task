using DG.Tweening;
using UnityEngine;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class WrathTargetingPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _targetingCirclePrefab;
        [SerializeField] private VfxPool _vfxPool;
        [SerializeField] private float _radius = 4f;

        private GameObject _activeIndicator;
        private Tween _pulseTween;

        public void Show(BattlePoint point)
        {
            if (_targetingCirclePrefab == null || _vfxPool == null) return;

            if (_activeIndicator == null || !_activeIndicator.activeSelf)
            {
                _activeIndicator = _vfxPool.Get(
                    _targetingCirclePrefab,
                    new Vector3(point.X, 0.05f, point.Z),
                    Quaternion.identity);

                var diameter = _radius * 2f;
                _activeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);

                _pulseTween?.Kill();
                var t = _activeIndicator.transform;
                _pulseTween = t.DOScale(
                        new Vector3(diameter * 1.08f, 0.01f, diameter * 1.08f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }
            else
            {
                _activeIndicator.transform.position = new Vector3(point.X, 0.05f, point.Z);
            }
        }

        public void Hide()
        {
            _pulseTween?.Kill();
            _pulseTween = null;

            if (_activeIndicator != null && _activeIndicator.activeSelf)
            {
                _activeIndicator.SetActive(false);
                _activeIndicator = null;
            }
        }
    }
}
