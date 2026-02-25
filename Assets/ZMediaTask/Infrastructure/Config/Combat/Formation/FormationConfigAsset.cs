using UnityEngine;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Infrastructure.Config.Combat
{
    [CreateAssetMenu(
        fileName = "FormationConfig",
        menuName = "ZMediaTask/Config/Formation Config")]
    public sealed class FormationConfigAsset : ScriptableObject
    {
        [Header("Spawn")]
        [SerializeField] private float _spawnOffsetX = 8f;

        [Header("Formation")]
        [SerializeField] private FormationType _formationType = FormationType.Grid;

        [Header("Grid / Staggered")]
        [SerializeField] private int _columns = 5;
        [SerializeField] private float _rowSpacing = 1.5f;
        [SerializeField] private float _columnSpacing = 1.5f;

        [Header("Line")]
        [SerializeField] private float _lineSpacing = 1.5f;

        [Header("Wedge")]
        [SerializeField] private float _wedgeDepthSpacing = 1.2f;
        [SerializeField] private float _wedgeWidthSpacing = 1.0f;

        public float SpawnOffsetX => _spawnOffsetX;
        public FormationType FormationType => _formationType;

        public IFormationStrategy BuildFormationStrategy()
        {
            return _formationType switch
            {
                FormationType.Line => new LineFormationStrategy(_lineSpacing),
                FormationType.Grid => new GridFormationStrategy(_columns, _rowSpacing, _columnSpacing),
                FormationType.Wedge => new WedgeFormationStrategy(_wedgeDepthSpacing, _wedgeWidthSpacing),
                FormationType.Staggered => new StaggeredFormationStrategy(_columns, _rowSpacing, _columnSpacing),
                _ => new LineFormationStrategy(_lineSpacing)
            };
        }
    }
}
