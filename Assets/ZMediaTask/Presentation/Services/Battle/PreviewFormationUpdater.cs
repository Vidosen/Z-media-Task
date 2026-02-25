using System;
using ZMediaTask.Application.Army;
using ZMediaTask.Application.Battle;
using ZMediaTask.Domain.Army;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Presentation.Services
{
    public sealed class PreviewFormationUpdater
    {
        private readonly BattleContextFactory _contextFactory;
        private IFormationStrategy _leftStrategy = new LineFormationStrategy();
        private IFormationStrategy _rightStrategy = new LineFormationStrategy();
        private Army _lastLeftArmy;
        private Army _lastRightArmy;

        public PreviewFormationUpdater(BattleContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Update(ArmyPair pair)
        {
            var leftChanged = !ReferenceEquals(_lastLeftArmy, pair.Left);
            var rightChanged = !ReferenceEquals(_lastRightArmy, pair.Right);
            var baseSeed = Environment.TickCount;

            if (leftChanged)
                _leftStrategy = FormationStrategyPicker.PickRandom(baseSeed + 1);

            if (rightChanged)
                _rightStrategy = FormationStrategyPicker.PickRandom(baseSeed + 2);

            _contextFactory.SetFormationStrategy(ArmySide.Left, _leftStrategy);
            _contextFactory.SetFormationStrategy(ArmySide.Right, _rightStrategy);

            _lastLeftArmy = pair.Left;
            _lastRightArmy = pair.Right;
        }
    }
}
