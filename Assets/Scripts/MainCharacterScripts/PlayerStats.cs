using System;
using UnityEngine;

namespace MainCharacterScripts
{

    [CreateAssetMenu(fileName = "New Player Stats", menuName = "Player/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Core Stats")] public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float boostSpeed = 10f;
        public float rotationSpeed = 100f;
        public int currentLevel;

        [Header("Combat Stats")] public float fireRate = 0.5f;
        public float projectileDamage = 10f;

        [Header("Missile Stats")] public int currentMissileAmount;
        public int maxMissileAmount;

        [Tooltip("Cooldown in seconds between missile shots (lower = faster reload)")]
        public float missileCooldownRate = 0.5f;

        [Tooltip("Radius and damage multiplier for missile area-of-effect")]
        public float missileAreaDamage = 5f;

        [Header("Defence Stats")]
        [Range(0f, 0.9f)]
        [Tooltip("Fraction of incoming damage blocked (0 = none, 0.9 = 90% reduction)")]
        public float damageResistance = 0f;

        private void OnEnable()
        {
            currentMissileAmount = maxMissileAmount;
        }

        public void RefillMissileAmmo(int amount)
        {
            currentMissileAmount = Mathf.Min(currentMissileAmount + amount, maxMissileAmount);
        }

        /// <summary>
        /// Resets all runtime-modified stats back to serialized defaults.
        /// Called by PlayerUpgradeManager.ResetStats() — do not call directly.
        /// </summary>
        public void ResetToDefaults(
            float baseMoveSpeed, float baseBoostSpeed,
            float baseFireRate, float baseDamage,
            int baseMaxMissiles, float baseMissileCooldown,
            float baseMissileAreaDamage, float baseDamageResistance)
        {
            moveSpeed = baseMoveSpeed;
            boostSpeed = baseBoostSpeed;
            fireRate = baseFireRate;
            projectileDamage = baseDamage;
            maxMissileAmount = baseMaxMissiles;
            currentMissileAmount = baseMaxMissiles;
            missileCooldownRate = baseMissileCooldown;
            missileAreaDamage = baseMissileAreaDamage;
            damageResistance = baseDamageResistance;
        }
    }
}