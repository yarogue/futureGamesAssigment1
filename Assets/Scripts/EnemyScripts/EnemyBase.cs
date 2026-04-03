using System.Linq;
using UnityEngine;
using generalScripts;
using generalScripts.Managers;
using scriptableObjects;
using Random = UnityEngine.Random;

namespace EnemyScripts
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        [Header("Component References")]
        [SerializeField] protected Health health;

        protected Transform playerTransform;
        protected EnemyData enemyData;
        protected float lastAttackTime;

        protected float scaleDamage,
               scaledAttackCooldown,
                    scaledMoveSpeed,
                   scaledScoreValue;

        public virtual void Initialize(Transform playerTransform, EnemyData enemyData)
        {
            this.playerTransform = playerTransform;
            this.enemyData = enemyData;

            var statMultiplier = 1.0f;
            if (DifficultyManager.Instance != null)
            {
                statMultiplier = DifficultyManager.Instance.GetCurrentStatMultiplier();
            }

            var scaledHealth = enemyData.maxHealth * statMultiplier;

            scaleDamage = enemyData.damage * statMultiplier;
            scaledMoveSpeed = enemyData.moveSpeed * statMultiplier;
            scaledAttackCooldown = enemyData.attackCooldown / statMultiplier;
            scaledScoreValue = enemyData.scoreValue * statMultiplier;

            if (health != null)
            {
                health.onDie.RemoveAllListeners();
                health.SetMaxHealth(scaledHealth);
                health.onDie.AddListener(Die);
            }
        }
        protected virtual void Update() { }
        public void Die()
        {
            DropItem();

            var gameplayManager = ServiceLocator.TryGetService<generalScripts.Interfaces.IGameplayManager>(out var gpm) ? gpm : null;
            if (gameplayManager != null)
            {
                gameplayManager.AddScore((int)scaledScoreValue);
                gameplayManager.OnEnemyDestroyed(1);
            }

            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.EnemyKilled();
            }

            Destroy(gameObject);
        }
        

        private void DropItem()
        {
            if (Random.value < enemyData.overallNoDropChance)
            {
                return;
            }
            if (enemyData?.dropConfigurations == null || enemyData.dropConfigurations.Count == 0)
            {
                return;
            }
            var itemsDroppedCount = 0;
            var validDrops = enemyData.dropConfigurations
                .Where(config => config.dropWeight > 0 && config.itemToDropPrefab != null)
                .ToList();
            if (validDrops.Count == 0) return;
            var totalWeight = validDrops.Sum(config => config.dropWeight);
            if (totalWeight <= 0) return;
            var randomNumber = Random.Range(1, totalWeight + 1);
            var selectedDrop = new DropConfiguration();
            var currentWeight = 0;
            foreach (var config in validDrops)
            {
                currentWeight += config.dropWeight;

                if (randomNumber <= currentWeight)
                {
                    selectedDrop = config;
                    break;
                }
            }
            if (selectedDrop.itemToDropPrefab != null)
            {
                Instantiate(selectedDrop.itemToDropPrefab, transform.position, Quaternion.identity);
                Debug.Log($"Enemy dropped single item: {selectedDrop.itemToDropPrefab.name} (Weight: {selectedDrop.dropWeight})");
            }
        }
        
        
        
        
        
        
        
        
        
        
        private void DealDamageToPlayer(Collider2D other)
        {
            var playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(scaleDamage);
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                DealDamageToPlayer(other);
                lastAttackTime = Time.time;
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Time.time >= lastAttackTime + scaledAttackCooldown)
                {
                    DealDamageToPlayer(other);
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}
