using generalScripts;
using generalScripts.Interfaces;
using UnityEngine;
using scriptableObjects;
using System;

namespace MainCharacterScripts
{
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Projectile Spawn Point")]
        [SerializeField]
        private Transform projectileSpawnPoint;

        [Header("Weapon Data")]
        [SerializeField]
        private WeaponData bulletData,
                           missileData;

        [Header("Player Data")]
        [SerializeField]
        private PlayerStats playerStats;

        private float _lastBulletShotTime;
        private float _lastMissileShotTime = -100f;

        private IInputManager inputManager;

        public static Action<int, int> OnMissileAmmoUpdated;
        public static Action<float> OnMissileCooldownStarted;

        private void Start()
        {
            if (playerStats == null)
            {
                enabled = false;
                return;
            }
            
            inputManager = ServiceLocator.GetService<IInputManager>();
            OnMissileAmmoUpdated?.Invoke(playerStats.currentMissileAmount, playerStats.maxMissileAmount);
        }

        private void Update()
        {
            if (inputManager == null) return;

            // LMB - Normal attack (bullets)
            if (inputManager.IsAttacking && CanFire(bulletData))
            {
                FireWeapon(bulletData);
            }

            // RMB - Special attack (missiles)
            if (inputManager.IsSpecialAttacking && CanFire(missileData))
            {
                FireWeapon(missileData);
            }
        }

        private bool CanFire(WeaponData weaponData)
        {
            if (weaponData == bulletData)
            {
                // fireRate = shots per second, convert to cooldown duration
                return Time.time - _lastBulletShotTime >= 1f / playerStats.fireRate;
            }
            else if (weaponData == missileData)
            {
                var cooldownReady = Time.time - _lastMissileShotTime >= playerStats.missileCooldownRate;
                var hasAmmo = playerStats.currentMissileAmount > 0;

                return cooldownReady && hasAmmo;
            }
            return false;
        }

        private void FireWeapon(WeaponData weaponData)
        {
            Instantiate(weaponData.projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            if (weaponData == bulletData)
            {
                _lastBulletShotTime = Time.time;
            }
            else if (weaponData == missileData)
            {
                _lastMissileShotTime = Time.time;
                playerStats.currentMissileAmount--;
                OnMissileAmmoUpdated?.Invoke(playerStats.currentMissileAmount, playerStats.maxMissileAmount);
                // Pass the live cooldown value so the UI slider reflects upgraded reload speed
                OnMissileCooldownStarted?.Invoke(playerStats.missileCooldownRate);
                if (playerStats.currentMissileAmount == 0)
                {
                    Debug.Log("Out of missiles");
                }
            }
        }

        public void AddAmmo(int amount)
        {
            if (playerStats == null)
            {
                return;
            }

            if (playerStats.currentMissileAmount >= playerStats.maxMissileAmount)
            {
                return;
            }
            playerStats.currentMissileAmount += amount;
            playerStats.currentMissileAmount = Mathf.Min(playerStats.currentMissileAmount, playerStats.maxMissileAmount);
            OnMissileAmmoUpdated?.Invoke(playerStats.currentMissileAmount, playerStats.maxMissileAmount);
        }
    }
}