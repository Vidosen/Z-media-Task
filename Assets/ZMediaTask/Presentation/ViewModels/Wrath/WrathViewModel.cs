using System;
using R3;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Presentation.ViewModels
{
    public sealed class WrathViewModel : IDisposable
    {
        private readonly ReactiveProperty<float> _chargeNormalized = new(0f);
        private readonly ReactiveProperty<bool> _canCast = new(false);
        private readonly ReactiveProperty<bool> _isDragging = new(false);
        private readonly ReactiveProperty<bool> _isTargeting = new(false);

        public ReadOnlyReactiveProperty<float> ChargeNormalized => _chargeNormalized;
        public ReadOnlyReactiveProperty<bool> CanCast => _canCast;
        public ReadOnlyReactiveProperty<bool> IsDragging => _isDragging;
        public ReadOnlyReactiveProperty<bool> IsTargeting => _isTargeting;

        public void UpdateFromMeter(WrathMeter meter)
        {
            _chargeNormalized.Value = meter.Normalized;
            _canCast.Value = meter.CanCast;
        }

        public void SetDragging(bool dragging)
        {
            _isDragging.Value = dragging;
        }

        public void SetTargeting(bool targeting)
        {
            _isTargeting.Value = targeting;
        }

        public void Dispose()
        {
            _chargeNormalized.Dispose();
            _canCast.Dispose();
            _isDragging.Dispose();
            _isTargeting.Dispose();
        }
    }
}
