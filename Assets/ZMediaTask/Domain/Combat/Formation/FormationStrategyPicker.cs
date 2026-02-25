using System;

namespace ZMediaTask.Domain.Combat
{
    public static class FormationStrategyPicker
    {
        private static readonly FormationType[] NonLineTypes =
        {
            FormationType.Grid,
            FormationType.Wedge,
            FormationType.Staggered
        };

        public static IFormationStrategy PickRandom(int seed)
        {
            var index = Math.Abs(seed) % NonLineTypes.Length;
            return CreateStrategy(NonLineTypes[index]);
        }

        public static IFormationStrategy CreateStrategy(FormationType type)
        {
            return type switch
            {
                FormationType.Line => new LineFormationStrategy(),
                FormationType.Grid => new GridFormationStrategy(),
                FormationType.Wedge => new WedgeFormationStrategy(),
                FormationType.Staggered => new StaggeredFormationStrategy(),
                _ => new GridFormationStrategy()
            };
        }
    }
}
