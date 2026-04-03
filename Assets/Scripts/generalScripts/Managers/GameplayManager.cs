using generalScripts.Interfaces;
using UnityEngine;

namespace generalScripts.Managers
{
    public class GameplayManager : MonoBehaviour, IGameplayManager
    {
        public int CurrentScore { get; private set; }
        public int KillCount { get; private set; }

        private float _currentHealth;
        private float _gameTime;

        private IApplicationManager _appManager;
        private GameUIManager _gameUIManager;

        private void Awake()
        {
            // Register with ServiceLocator
            ServiceLocator.RegisterService<IGameplayManager>(this);
            Debug.Log("[GameplayManager] Registered with ServiceLocator");
        }

        private void Start()
        {
            _appManager = ServiceLocator.GetService<IApplicationManager>();
            _gameUIManager = FindObjectOfType<GameUIManager>();

            // Initialize session
            CurrentScore = 0;
            KillCount = 0;
            _gameTime = 0;

            if (_gameUIManager != null)
            {
                _gameUIManager.InitializeUI();

                // Push initial player health to UI
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var health = player.GetComponent<Health>();
                    if (health != null)
                    {
                        health.RefreshHealthUI();
                    }
                }
            }
        }

        private void Update()
        {
            if (_appManager != null && _appManager.CurrentGameState == GameState.Gameplay)
            {
                _gameTime += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterService<IGameplayManager>(this);
            Debug.Log("[GameplayManager] Unregistered from ServiceLocator");
        }

        public void AddScore(int score)
        {
            CurrentScore += score;
            if (_gameUIManager != null)
            {
                _gameUIManager.UpdateScore(CurrentScore);
            }
        }

        public void OnEnemyDestroyed(int killCount)
        {
            KillCount += killCount;
            if (_gameUIManager != null)
            {
                _gameUIManager.UpdateKillCount(KillCount / 2);
            }

            // Notify difficulty manager
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.EnemyKilled();
            }
        }

        public void UpdateHealth(float newHealth)
        {
            _currentHealth = newHealth;
            if (_gameUIManager != null)
            {
                _gameUIManager.UpdateHealth(newHealth);
            }
        }

        public void OnPlayerDied()
        {
            if (_appManager != null)
            {
                _appManager.SetGameOverState();
            }

            if (_gameUIManager != null)
            {
                _gameUIManager.ShowGameOverMenu();
                _gameUIManager.UpdateGameOverScore(CurrentScore);
            }
        }

        public string GetFormatedTime()
        {
            int hours = Mathf.FloorToInt(_gameTime / 3600);
            int minutes = Mathf.FloorToInt((_gameTime % 3600) / 60);
            int seconds = Mathf.FloorToInt((_gameTime % 3600) % 60);

            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }
}
