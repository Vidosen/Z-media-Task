using UnityEngine;
using ZMediaTask.Application.Battle;
using ZMediaTask.Infrastructure.Config.Combat;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class BattleLoopRunnerPresenter : MonoBehaviour
    {
        [SerializeField] private ScreenFlowPresenter _screenFlow;
        [SerializeField] private CombatGameplayConfigAsset _combatConfig;

        private BattleLoopService _battleLoopService;
        private float _tickInterval;
        private float _accumulator;
        private float _elapsedTime;
        private bool _isRunning;

        public void Construct(BattleLoopService battleLoopService)
        {
            _battleLoopService = battleLoopService;
            _tickInterval = _combatConfig != null ? _combatConfig.FixedTickInterval : 0.02f;
        }

        public void StartRunning()
        {
            _accumulator = 0f;
            _elapsedTime = 0f;
            _isRunning = true;
        }

        public void StopRunning()
        {
            _isRunning = false;
        }

        private void Update()
        {
            if (!_isRunning || _battleLoopService == null)
            {
                return;
            }

            _accumulator += Time.deltaTime;

            while (_accumulator >= _tickInterval)
            {
                _accumulator -= _tickInterval;
                _elapsedTime += _tickInterval;
                _battleLoopService.Tick(_tickInterval, _elapsedTime);
                _screenFlow.OnBattleTick();

                if (!_isRunning)
                {
                    break;
                }
            }
        }
    }
}
