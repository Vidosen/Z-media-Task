using UnityEngine;
using ZMediaTask.Domain.Combat;

namespace ZMediaTask.Infrastructure.Config.Combat
{
    [CreateAssetMenu(
        fileName = "CombatGameplayConfig",
        menuName = "ZMediaTask/Config/Combat Gameplay Config")]
    public sealed class CombatGameplayConfigAsset : ScriptableObject
    {
        [Header("Tick")]
        [SerializeField] private float _fixedTickInterval = 0.02f;

        [Header("Movement")]
        [SerializeField] private float _meleeRange = 1.5f;
        [SerializeField] private float _repathDistanceThreshold = 2f;
        [SerializeField] private float _steeringRadius = 1.2f;
        [SerializeField] private float _slotRadius = 1f;

        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _baseAttackDelay = 1f;

        [Header("Wrath")]
        [SerializeField] private int _chargePerKill = 20;
        [SerializeField] private int _maxCharge = 100;
        [SerializeField] private float _wrathRadius = 4f;
        [SerializeField] private int _wrathDamage = 80;
        [SerializeField] private float _impactDelaySeconds = 0.35f;

        [Header("Arena")]
        [SerializeField] private float _arenaMinX = -15f;
        [SerializeField] private float _arenaMaxX = 15f;
        [SerializeField] private float _arenaMinZ = -20f;
        [SerializeField] private float _arenaMaxZ = 20f;

        public float FixedTickInterval => _fixedTickInterval;

        public MovementConfig BuildMovementConfig()
        {
            return new MovementConfig(_meleeRange, _repathDistanceThreshold, _steeringRadius, _slotRadius);
        }

        public AttackConfig BuildAttackConfig()
        {
            return new AttackConfig(_attackRange, _baseAttackDelay);
        }

        public WrathConfig BuildWrathConfig()
        {
            return new WrathConfig(_chargePerKill, _maxCharge, _wrathRadius, _wrathDamage, _impactDelaySeconds);
        }

        public ArenaBounds BuildArenaBounds()
        {
            return new ArenaBounds(_arenaMinX, _arenaMaxX, _arenaMinZ, _arenaMaxZ);
        }
    }
}
