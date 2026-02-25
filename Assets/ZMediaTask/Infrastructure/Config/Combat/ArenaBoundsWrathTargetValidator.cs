using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Infrastructure.Config.Combat
{
    public sealed class ArenaBoundsWrathTargetValidator : IWrathTargetValidator
    {
        private readonly ArenaBounds _bounds;

        public ArenaBoundsWrathTargetValidator(ArenaBounds bounds)
        {
            _bounds = bounds;
        }

        public bool IsValid(BattlePoint point)
        {
            return _bounds.Contains(point.X, point.Z);
        }
    }
}
