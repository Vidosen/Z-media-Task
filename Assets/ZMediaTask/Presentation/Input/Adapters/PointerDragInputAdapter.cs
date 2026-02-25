using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZMediaTask.Presentation.Input
{
    public sealed class PointerDragInputAdapter : MonoBehaviour, IPointerDragInput
    {
        private readonly Subject<Vector2> _dragStarted = new();
        private readonly Subject<Vector2> _dragEnded = new();
        private bool _isDragging;

        public Observable<Vector2> DragStarted => _dragStarted;
        public Observable<Vector2> DragEnded => _dragEnded;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            var screenPos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                _isDragging = true;
                _dragStarted.OnNext(screenPos);
            }

            if (pointer.press.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                _dragEnded.OnNext(screenPos);
            }
        }

        private void OnDestroy()
        {
            _dragStarted.Dispose();
            _dragEnded.Dispose();
        }
    }
}
