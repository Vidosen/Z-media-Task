using System;
using R3;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Presentation.ViewModels
{
    public sealed class ResultViewModel : IDisposable
    {
        private readonly ReactiveProperty<string> _winnerText = new("");
        private readonly Subject<Unit> _returnRequested = new();

        public ReadOnlyReactiveProperty<string> WinnerText => _winnerText;
        public Observable<Unit> ReturnRequested => _returnRequested;

        public void SetWinner(ArmySide? winner)
        {
            _winnerText.Value = winner.HasValue
                ? $"{GetSideLabel(winner.Value)} army wins!"
                : "Draw!";
        }

        public void RequestReturn()
        {
            _returnRequested.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _winnerText.Dispose();
            _returnRequested.Dispose();
        }

        private static string GetSideLabel(ArmySide side)
        {
            return side == ArmySide.Left ? "Blue" : "Red";
        }
    }
}
