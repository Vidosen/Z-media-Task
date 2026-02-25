using R3;
using UnityEngine;
using UnityEngine.UIElements;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Presentation.Services;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class ScreenFlowPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private BattleLoopRunnerPresenter _battleRunner;
        [SerializeField] private BattleUnitsPresenter _unitsPresenter;
        [SerializeField] private BattleVfxPresenter _vfxPresenter;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _arenaLayer;
        [SerializeField] private WrathTargetingPresenter _wrathTargeting;

        private readonly ReactiveProperty<GameFlowState> _currentState = new(GameFlowState.MainMenu);
        private readonly CompositeDisposable _disposables = new();

        private MainMenuViewModel _mainMenuVm;
        private PreparationViewModel _preparationVm;
        private BattleHudViewModel _battleHudVm;
        private WrathViewModel _wrathVm;
        private ResultViewModel _resultVm;

        private WrathCardPresenter _wrathCardPresenter;
        private SafeAreaController _safeArea;
        private BattleSessionController _battleSession;
        private PreviewFormationUpdater _formationUpdater;

        private VisualElement _uiRoot;
        private VisualElement _mainMenuRoot;
        private VisualElement _preparationRoot;
        private VisualElement _battleHudRoot;
        private VisualElement _resultRoot;

        private ArmyRandomizationUseCase _randomizationUseCase;
        private BattleLoopService _battleLoopService;
        private BattleContextFactory _battleContextFactory;
        private TryCastWrathUseCase _tryCastWrathUseCase;

        public ReadOnlyReactiveProperty<GameFlowState> CurrentState => _currentState;

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
            _uiRoot = _uiDocument.rootVisualElement;
            var safeAreaRoot = _uiRoot.Q<VisualElement>("SafeAreaRoot") ?? _uiRoot;
            _safeArea = new SafeAreaController(_uiRoot, safeAreaRoot);

            var root = _uiRoot;

            _mainMenuRoot = root.Q<VisualElement>("MainMenuScreen");
            _preparationRoot = root.Q<VisualElement>("PreparationScreen");
            _battleHudRoot = root.Q<VisualElement>("BattleHudScreen");
            _resultRoot = root.Q<VisualElement>("ResultScreen");

            _mainMenuVm = new MainMenuViewModel();
            _preparationVm = new PreparationViewModel(_randomizationUseCase);
            _battleHudVm = new BattleHudViewModel();
            _wrathVm = new WrathViewModel();
            _resultVm = new ResultViewModel();

            _formationUpdater = new PreviewFormationUpdater(_battleContextFactory);
            _battleSession = new BattleSessionController(
                _battleLoopService, _tryCastWrathUseCase,
                _battleHudVm, _wrathVm, _resultVm,
                _unitsPresenter, _vfxPresenter,
                onBattleFinished: () => _currentState.Value = GameFlowState.Result);

            if (_battleRunner)
                _battleRunner.Construct(_battleLoopService, _battleSession);

            BindMainMenu(root);
            BindPreparation(root);
            BindBattleHud(root);
            BindResult(root);

            _currentState.Subscribe(OnStateChanged).AddTo(_disposables);
            _currentState.Value = GameFlowState.MainMenu;
        }

        private void Update()
        {
            _safeArea?.Refresh();
        }

        private void OnDestroy()
        {
            _safeArea?.Dispose();
            _wrathCardPresenter?.Dispose();
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
                playButton.clicked += () => _mainMenuVm.RequestStart();

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
                _preparationVm.LeftPreview.Subscribe(t => leftPreviewLabel.text = t).AddTo(_disposables);

            if (rightPreviewLabel != null)
                _preparationVm.RightPreview.Subscribe(t => rightPreviewLabel.text = t).AddTo(_disposables);

            _preparationVm.Armies.Subscribe(pair =>
            {
                if (pair is { Left: not null, Right: not null } && _unitsPresenter)
                {
                    _formationUpdater.Update(pair);
                    var previewContext = _battleContextFactory.Create(pair);
                    _unitsPresenter.SpawnPreview(previewContext);
                }
            }).AddTo(_disposables);

            _preparationVm.StartRequested.Subscribe(armies =>
            {
                _battleSession.StartBattle(armies);
                if (_battleRunner) _battleRunner.StartRunning();
                _currentState.Value = GameFlowState.Running;
            }).AddTo(_disposables);
        }

        private void BindBattleHud(VisualElement root)
        {
            var leftAliveLabel = root.Q<Label>("LeftAliveLabel");
            var rightAliveLabel = root.Q<Label>("RightAliveLabel");
            var timerLabel = root.Q<Label>("TimerLabel");

            if (leftAliveLabel != null)
                _battleHudVm.LeftAlive.Subscribe(v => leftAliveLabel.text = v.ToString()).AddTo(_disposables);

            if (rightAliveLabel != null)
                _battleHudVm.RightAlive.Subscribe(v => rightAliveLabel.text = v.ToString()).AddTo(_disposables);

            if (timerLabel != null)
                _battleHudVm.TimerText.Subscribe(t => timerLabel.text = t).AddTo(_disposables);

            _wrathCardPresenter = new WrathCardPresenter(
                root, _wrathVm, _mainCamera, _arenaLayer,
                _battleSession.TryCastWrath, _wrathTargeting);

            var bopLeftFill = root.Q<VisualElement>("BopLeftFill");
            if (bopLeftFill != null)
            {
                _battleHudVm.BalanceRatio.Subscribe(ratio =>
                {
                    bopLeftFill.style.width = new StyleLength(new Length(ratio * 100f, LengthUnit.Percent));
                }).AddTo(_disposables);
            }
        }

        private void BindResult(VisualElement root)
        {
            var winnerLabel = root.Q<Label>("WinnerLabel");
            var returnBtn = root.Q<Button>("ReturnButton");

            if (winnerLabel != null)
                _resultVm.WinnerText.Subscribe(t => winnerLabel.text = t).AddTo(_disposables);

            if (returnBtn != null)
                returnBtn.clicked += () => _resultVm.RequestReturn();

            _resultVm.ReturnRequested.Subscribe(_ =>
            {
                _currentState.Value = GameFlowState.MainMenu;
            }).AddTo(_disposables);
        }

        private void OnStateChanged(GameFlowState state)
        {
            SetScreenVisible(_mainMenuRoot, state == GameFlowState.MainMenu);
            SetScreenVisible(_preparationRoot, state == GameFlowState.Preparation);
            SetScreenVisible(_battleHudRoot, state == GameFlowState.Running);
            SetScreenVisible(_resultRoot, state == GameFlowState.Result);

            if (state == GameFlowState.Preparation)
                _preparationVm.RandomizeBoth();

            if (state == GameFlowState.MainMenu)
                _battleSession.Cleanup();
        }

        private static void SetScreenVisible(VisualElement element, bool visible)
        {
            if (element == null) return;
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
