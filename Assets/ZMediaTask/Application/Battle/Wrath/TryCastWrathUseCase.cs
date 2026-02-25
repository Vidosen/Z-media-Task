using System;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Application.Battle
{
    public sealed class TryCastWrathUseCase
    {
        private readonly ArmySide _ownerSide;
        private readonly WrathConfig _config;
        private readonly WrathService _wrathService;
        private readonly IWrathTargetValidator _targetValidator;

        public TryCastWrathUseCase(
            ArmySide ownerSide,
            WrathConfig config,
            WrathService wrathService,
            IWrathTargetValidator targetValidator)
        {
            _ownerSide = ownerSide;
            _config = config;
            _wrathService = wrathService ?? throw new ArgumentNullException(nameof(wrathService));
            _targetValidator = targetValidator ?? throw new ArgumentNullException(nameof(targetValidator));
        }

        public TryCastWrathResult TryCast(
            WrathMeter meter,
            ArmySide controllerSide,
            BattlePoint targetPoint,
            float currentTimeSec)
        {
            if (controllerSide != _ownerSide)
            {
                return new TryCastWrathResult(false, meter, null, WrathCastFailureReason.NotOwnerController);
            }

            if (!_wrathService.CanCast(meter))
            {
                return new TryCastWrathResult(false, meter, null, WrathCastFailureReason.MeterNotFull);
            }

            if (!_targetValidator.IsValid(targetPoint))
            {
                return new TryCastWrathResult(false, meter, null, WrathCastFailureReason.InvalidTarget);
            }

            var command = new WrathCastCommand(
                _ownerSide,
                targetPoint,
                _config.Radius,
                _config.Damage,
                currentTimeSec,
                currentTimeSec + _config.ImpactDelaySeconds);

            var consumed = _wrathService.Consume(meter);
            return new TryCastWrathResult(true, consumed, command, null);
        }
    }
}
