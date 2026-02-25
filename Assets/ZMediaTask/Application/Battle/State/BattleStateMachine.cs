namespace ZMediaTask.Application.Battle
{
    public sealed class BattleStateMachine
    {
        private readonly GenericStateMachine<BattlePhase> _stateMachine = new(
            BattlePhase.Preparation,
            IsValidTransition);

        public BattlePhase Current => _stateMachine.Current;

        public bool Start()
        {
            return _stateMachine.TryTransition(BattlePhase.Running);
        }

        public bool Finish()
        {
            return _stateMachine.TryTransition(BattlePhase.Finished);
        }

        public bool Reset()
        {
            return _stateMachine.TryTransition(BattlePhase.Preparation);
        }

        private static bool IsValidTransition(BattlePhase current, BattlePhase next)
        {
            return current == BattlePhase.Preparation && next == BattlePhase.Running
                || current == BattlePhase.Running && next == BattlePhase.Finished
                || current == BattlePhase.Finished && next == BattlePhase.Preparation;
        }
    }
}
