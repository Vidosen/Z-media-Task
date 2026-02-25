using R3;
using UnityEngine;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Presentation.Input;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class WrathDragPresenter : MonoBehaviour
    {
        [SerializeField] private ScreenFlowPresenter _screenFlow;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _arenaLayer;
        [SerializeField] private PointerDragInputAdapter _pointerInput;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_pointerInput == null) return;

            _pointerInput.DragStarted.Subscribe(OnDragStarted).AddTo(_disposables);
            _pointerInput.DragEnded.Subscribe(OnDragEnded).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void OnDragStarted(Vector2 screenPos)
        {
            if (_screenFlow == null || _screenFlow.WrathVm == null) return;
            if (!_screenFlow.WrathVm.CanCast.CurrentValue) return;

            _screenFlow.WrathVm.SetDragging(true);
        }

        private void OnDragEnded(Vector2 screenPos)
        {
            if (_screenFlow == null || _screenFlow.WrathVm == null) return;

            var wasDragging = _screenFlow.WrathVm.IsDragging.CurrentValue;
            _screenFlow.WrathVm.SetDragging(false);

            if (!wasDragging) return;

            if (TryGetArenaPoint(screenPos, out var point))
            {
                _screenFlow.TryCastWrath(point);
            }
        }

        private bool TryGetArenaPoint(Vector2 screenPos, out BattlePoint point)
        {
            point = default;
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            if (cam == null)
            {
                return false;
            }

            var ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 200f, _arenaLayer))
            {
                point = new BattlePoint(hit.point.x, hit.point.z);
                return true;
            }

            // Fallback: intersect with Y=0 plane
            if (ray.direction.y != 0f)
            {
                var t = -ray.origin.y / ray.direction.y;
                if (t > 0f)
                {
                    var p = ray.origin + ray.direction * t;
                    point = new BattlePoint(p.x, p.z);
                    return true;
                }
            }

            return false;
        }
    }
}
