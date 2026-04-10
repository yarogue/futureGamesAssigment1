namespace generalScripts.Managers
{
    // TEMPORARILY DISABLED - Migrating to ApplicationManager + GameplayManager architecture
    
    
    // TODO: Migrate all GameManager.Instance references to use ServiceLocator


    /*
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentGameState { get; private set; }

        [Header("Difficulty Settings")]
        [SerializeField] private int difficultyActivationThreshold;

        [Header("References")]
        [SerializeField] private DifficultyManager difficultyManager;

        [Header("Game Data")]
        [SerializeField]
        private PlayerStats playerStats;

        private float _gameTime,
                 _currentHealth;
        private int _currentScore,
                       _killCount;

        private bool _difficultyActivated;

        private GameUIManager _gameUIManager;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _gameUIManager = GetComponent<GameUIManager>();
            SetGameState(GameState.Gameplay);
            _currentScore = 0;
            _killCount = 0;
            _gameTime = 0;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (CurrentGameState == GameState.Gameplay)
            {
                _gameTime += Time.deltaTime;
            }
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        public void AddScore(int score)
        {
            _currentScore += score;
            if (_gameUIManager != null)
            {
                _gameUIManager.UpdateScore(_currentScore);
            }
        }
        public void OnEnemyDestroyed(int killCount)
        {
            _killCount += killCount;
            if (_gameUIManager != null)
            {
                _gameUIManager.UpdateKillCount(_killCount / 2);
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

        public void TogglePause()
        {
            if (CurrentGameState == GameState.Gameplay)
            {
                SetGameState(GameState.Paused);
            }
            else if (CurrentGameState == GameState.Paused)
            {
                SetGameState(GameState.Gameplay);
            }
        }

        public void OnPlayerDied()
        {
            CurrentGameState = GameState.GameOver;
            Time.timeScale = 0f;
            SetGameState(GameState.GameOver);
        }

        public void RestartGame()
        {
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.ResetDifficulty();
            }

            _gameTime = 0;
            _currentScore = 0;
            _killCount = 0;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            if (playerStats != null)
            {
                Debug.Log("Resetting player stats before scene reload.");
                playerStats.currentMissileAmount = playerStats.maxMissileAmount;
            }

            CurrentGameState = GameState.Gameplay;
            _difficultyActivated = false;
            Time.timeScale = 1f;
        }

        private void SetGameState(GameState newState)
        {
            CurrentGameState = newState;

            if (_gameUIManager == null)
            {
                _gameUIManager = FindObjectOfType<GameUIManager>();
            }
            if (_gameUIManager == null)
            {
                return;
            }

            switch (CurrentGameState)
            {
                case GameState.Gameplay:
                    Time.timeScale = 1;
                    _gameUIManager.ShowGameplayUI();
                    _gameUIManager.UpdateScore(_currentScore);
                    _gameUIManager.UpdateKillCount(_killCount);
                    break;
                case GameState.Paused:
                    Time.timeScale = 0;
                    _gameUIManager.ShowPauseMenu();
                    break;
                case GameState.GameOver:
                    _gameUIManager.ShowGameOverMenu();
                    _gameUIManager.UpdateGameOverScore(_currentScore);
                    break;

                default:
                    Debug.Log("Game state not set");
                    break;

            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == SceneManager.GetActiveScene().buildIndex)
            {
                _gameUIManager = FindObjectOfType<GameUIManager>();
                if (_gameUIManager != null)
                {
                    _gameUIManager.InitializeUI();
                }

                if (WorldManager.Instance != null)
                {
                    var newPlayer = FindObjectOfType<PlayerController>();
                    if (newPlayer != null)
                    {
                        WorldManager.Instance.ResetWorld(newPlayer.transform);
                    }
                }

                SetGameState(GameState.Gameplay);
            }
        }
        public string GetFormatedTime()

        {

            int hours = Mathf.FloorToInt(_gameTime / 3600),

                minutes = Mathf.FloorToInt((_gameTime % 3600) / 60),

                seconds = Mathf.FloorToInt((_gameTime % 3600) % 60);


            return $"{hours:00}:{minutes:00}:{seconds:00}";

        }
    }
    */
}
