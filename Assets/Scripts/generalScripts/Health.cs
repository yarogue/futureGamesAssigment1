using UnityEngine;
using UnityEngine.Events;
using generalScripts.Interfaces;
using MainCharacterScripts;

namespace generalScripts
{
    public class Health : MonoBehaviour
    {
        [Header("Health Stats")]
        [SerializeField]
        private float maxHealth;
        [SerializeField]
        private float currentHealth;

        [Header("Events")]
        public UnityEvent onTakeDamage;
        public UnityEvent onDie;
        private void Awake()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
            UpdateHealthUI();
        }
        public void Heal(int amount)
        {
            if (currentHealth >= maxHealth) return;

            float startingHealth = currentHealth;

            currentHealth = Mathf.Min(currentHealth + (float)amount, maxHealth);

            float actualHealth = currentHealth - startingHealth;

            Debug.Log($"Player healed {amount:F1} HP. Current HP: {actualHealth:F1}");
            UpdateHealthUI();
        }

        public void TakeDamage(float damageAmount)
        {
            // Apply damage resistance if this is the player
            if (gameObject.CompareTag("Player") && PlayerUpgradeManager.Instance != null)
            {
                var stats = PlayerUpgradeManager.GetPlayerStats();
                if (stats != null && stats.damageResistance > 0f)
                {
                    damageAmount *= (1f - stats.damageResistance);
                }
            }

            currentHealth -= damageAmount;

            // Spawn damage number
            SpawnDamageNumber(damageAmount);

            onTakeDamage.Invoke();
            UpdateHealthUI();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void SpawnDamageNumber(float damage)
        {
            var go = new GameObject("DamageNumber");
            go.transform.position = transform.position + Vector3.up * 0.5f;
            var dn = go.AddComponent<DamageNumber>();

            // Red for player damage, yellow for enemy damage
            var color = gameObject.CompareTag("Player") ? Color.red : Color.yellow;
            dn.Initialize(damage, color);
        }
        private void Die()
        {
            onDie.Invoke();
        }
        private void UpdateHealthUI()
        {
            if (gameObject.CompareTag("Player"))
            {
                if (ServiceLocator.TryGetService<IGameplayManager>(out var gameplayManager))
                {
                    gameplayManager.UpdateHealth(currentHealth);
                }
            }
        }

        public void RefreshHealthUI()
        {
            UpdateHealthUI();
        }
    }
}