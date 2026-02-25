namespace ZMediaTask.Application.Battle
{
    public interface IBattleStepProcessor
    {
        BattleContext Step(BattleStepInput input);
    }
}
