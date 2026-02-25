using System;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class OnUnitKilledUseCase
    {
        private readonly ArmySide _ownerSide;
        private readonly WrathConfig _config;
        private readonly WrathService _wrathService;

        public OnUnitKilledUseCase(ArmySide ownerSide, WrathConfig config, WrathService wrathService)
        {
            _ownerSide = ownerSide;
            _config = config;
            _wrathService = wrathService ?? throw new ArgumentNullException(nameof(wrathService));
        }

        public WrathMeter OnUnitKilled(WrathMeter current, ArmySide killerSide, ArmySide victimSide)
        {
            if (killerSide != _ownerSide || victimSide == _ownerSide)
            {
                return current;
            }

            return _wrathService.AccumulateOnEnemyKill(current, _config.ChargePerKill);
        }
    }
}
