namespace ZMediaTask.Application.Battle
{
    public sealed class BattleStepProcessor : IBattleStepProcessor
    {
        public BattleContext Step(BattleStepInput input)
        {
            return new BattleContext(
                input.Context.Units,
                input.Context.ElapsedTimeSec + input.DeltaTimeSec,
                input.Context.WinnerSide,
                input.Context.WrathMeters);
        }
    }
}
