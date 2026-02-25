using System;
using System.Collections.Generic;
using R3;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;

namespace ZMediaTask.Presentation.ViewModels
{
    public sealed class BattleHudViewModel : IDisposable
    {
        private readonly ReactiveProperty<int> _leftAlive = new(0);
        private readonly ReactiveProperty<int> _rightAlive = new(0);
        private readonly ReactiveProperty<string> _timerText = new("0:00");

        public ReadOnlyReactiveProperty<int> LeftAlive => _leftAlive;
        public ReadOnlyReactiveProperty<int> RightAlive => _rightAlive;
        public ReadOnlyReactiveProperty<string> TimerText => _timerText;

        public void UpdateFromContext(BattleContext context)
        {
            var left = 0;
            var right = 0;
            IReadOnlyList<BattleUnitRuntime> units = context.Units;

            for (var i = 0; i < units.Count; i++)
            {
                if (!units[i].Combat.IsAlive)
                {
                    continue;
                }

                if (units[i].Side == ArmySide.Left)
                {
                    left++;
                }
                else
                {
                    right++;
                }
            }

            _leftAlive.Value = left;
            _rightAlive.Value = right;

            var elapsed = context.ElapsedTimeSec;
            var minutes = (int)(elapsed / 60f);
            var seconds = (int)(elapsed % 60f);
            _timerText.Value = $"{minutes}:{seconds:D2}";
        }

        public void Dispose()
        {
            _leftAlive.Dispose();
            _rightAlive.Dispose();
            _timerText.Dispose();
        }
    }
}
