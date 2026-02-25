using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class ScreenFlowPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private BattleLoopRunnerPresenter _battleRunner;
        [SerializeField] private BattleUnitsPresenter _unitsPresenter;
        [SerializeField] private BattleVfxPresenter _vfxPresenter;

        private readonly ReactiveProperty<GameFlowState> _currentState = new(GameFlowState.MainMenu);
        private readonly CompositeDisposable _disposables = new();

        private MainMenuViewModel _mainMenuVm;
        private PreparationViewModel _preparationVm;
        private BattleHudViewModel _battleHudVm;
        private WrathViewModel _wrathVm;
        private ResultViewModel _resultVm;

        private VisualElement _mainMenuRoot;
        private VisualElement _preparationRoot;
        private VisualElement _battleHudRoot;
        private VisualElement _resultRoot;

        private ArmyRandomizationUseCase _randomizationUseCase;
        private BattleLoopService _battleLoopService;
        private BattleContextFactory _battleContextFactory;
        private TryCastWrathUseCase _tryCastWrathUseCase;

        public ReadOnlyReactiveProperty<GameFlowState> CurrentState => _currentState;
        public BattleHudViewModel BattleHudVm => _battleHudVm;
        public WrathViewModel WrathVm => _wrathVm;

        public void Construct(
            ArmyRandomizationUseCase randomizationUseCase,
            BattleLoopService battleLoopService,
            BattleContextFactory battleContextFactory,
            TryCastWrathUseCase tryCastWrathUseCase)
        {
            _randomizationUseCase = randomizationUseCase;
            _battleLoopService = battleLoopService;
            _battleContextFactory = battleContextFactory;
            _tryCastWrathUseCase = tryCastWrathUseCase;
        }

        private void Start()
        {
            var root = _uiDocument.rootVisualElement;

            _mainMenuRoot = root.Q<VisualElement>("MainMenuScreen");
            _preparationRoot = root.Q<VisualElement>("PreparationScreen");
            _battleHudRoot = root.Q<VisualElement>("BattleHudScreen");
            _resultRoot = root.Q<VisualElement>("ResultScreen");

            _mainMenuVm = new MainMenuViewModel();
            _preparationVm = new PreparationViewModel(_randomizationUseCase);
            _battleHudVm = new BattleHudViewModel();
            _wrathVm = new WrathViewModel();
            _resultVm = new ResultViewModel();

            BindMainMenu(root);
            BindPreparation(root);
            BindBattleHud(root);
            BindResult(root);

            _currentState.Subscribe(OnStateChanged).AddTo(_disposables);
            _currentState.Value = GameFlowState.MainMenu;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _mainMenuVm?.Dispose();
            _preparationVm?.Dispose();
            _battleHudVm?.Dispose();
            _wrathVm?.Dispose();
            _resultVm?.Dispose();
            _currentState?.Dispose();
        }

        private void BindMainMenu(VisualElement root)
        {
            var playButton = root.Q<Button>("PlayButton");
            if (playButton != null)
            {
                playButton.clicked += () => _mainMenuVm.RequestStart();
            }

            _mainMenuVm.StartRequested.Subscribe(_ =>
            {
                _currentState.Value = GameFlowState.Preparation;
            }).AddTo(_disposables);
        }

        private void BindPreparation(VisualElement root)
        {
            var randomLeftBtn = root.Q<Button>("RandomizeLeftButton");
            var randomRightBtn = root.Q<Button>("RandomizeRightButton");
            var randomBothBtn = root.Q<Button>("RandomizeBothButton");
            var startBattleBtn = root.Q<Button>("StartBattleButton");
            var leftPreviewLabel = root.Q<Label>("LeftArmyPreview");
            var rightPreviewLabel = root.Q<Label>("RightArmyPreview");

            if (randomLeftBtn != null) randomLeftBtn.clicked += () => _preparationVm.RandomizeLeft();
            if (randomRightBtn != null) randomRightBtn.clicked += () => _preparationVm.RandomizeRight();
            if (randomBothBtn != null) randomBothBtn.clicked += () => _preparationVm.RandomizeBoth();
            if (startBattleBtn != null) startBattleBtn.clicked += () => _preparationVm.RequestStart();

            if (leftPreviewLabel != null)
            {
                _preparationVm.LeftPreview.Subscribe(t => leftPreviewLabel.text = t).AddTo(_disposables);
            }

            if (rightPreviewLabel != null)
            {
                _preparationVm.RightPreview.Subscribe(t => rightPreviewLabel.text = t).AddTo(_disposables);
            }

            _preparationVm.Armies.Subscribe(pair =>
            {
                if (pair.Left != null && pair.Right != null && _unitsPresenter != null)
                {
                    var formation = FormationStrategyPicker.PickRandom(Environment.TickCount);
                    _battleContextFactory.SetFormationStrategy(formation);
                    var previewContext = _battleContextFactory.Create(pair);
                    _unitsPresenter.SpawnPreview(previewContext);
                }
            }).AddTo(_disposables);

            _preparationVm.StartRequested.Subscribe(armies =>
            {
                StartBattle(armies);
            }).AddTo(_disposables);
        }

        private void BindBattleHud(VisualElement root)
        {
            var leftAliveLabel = root.Q<Label>("LeftAliveLabel");
            var rightAliveLabel = root.Q<Label>("RightAliveLabel");
            var timerLabel = root.Q<Label>("TimerLabel");
            var wrathBar = root.Q<ProgressBar>("WrathProgressBar");

            if (leftAliveLabel != null)
            {
                _battleHudVm.LeftAlive.Subscribe(v => leftAliveLabel.text = v.ToString()).AddTo(_disposables);
            }

            if (rightAliveLabel != null)
            {
                _battleHudVm.RightAlive.Subscribe(v => rightAliveLabel.text = v.ToString()).AddTo(_disposables);
            }

            if (timerLabel != null)
            {
                _battleHudVm.TimerText.Subscribe(t => timerLabel.text = t).AddTo(_disposables);
            }

            if (wrathBar != null)
            {
                _wrathVm.ChargeNormalized.Subscribe(v =>
                {
                    wrathBar.value = v * 100f;
                    wrathBar.title = _wrathVm.CanCast.CurrentValue ? "WRATH READY!" : $"{(int)(v * 100)}%";
                }).AddTo(_disposables);
            }
        }

        private void BindResult(VisualElement root)
        {
            var winnerLabel = root.Q<Label>("WinnerLabel");
            var returnBtn = root.Q<Button>("ReturnButton");

            if (winnerLabel != null)
            {
                _resultVm.WinnerText.Subscribe(t => winnerLabel.text = t).AddTo(_disposables);
            }

            if (returnBtn != null)
            {
                returnBtn.clicked += () => _resultVm.RequestReturn();
            }

            _resultVm.ReturnRequested.Subscribe(_ =>
            {
                _currentState.Value = GameFlowState.MainMenu;
            }).AddTo(_disposables);
        }

        private void StartBattle(ArmyPair armies)
        {
            _battleLoopService.Initialize(armies);
            _battleLoopService.Start();
            _battleHudVm.UpdateFromContext(_battleLoopService.Context);

            if (_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
            {
                _wrathVm.UpdateFromMeter(meter);
            }

            if (_unitsPresenter != null)
            {
                _unitsPresenter.SpawnUnits(_battleLoopService.Context);
            }

            if (_battleRunner != null)
            {
                _battleRunner.StartRunning();
            }

            _currentState.Value = GameFlowState.Running;
        }

        public void OnBattleTick()
        {
            _battleHudVm.UpdateFromContext(_battleLoopService.Context);

            if (_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
            {
                _wrathVm.UpdateFromMeter(meter);
            }

            if (_vfxPresenter != null && _battleLoopService.LastTickEvents.Count > 0)
            {
                _vfxPresenter.ProcessEvents(_battleLoopService.LastTickEvents);
            }

            if (_unitsPresenter != null)
            {
                var events = _battleLoopService.LastTickEvents;
                for (var i = 0; i < events.Count; i++)
                {
                    var evt = events[i];
                    if (evt.Kind == BattleEventKind.UnitDamaged && evt.UnitId.HasValue)
                    {
                        _unitsPresenter.FlashUnit(evt.UnitId.Value);
                    }
                }

                _unitsPresenter.SyncPositions(_battleLoopService.Context);
            }

            if (_battleLoopService.StateMachine.Current == BattlePhase.Finished)
            {
                if (_battleRunner != null)
                {
                    _battleRunner.StopRunning();
                }

                _resultVm.SetWinner(_battleLoopService.Context.WinnerSide);
                _currentState.Value = GameFlowState.Result;
            }
        }

        public void TryCastWrath(BattlePoint targetPoint)
        {
            if (_battleLoopService.StateMachine.Current != BattlePhase.Running)
            {
                return;
            }

            if (!_battleLoopService.Context.WrathMeters.TryGetValue(ArmySide.Left, out var meter))
            {
                return;
            }

            var result = _tryCastWrathUseCase.TryCast(
                meter, ArmySide.Left, targetPoint, _battleLoopService.Context.ElapsedTimeSec);
            if (!result.Success)
            {
                return;
            }

            _battleLoopService.SetWrathMeter(ArmySide.Left, result.MeterAfter);
            _battleLoopService.EnqueueWrathCast(result.Command!.Value);
            _wrathVm.UpdateFromMeter(result.MeterAfter);
        }

        private void OnStateChanged(GameFlowState state)
        {
            SetScreenVisible(_mainMenuRoot, state == GameFlowState.MainMenu);
            SetScreenVisible(_preparationRoot, state == GameFlowState.Preparation);
            SetScreenVisible(_battleHudRoot, state == GameFlowState.Running);
            SetScreenVisible(_resultRoot, state == GameFlowState.Result);

            if (state == GameFlowState.Preparation)
            {
                _preparationVm.RandomizeBoth();
            }

            if (state == GameFlowState.MainMenu)
            {
                if (_battleLoopService.StateMachine.Current != BattlePhase.Preparation)
                {
                    _battleLoopService.Reset();
                }

                if (_unitsPresenter != null)
                {
                    _unitsPresenter.ClearUnits();
                }

                _vfxPresenter?.ClearAll();
            }
        }

        private static void SetScreenVisible(VisualElement element, bool visible)
        {
            if (element == null)
            {
                return;
            }

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
