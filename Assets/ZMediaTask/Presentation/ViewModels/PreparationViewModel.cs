using System;
using R3;
using ZMediaTask.Application.Army;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Presentation.ViewModels
{
    public sealed class PreparationViewModel : IDisposable
    {
        private readonly ArmyRandomizationUseCase _randomizationUseCase;
        private readonly ReactiveProperty<ArmyPair> _armies = new();
        private readonly ReactiveProperty<string> _leftPreview = new("—");
        private readonly ReactiveProperty<string> _rightPreview = new("—");
        private readonly Subject<ArmyPair> _startRequested = new();
        private int _nextSeed;

        public PreparationViewModel(ArmyRandomizationUseCase randomizationUseCase)
        {
            _randomizationUseCase = randomizationUseCase
                ?? throw new ArgumentNullException(nameof(randomizationUseCase));
            _nextSeed = Environment.TickCount;
        }

        public ReadOnlyReactiveProperty<ArmyPair> Armies => _armies;
        public ReadOnlyReactiveProperty<string> LeftPreview => _leftPreview;
        public ReadOnlyReactiveProperty<string> RightPreview => _rightPreview;
        public Observable<ArmyPair> StartRequested => _startRequested;

        public void RandomizeLeft()
        {
            var pair = _randomizationUseCase.RandomizeBoth(_nextSeed++);
            UpdateArmies(new ArmyPair(pair.Left, CurrentRight ?? pair.Right));
        }

        public void RandomizeRight()
        {
            var pair = _randomizationUseCase.RandomizeBoth(_nextSeed++);
            UpdateArmies(new ArmyPair(CurrentLeft ?? pair.Left, pair.Right));
        }

        public void RandomizeBoth()
        {
            var pair = _randomizationUseCase.RandomizeBoth(_nextSeed++);
            UpdateArmies(pair);
        }

        public void RequestStart()
        {
            if (_armies.Value.Left == null || _armies.Value.Right == null)
            {
                RandomizeBoth();
            }

            _startRequested.OnNext(_armies.Value);
        }

        public void Dispose()
        {
            _armies.Dispose();
            _leftPreview.Dispose();
            _rightPreview.Dispose();
            _startRequested.Dispose();
        }

        private Army CurrentLeft => _armies.Value.Left;
        private Army CurrentRight => _armies.Value.Right;

        private void UpdateArmies(ArmyPair pair)
        {
            _armies.Value = pair;
            _leftPreview.Value = FormatArmy(pair.Left);
            _rightPreview.Value = FormatArmy(pair.Right);
        }

        private static string FormatArmy(Army army)
        {
            if (army == null) return "—";
            return $"{army.Units.Count} units ({GetSideLabel(army.Side)})";
        }

        private static string GetSideLabel(ArmySide side)
        {
            return side == ArmySide.Left ? "Blue" : "Red";
        }
    }
}
