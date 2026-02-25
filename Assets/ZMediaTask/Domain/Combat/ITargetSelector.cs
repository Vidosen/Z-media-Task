using System.Collections.Generic;

namespace ZMediaTask.Domain.Combat
{
    public interface ITargetSelector
    {
        int? SelectTarget(
            TargetableUnit self,
            IReadOnlyList<TargetableUnit> enemies,
            int? currentTargetId);
    }
}
