using UnityEngine;

namespace scriptableObjects
{
    public enum StatType
    {
        MoveSpeed,          // movement speed
        FireRate,           // bullet fire rate (lower interval = faster)
        Damage,             // bullet / projectile damage
        DamageResistance,   // % of incoming damage blocked (0–1 range)
        MissileAmmo,        // max missile capacity
        MissileReloadSpeed, // missile cooldown (lower = faster reload)
        MissileAreaDamage   // area-of-effect damage on missile impact
    }
    
    [CreateAssetMenu(fileName = "New Upgrade", menuName = "Player/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Display")]
        public string upgradeName = "Upgrade";

        [TextArea(2, 4)]
        public string description = "Increases something by X%.";

        [Header("Effect")]
        public StatType statType;

        [Range(0.05f, 1f)]
        [Tooltip("How much to increase the stat, e.g. 0.15 = +15%")]
        public float percentageIncrease = 0.15f;
    }
}
