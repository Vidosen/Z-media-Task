using System;
using System.Collections.Generic;
using ZMediaTask.Application.Army;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class BattleLoopService
    {
        private readonly BattleContextFactory _contextFactory;
        private readonly IBattleStepProcessor _stepProcessor;
        private readonly IUnitQueryInRadius _unitQueryInRadius;
        private readonly WrathService _wrathService;
        private readonly WrathConfig _wrathConfig;
        private readonly List<PendingWrathImpact> _pendingImpacts;
        private readonly List<BattleEvent> _lastTickEvents;
        private readonly List<BattleEvent> _queuedEvents;
        private BattleContext _context;

        public BattleLoopService(BattleContextFactory contextFactory, IBattleStepProcessor stepProcessor)
            : this(
                contextFactory,
                stepProcessor,
                new DistanceUnitQueryInRadius(),
                new WrathService(),
                new WrathConfig(chargePerKill: 20, maxCharge: 100, radius: 4f, damage: 80, impactDelaySeconds: 0.35f))
        {
        }

        public BattleLoopService(
            BattleContextFactory contextFactory,
            IBattleStepProcessor stepProcessor,
            IUnitQueryInRadius unitQueryInRadius,
            WrathService wrathService,
            WrathConfig wrathConfig)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _stepProcessor = stepProcessor ?? throw new ArgumentNullException(nameof(stepProcessor));
            _unitQueryInRadius = unitQueryInRadius ?? throw new ArgumentNullException(nameof(unitQueryInRadius));
            _wrathService = wrathService ?? throw new ArgumentNullException(nameof(wrathService));
            _wrathConfig = wrathConfig;
            _pendingImpacts = new List<PendingWrathImpact>();
            _lastTickEvents = new List<BattleEvent>();
            _queuedEvents = new List<BattleEvent>();

            StateMachine = new BattleStateMachine();
            _context = BattleContext.Empty;
        }

        public BattleStateMachine StateMachine { get; }

        public BattleContext Context => _context;

        public IReadOnlyList<BattleEvent> LastTickEvents => _lastTickEvents;

        public void Initialize(ArmyPair armies)
        {
            EnsurePreparationState();
            _pendingImpacts.Clear();
            _lastTickEvents.Clear();
            _queuedEvents.Clear();

            var initialized = _contextFactory.Create(armies);
            _context = new BattleContext(
                initialized.Units,
                initialized.ElapsedTimeSec,
                initialized.WinnerSide,
                CreateInitialWrathMeters());
        }

        public void Start()
        {
            StateMachine.Start();
        }

        public bool EnqueueWrathCast(WrathCastCommand command)
        {
            if (StateMachine.Current != BattlePhase.Running)
            {
                return false;
            }

            _pendingImpacts.Add(new PendingWrathImpact(command));
            _queuedEvents.Add(new BattleEvent(
                BattleEventKind.WrathCastStarted,
                command.CastTimeSec,
                unitId: null,
                side: command.CasterSide,
                cast: command,
                affectedCount: null));

            return true;
        }

        public void Tick(float deltaTimeSec, float currentTimeSec)
        {
            if (deltaTimeSec < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSec), "Delta time must be >= 0.");
            }

            _lastTickEvents.Clear();
            if (StateMachine.Current != BattlePhase.Running)
            {
                return;
            }

            if (_queuedEvents.Count > 0)
            {
                _lastTickEvents.AddRange(_queuedEvents);
                _queuedEvents.Clear();
            }

            var next = _stepProcessor.Step(new BattleStepInput(_context, deltaTimeSec, currentTimeSec));

            if (_stepProcessor is AutoBattleStepProcessor auto)
            {
                _lastTickEvents.AddRange(auto.LastStepEvents);
            }

            var unitsAfterStep = CopyUnits(next.Units);
            ApplyDueWrathImpacts(unitsAfterStep, currentTimeSec);
            var winner = GetWinner(unitsAfterStep);

            _context = new BattleContext(unitsAfterStep, next.ElapsedTimeSec, winner, next.WrathMeters);

            if (winner.HasValue || IsDraw(unitsAfterStep))
            {
                StateMachine.Finish();
            }
        }

        public void SetWrathMeter(ArmySide side, WrathMeter meter)
        {
            var updatedMeters = new Dictionary<ArmySide, WrathMeter>();
            foreach (var pair in _context.WrathMeters)
            {
                updatedMeters[pair.Key] = pair.Key == side ? meter : pair.Value;
            }

            _context = new BattleContext(
                _context.Units,
                _context.ElapsedTimeSec,
                _context.WinnerSide,
                updatedMeters);
        }

        public void Reset()
        {
            EnsurePreparationState();
            _pendingImpacts.Clear();
            _lastTickEvents.Clear();
            _queuedEvents.Clear();
            _context = BattleContext.Empty;
        }

        private void EnsurePreparationState()
        {
            if (StateMachine.Current == BattlePhase.Running)
            {
                StateMachine.Finish();
            }

            if (StateMachine.Current == BattlePhase.Finished)
            {
                StateMachine.Reset();
            }
        }

        private void ApplyDueWrathImpacts(BattleUnitRuntime[] units, float currentTimeSec)
        {
            for (var i = _pendingImpacts.Count - 1; i >= 0; i--)
            {
                var pending = _pendingImpacts[i];
                if (pending.Command.ImpactTimeSec > currentTimeSec)
                {
                    continue;
                }

                _pendingImpacts.RemoveAt(i);
                ApplyWrathImpact(units, pending.Command, currentTimeSec);
            }
        }

        private void ApplyWrathImpact(BattleUnitRuntime[] units, WrathCastCommand command, float currentTimeSec)
        {
            var affectedUnitIds = _unitQueryInRadius.Query(units, command.Center, command.Radius);
            _lastTickEvents.Add(new BattleEvent(
                BattleEventKind.WrathImpactApplied,
                currentTimeSec,
                unitId: null,
                side: command.CasterSide,
                cast: command,
                affectedCount: affectedUnitIds.Count));

            if (affectedUnitIds.Count == 0)
            {
                return;
            }

            var combatStates = new CombatUnitState[units.Length];
            var unitIndexById = new Dictionary<int, int>(units.Length);
            for (var i = 0; i < units.Length; i++)
            {
                combatStates[i] = units[i].Combat;
                unitIndexById[units[i].UnitId] = i;
            }

            var applyResult = _wrathService.ApplyAoe(combatStates, affectedUnitIds, command.Damage);
            for (var i = 0; i < units.Length; i++)
            {
                var updatedCombat = applyResult.UnitsAfter[i];
                var updatedMovement = WithAlive(units[i].Movement, updatedCombat.IsAlive);
                units[i] = units[i].WithCombat(updatedCombat).WithMovement(updatedMovement);
            }

            for (var i = 0; i < applyResult.KilledUnitIds.Count; i++)
            {
                var unitId = applyResult.KilledUnitIds[i];
                if (!unitIndexById.TryGetValue(unitId, out var index))
                {
                    continue;
                }

                _lastTickEvents.Add(new BattleEvent(
                    BattleEventKind.UnitKilled,
                    currentTimeSec,
                    unitId,
                    units[index].Side,
                    cast: null,
                    affectedCount: null));
            }
        }

        private Dictionary<ArmySide, WrathMeter> CreateInitialWrathMeters()
        {
            return new Dictionary<ArmySide, WrathMeter>
            {
                [ArmySide.Left] = new WrathMeter(0, _wrathConfig.MaxCharge),
                [ArmySide.Right] = new WrathMeter(0, _wrathConfig.MaxCharge)
            };
        }

        private static BattleUnitRuntime[] CopyUnits(IReadOnlyList<BattleUnitRuntime> units)
        {
            var copy = new BattleUnitRuntime[units.Count];
            for (var i = 0; i < units.Count; i++)
            {
                copy[i] = units[i];
            }

            return copy;
        }

        private static MovementAgentState WithAlive(MovementAgentState movement, bool isAlive)
        {
            return new MovementAgentState(
                movement.UnitId,
                isAlive,
                movement.Speed,
                movement.Position,
                movement.TargetId,
                movement.CurrentPath,
                movement.LastPathTargetPosition);
        }

        private static ArmySide? GetWinner(IReadOnlyList<BattleUnitRuntime> units)
        {
            var hasLeftAlive = false;
            var hasRightAlive = false;

            for (var i = 0; i < units.Count; i++)
            {
                if (!units[i].Combat.IsAlive)
                {
                    continue;
                }

                if (units[i].Side == ArmySide.Left)
                {
                    hasLeftAlive = true;
                }
                else
                {
                    hasRightAlive = true;
                }

                if (hasLeftAlive && hasRightAlive)
                {
                    return null;
                }
            }

            if (hasLeftAlive)
            {
                return ArmySide.Left;
            }

            return hasRightAlive ? ArmySide.Right : null;
        }

        private static bool IsDraw(IReadOnlyList<BattleUnitRuntime> units)
        {
            for (var i = 0; i < units.Count; i++)
            {
                if (units[i].Combat.IsAlive)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
