using UnityEngine;
using UnityEngine.InputSystem;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Infrastructure.Config.Combat;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class WrathDragPresenter : MonoBehaviour
    {
        [SerializeField] private ScreenFlowPresenter _screenFlow;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _arenaLayer;

        private bool _isDragging;

        private void Update()
        {
            if (_screenFlow == null || _screenFlow.WrathVm == null)
            {
                return;
            }

            if (!_screenFlow.WrathVm.CanCast.CurrentValue)
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    _screenFlow.WrathVm.SetDragging(false);
                }

                return;
            }

            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                _isDragging = true;
                _screenFlow.WrathVm.SetDragging(true);
            }

            if (pointer.press.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                _screenFlow.WrathVm.SetDragging(false);

                if (TryGetArenaPoint(pointer.position.ReadValue(), out var point))
                {
                    _screenFlow.TryCastWrath(point);
                }
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
