using UnityEngine;
using generalScripts;
using generalScripts.Interfaces;

namespace MainCharacterScripts
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField]
        private Health health;

        private void Awake()
        {
            if (health != null)
            {
                health.onDie.AddListener(Die);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.onDie.RemoveListener(Die);
            }
        }

        public void Die()
        {
            Debug.LogError("[PlayerHealth] ☠ Die() was called! Destroying player. " +
                           "If unexpected, check what called TakeDamage() or onDie.Invoke().");

            // Reset all runtime upgrades so stats are clean for the next session
            if (PlayerUpgradeManager.Instance != null)
            {
                PlayerUpgradeManager.Instance.ResetStats();
            }

            if (ServiceLocator.TryGetService<IGameplayManager>(out var gameplayManager))
            {
                gameplayManager.OnPlayerDied();
            }
            Destroy(gameObject);
        }
    }
}