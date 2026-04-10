using UnityEngine;
using scriptableObjects;
using generalScripts;

namespace MainCharacterScripts
{
    public class ProjectileController : MonoBehaviour
    {
        [Header("Projectile Data")]
        [SerializeField] private WeaponData data;

        [SerializeField] private GameObject explosivePrefab;
        [SerializeField] private float areaDamageRadius;
        [SerializeField] private Rigidbody2D body;

        private bool _hasCollided;

        // ── Damage helpers ───────────────────────────────────────────────────

        /// <summary>Returns the live direct-hit damage, reflecting Damage upgrades.</summary>
        public float GetDamage()
        {
            var stats = PlayerUpgradeManager.GetPlayerStats();
            return stats != null ? stats.projectileDamage : data.projectileDamage;
        }

        /// <summary>Returns the live area-of-effect damage, reflecting MissileAreaDamage upgrades.</summary>
        private float GetAreaDamage()
        {
            var stats = PlayerUpgradeManager.GetPlayerStats();
            return stats != null ? stats.missileAreaDamage : data.projectileDamage;
        }

        // ────────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (body != null)
            {
                GetComponent<Rigidbody2D>().linearVelocity = transform.up * data.projectileSpeed;
            }
            Destroy(gameObject, data.projectileGetDestroyTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile"))
            {
                var enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(GetDamage());
                }

                if (data.isExplosive)
                {
                    ApplyAreaDamage();
                    Instantiate(explosivePrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }

        private void ApplyAreaDamage()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);

            foreach (var hit in colliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemyHealth = hit.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(GetAreaDamage());
                    }
                }
            }
        }
    }
}
