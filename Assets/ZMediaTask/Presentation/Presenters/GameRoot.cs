using Reflex.Attributes;
using UnityEngine;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private ScreenFlowPresenter _screenFlow;
        [SerializeField] private BattleLoopRunnerPresenter _battleRunner;
        [SerializeField] private BattleVfxPresenter _vfxPresenter;

        [Inject] private ArmyRandomizationUseCase _randomizationUseCase;
        [Inject] private BattleLoopService _battleLoopService;
        [Inject] private BattleContextFactory _battleContextFactory;
        [Inject] private TryCastWrathUseCase _tryCastWrathUseCase;

        private void Start()
        {
            _screenFlow.Construct(
                _randomizationUseCase,
                _battleLoopService,
                _battleContextFactory,
                _tryCastWrathUseCase);

            _battleRunner.Construct(_battleLoopService);

            if (_vfxPresenter != null)
            {
                _vfxPresenter.Initialize();
            }
        }
    }
}
