using R3;
using UnityEngine;

namespace ZMediaTask.Presentation.Input
{
    public interface IPointerDragInput
    {
        Observable<Vector2> DragStarted { get; }
        Observable<Vector2> DragEnded { get; }
    }
}
