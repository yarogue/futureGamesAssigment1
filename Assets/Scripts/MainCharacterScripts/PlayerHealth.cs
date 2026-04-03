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
            if (ServiceLocator.TryGetService<IGameplayManager>(out var gameplayManager))
            {
                gameplayManager.OnPlayerDied();
            }
            Destroy(gameObject);
        }
    }
}