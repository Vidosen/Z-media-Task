using System;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class BattleSessionController
    {
        private readonly BattleLoopService _battleLoopService;
        private readonly TryCastWrathUseCase _tryCastWrathUseCase;
        private readonly BattleHudViewModel _battleHudVm;
        private readonly WrathViewModel _wrathVm;
        private readonly ResultViewModel _resultVm;
        private readonly BattleUnitsPresenter _unitsPresenter;
        private readonly BattleVfxPresenter _vfxPresenter;
        private readonly Action _onBattleFinished;

        public BattleSessionController(
            BattleLoopService battleLoopService,
            TryCastWrathUseCase tryCastWrathUseCase,
            BattleHudViewModel battleHudVm,
            WrathViewModel wrathVm,
            ResultViewModel resultVm,
            BattleUnitsPresenter unitsPresenter,
            BattleVfxPresenter vfxPresenter,
            Action onBattleFinished)
        {
            _battleLoopService = battleLoopService;
            _tryCastWrathUseCase = tryCastWrathUseCase;
            _battleHudVm = battleHudVm;
            _wrathVm = wrathVm;
            _resultVm = resultVm;
            _unitsPresenter = unitsPresenter;
            _vfxPresenter = vfxPresenter;
            _onBattleFinished = onBattleFinished;
        }

        public void StartBattle(ArmyPair armies)
        {
            _battleLoopService.Initialize(armies);
            _battleLoopService.Start();
            _battleHudVm.UpdateFromContext(_battleLoopService.Context);

            if (_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
                _wrathVm.UpdateFromMeter(meter);

            if (_unitsPresenter)
                _unitsPresenter.SpawnUnits(_battleLoopService.Context);
        }

        /// <returns>true when the battle has finished this tick</returns>
        public bool ProcessTick()
        {
            _battleHudVm.UpdateFromContext(_battleLoopService.Context);

            if (_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
                _wrathVm.UpdateFromMeter(meter);

            if (_vfxPresenter && _battleLoopService.LastTickEvents.Count > 0)
                _vfxPresenter.ProcessEvents(_battleLoopService.LastTickEvents);

            if (_unitsPresenter)
            {
                var events = _battleLoopService.LastTickEvents;
                for (var i = 0; i < events.Count; i++)
                {
                    var evt = events[i];
                    if (evt is { Kind: BattleEventKind.UnitDamaged, UnitId: not null })
                        _unitsPresenter.FlashUnit(evt.UnitId.Value);
                }

                _unitsPresenter.SyncPositions(_battleLoopService.Context);
            }

            if (_battleLoopService.StateMachine.Current != BattlePhase.Finished)
                return false;

            _resultVm.SetWinner(_battleLoopService.Context.WinnerSide);
            _onBattleFinished?.Invoke();
            return true;
        }

        public void TryCastWrath(BattlePoint targetPoint)
        {
            if (_battleLoopService.StateMachine.Current != BattlePhase.Running)
                return;

            if (!_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
                return;

            var result = _tryCastWrathUseCase.TryCast(
                meter, ArmySide.Left, targetPoint, _battleLoopService.Context.ElapsedTimeSec);
            if (!result.Success)
                return;

            _battleLoopService.SetWrathMeter(ArmySide.Left, result.MeterAfter);
            _battleLoopService.EnqueueWrathCast(result.Command!.Value);
            _wrathVm.UpdateFromMeter(result.MeterAfter);
        }

        public void Cleanup()
        {
            if (_battleLoopService.StateMachine.Current != BattlePhase.Preparation)
                _battleLoopService.Reset();

            if (_unitsPresenter)
                _unitsPresenter.ClearUnits();

            _vfxPresenter?.ClearAll();
        }
    }
}
