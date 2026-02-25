using System;
using R3;
using UnityEngine.InputSystem;

namespace ZMediaTask.Presentation.Input
{
    /// <summary>
    /// Bridges Unity InputAction callbacks to R3 Observable streams.
    /// Usage: action.OnPerformedAsObservable().Subscribe(...).AddTo(disposables);
    /// </summary>
    public static class InputActionAdapter
    {
        public static Observable<Unit> OnPerformedAsObservable(this InputAction action)
        {
            var subject = new Subject<Unit>();
            action.performed += _ => subject.OnNext(Unit.Default);
            return subject;
        }

        public static Observable<Unit> OnStartedAsObservable(this InputAction action)
        {
            var subject = new Subject<Unit>();
            action.started += _ => subject.OnNext(Unit.Default);
            return subject;
        }

        public static Observable<Unit> OnCanceledAsObservable(this InputAction action)
        {
            var subject = new Subject<Unit>();
            action.canceled += _ => subject.OnNext(Unit.Default);
            return subject;
        }
    }
}
