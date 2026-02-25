using System;
using R3;

namespace ZMediaTask.Presentation.ViewModels
{
    public sealed class MainMenuViewModel : IDisposable
    {
        private readonly Subject<Unit> _startRequested = new();

        public Observable<Unit> StartRequested => _startRequested;

        public void RequestStart()
        {
            _startRequested.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _startRequested.Dispose();
        }
    }
}
