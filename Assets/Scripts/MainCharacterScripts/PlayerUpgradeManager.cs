using System;
using System.Collections.Generic;
using scriptableObjects;
using UnityEngine;

namespace MainCharacterScripts
{
    /// <summary>
    /// Manages runtime stat upgrades chosen on level-up.
    /// - Snapshots all relevant PlayerStats fields at game start.
    /// - Applies percentage upgrades to live stats (compounding).
    /// - Resets stats back to base on death / restart via PlayerStats.ResetToDefaults().
    ///
    /// FUTURE: Uncomment _appliedCounts to track how many times each upgrade was stacked.
    /// </summary>
    public class PlayerUpgradeManager : MonoBehaviour
    {
        public static PlayerUpgradeManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private UpgradeLibrary upgradeLibrary;

        // ── Base stat snapshot (captured once at game start) ─────────────────
        private float _baseMoveSpeed;
        private float _baseBoostSpeed;
        private float _baseFireRate;
        private float _baseDamage;
        private int   _baseMaxMissiles;
        private float _baseMissileCooldown;
        private float _baseMissileAreaDamage;
        private float _baseDamageResistance;

        // ── Update guard ─────────────────────────────────────────────────────
        // True during gameplay; False while dead / before game starts.
        private bool _updatesEnabled = false;

        // ── Future stack tracking ────────────────────────────────────────────
        // Uncomment when you want to count & display how many times each
        // upgrade has been stacked (e.g. "Speed +15% ×3"):
        // private Dictionary<UpgradeDefinition, int> _appliedCounts = new();

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Static helper so other systems (e.g. Health) can read PlayerStats
        /// without needing a direct SerializeField reference.
        /// </summary>
        public static PlayerStats GetPlayerStats()
        {
            return Instance != null ? Instance.playerStats : null;
        }

        private void Start()
        {
            SnapshotBaseStats();
            _updatesEnabled = true;
        }

        // ── Snapshot / Reset ─────────────────────────────────────────────────

        private void SnapshotBaseStats()
        {
            if (playerStats == null)
            {
                Debug.LogWarning("[PlayerUpgradeManager] PlayerStats reference is missing!");
                return;
            }

            _baseMoveSpeed          = playerStats.moveSpeed;
            _baseBoostSpeed         = playerStats.boostSpeed;
            _baseFireRate           = playerStats.fireRate;
            _baseDamage             = playerStats.projectileDamage;
            _baseMaxMissiles        = playerStats.maxMissileAmount;
            _baseMissileCooldown    = playerStats.missileCooldownRate;
            _baseMissileAreaDamage  = playerStats.missileAreaDamage;
            _baseDamageResistance   = playerStats.damageResistance;

            Debug.Log("[PlayerUpgradeManager] Base stats snapshot taken.");
        }

        /// <summary>
        /// Call this on player death / game restart.
        /// Restores PlayerStats to the values captured at game start.
        /// </summary>
        public void ResetStats()
        {
            _updatesEnabled = false;

            if (playerStats == null) return;

            playerStats.ResetToDefaults(
                _baseMoveSpeed,
                _baseBoostSpeed,
                _baseFireRate,
                _baseDamage,
                _baseMaxMissiles,
                _baseMissileCooldown,
                _baseMissileAreaDamage,
                _baseDamageResistance
            );

            // _appliedCounts.Clear(); // ← uncomment when tracking stacks

            Debug.Log("[PlayerUpgradeManager] All stats reset to base values.");
        }

        /// <summary>
        /// Re-enables upgrades after a reset (call at the start of a new session if needed).
        /// </summary>
        public void EnableUpgrades()
        {
            SnapshotBaseStats();
            _updatesEnabled = true;
        }

        // ── Upgrade Picking ──────────────────────────────────────────────────

        /// <summary>
        /// Returns 'count' unique random upgrades from the library (no duplicates per draw).
        /// If the library has fewer entries than 'count', returns all of them.
        /// </summary>
        public List<UpgradeDefinition> GetRandomUpgrades(int count)
        {
            var result = new List<UpgradeDefinition>();

            if (upgradeLibrary == null || upgradeLibrary.allUpgrades == null ||
                upgradeLibrary.allUpgrades.Count == 0)
            {
                Debug.LogWarning("[PlayerUpgradeManager] UpgradeLibrary is empty or not assigned!");
                return result;
            }

            // Shallow copy so we can remove without touching the SO asset
            var pool = new List<UpgradeDefinition>(upgradeLibrary.allUpgrades);
            count = Mathf.Min(count, pool.Count);

            for (int i = 0; i < count; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            return result;
        }

        // ── Apply ────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies an upgrade to the live PlayerStats (compounding — always based on current value).
        /// </summary>
        public void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            if (!_updatesEnabled)
            {
                Debug.LogWarning("[PlayerUpgradeManager] Tried to apply upgrade while disabled.");
                return;
            }

            if (upgrade == null || playerStats == null) return;

            float mult = 1f + upgrade.percentageIncrease;

            switch (upgrade.statType)
            {
                case StatType.MoveSpeed:
                    playerStats.moveSpeed *= mult;
                    break;

                case StatType.FireRate:
                    // fireRate = shots per second — multiply to fire faster
                    playerStats.fireRate *= mult;
                    break;

                case StatType.Damage:
                    playerStats.projectileDamage *= mult;
                    break;

                case StatType.DamageResistance:
                    // Resistance is capped at 0.9 (90%) to prevent invincibility
                    // Formula: newResist = 1 - (1 - current) * (1 - increase)
                    // This stacks multiplicatively so it never reaches 1.0
                    playerStats.damageResistance =
                        1f - (1f - playerStats.damageResistance) * (1f - upgrade.percentageIncrease);
                    playerStats.damageResistance =
                        Mathf.Min(playerStats.damageResistance, 0.9f);
                    break;

                case StatType.MissileAmmo:
                    // Flat +1 per pick — keeps missile count as a clean integer
                    playerStats.maxMissileAmount += 1;
                    playerStats.currentMissileAmount = playerStats.maxMissileAmount;
                    // Notify HUD immediately
                    PlayerShooting.OnMissileAmmoUpdated?.Invoke(
                        playerStats.currentMissileAmount, playerStats.maxMissileAmount);
                    break;

                case StatType.MissileReloadSpeed:
                    // missileCooldownRate is a duration (lower = faster reload), so we divide
                    playerStats.missileCooldownRate /= mult;
                    break;

                case StatType.MissileAreaDamage:
                    playerStats.missileAreaDamage *= mult;
                    break;
            }

            // _appliedCounts.TryAdd(upgrade, 0); // ← future stack tracking
            // _appliedCounts[upgrade]++;

            Debug.Log($"[PlayerUpgradeManager] Applied '{upgrade.upgradeName}'" +
                      $" to {upgrade.statType}. New value: {GetCurrentValueFor(upgrade.statType):F2}");
        }

        // Helper for debug logging
        private float GetCurrentValueFor(StatType stat)
        {
            if (playerStats == null) return 0f;
            return stat switch
            {
                StatType.MoveSpeed          => playerStats.moveSpeed,
                StatType.FireRate           => playerStats.fireRate,
                StatType.Damage             => playerStats.projectileDamage,
                StatType.DamageResistance   => playerStats.damageResistance,
                StatType.MissileAmmo        => playerStats.maxMissileAmount,
                StatType.MissileReloadSpeed => playerStats.missileCooldownRate,
                StatType.MissileAreaDamage  => playerStats.missileAreaDamage,
                _                           => 0f
            };
        }
    }
}
