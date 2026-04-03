using MainCharacterScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using generalScripts.Interfaces;

namespace generalScripts.Managers
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField]
        private PlayerStats playerData;

        [Header("UI Panels")]
        [SerializeField]
        private GameObject gameplayPanel;
        [SerializeField]
        private GameObject pauseMenuPanel;
        [SerializeField]
        private GameObject gameOverPanel;

        [Header("UI Text")]
        [SerializeField]
        private TextMeshProUGUI playerNameText;
        [SerializeField]
        private TextMeshProUGUI playerScoreText;
        [SerializeField]
        private TextMeshProUGUI gameOverScoreText;

        [Header("HUD")]

        [Header("Top UI's")]
        [SerializeField]
        private TextMeshProUGUI healthText;
        [SerializeField]
        private TextMeshProUGUI timerText;
        [SerializeField]
        private TextMeshProUGUI currentScoreText;
        [SerializeField]
        private TextMeshProUGUI killCountText;
        [SerializeField]

        [Header("Bottom UI's")]
        private TextMeshProUGUI levelText;
        [SerializeField]
        private TextMeshProUGUI xpProgressText;
        [SerializeField]
        private Slider xpSlider;
        [SerializeField]
        private TextMeshProUGUI missileCountText;
        [SerializeField]
        private Slider missileReloadSlider;

        private int _currentScore,
                    _killCount;

        private float _currentHealth;

        private float _lastMissileFiredTime = -100f;
        private float _missileCooldownDuration = 0.5f;

        private bool _isPaused = false;
        private bool _isGameOver = false;
        private IInputManager _inputManager;

        private void OnEnable()
        {
            PlayerShooting.OnMissileAmmoUpdated += UpdateMissileCountDisplay;
            PlayerShooting.OnMissileCooldownStarted += StartMissileCooldownDisplay;

            // Subscribe to pause input
            _inputManager = ServiceLocator.GetService<IInputManager>();
            if (_inputManager != null)
            {
                _inputManager.OnPausePressed += TogglePause;
            }
        }

        private void OnDisable()
        {
            PlayerShooting.OnMissileAmmoUpdated -= UpdateMissileCountDisplay;
            PlayerShooting.OnMissileCooldownStarted -= StartMissileCooldownDisplay;

            // Unsubscribe from pause input
            if (_inputManager != null)
            {
                _inputManager.OnPausePressed -= TogglePause;
            }
        }

        public void InitializeUI()
        {
            _currentScore = 0;
            _killCount = 0;

            UpdateScore(_currentScore);
            UpdateKillCount(_killCount);

            ShowGameplayUI();

            var dataManager = ServiceLocator.GetService<IDataManager>();
            if (dataManager != null && !string.IsNullOrEmpty(dataManager.CurrentPlayerUsername))
            {
                SetPlayerName(dataManager.CurrentPlayerUsername);
            }

            if (playerData != null)
            {
                UpdateMissileCountDisplay(playerData.currentMissileAmount, playerData.maxMissileAmount);
            }

            if (missileReloadSlider != null)
            {
                missileReloadSlider.maxValue = _missileCooldownDuration;
                missileReloadSlider.value = _missileCooldownDuration;
                missileReloadSlider.gameObject.SetActive(false);
            }
        }
        private void Start()
        {
            xpProgressText.text = "0 / 0 XP";
        }

        public void Update()
        {
            /*
            if (GameManager.Instance != null && timerText != null)
            {
                timerText.text = GameManager.Instance.GetFormatedTime();
            }
            */
            currentScoreText.text = "Current score: " + _currentScore;
            killCountText.text = "Kill count: " + _killCount;
            healthText.text = "Health: " + Mathf.Max(0, _currentHealth).ToString("F0");

            UpdateMissileSlider();
        }

        public void ShowGameplayUI()
        {
            gameplayPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void ShowPauseMenu()
        {
            pauseMenuPanel.SetActive(true);
            gameplayPanel.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ShowGameOverMenu()
        {
            _isGameOver = true;
            gameOverPanel.SetActive(true);
            gameplayPanel.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void TogglePause()
        {
            // Block pause during game over
            if (_isGameOver) return;

            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Time.timeScale = 0f;
                ShowPauseMenu();
                Debug.Log("[GameUIManager] Game Paused");
            }
            else
            {
                Time.timeScale = 1f;
                ShowGameplayUI();
                Debug.Log("[GameUIManager] Game Resumed");
            }
        }

        public void SetPlayerName(string name)
        {
            playerNameText.text = "Player: " + name;
        }

        public void UpdateScore(int score)
        {
            _currentScore = score;
            playerScoreText.text = "Score: " + _currentScore;
        }

        public void UpdateHealth(float health)
        {
            _currentHealth = health;
            healthText.text = "Health: " + Mathf.Max(0, _currentHealth).ToString("F0");
        }

        public void UpdateKillCount(int killCount)
        {
            _killCount = killCount;
            killCountText.text = "Kill count: " + _killCount;
        }

        //Level up and XP parts.
        public void UpdateLevelDisplay(int level, float progression, int currentXp, int requiredXp)
        {
            if (levelText != null)
            {
                levelText.text = $"Level:  {level}";
            }

            if (xpSlider != null)
            {
                xpSlider.maxValue = requiredXp;

                xpSlider.value = currentXp;
            }

            if (xpProgressText != null)
            {
                xpProgressText.text = $"{currentXp} / {requiredXp} XP";
            }
        }

        //Missile UI

        private void UpdateMissileSlider()
        {
            if (missileReloadSlider == null) return;
            float timeElapsed = Time.time - _lastMissileFiredTime;

            float progress = Mathf.Min(timeElapsed, _missileCooldownDuration);
            missileReloadSlider.value = progress;

            // Hide slider when cooldown is done
            if (progress >= _missileCooldownDuration && missileReloadSlider.gameObject.activeSelf)
            {
                missileReloadSlider.gameObject.SetActive(false);
            }
        }

        public void UpdateMissileCountDisplay(int currentAmmo, int maxAmmo)
        {
            if (missileCountText != null)
            {
                missileCountText.text = $"Missiles: {currentAmmo} / {maxAmmo}";
            }
        }

        public void StartMissileCooldownDisplay(float cooldownDuration)
        {
            _missileCooldownDuration = cooldownDuration;
            _lastMissileFiredTime = Time.time;

            if (missileReloadSlider != null)
            {
                missileReloadSlider.gameObject.SetActive(true);
                missileReloadSlider.maxValue = cooldownDuration;
                missileReloadSlider.value = 0f;
            }
        }

        public void UpdateGameOverScore(int score)
        {
            if (gameOverPanel != null)
            {
                gameOverScoreText.text = "Score: " + score;
            }
        }
        public void OnContinueClicked()
        {
            TogglePause(); // Resume game
        }

        public void OnMainMenuClicked()
        {
            var dataManager = ServiceLocator.GetService<IDataManager>();
            if (dataManager != null)
            {
                dataManager.AddOrUpdateHighScore(dataManager.CurrentPlayerUsername, _currentScore);
            }

            // Destroy persistent player before going to menu
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                Destroy(player.gameObject);
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        public void OnExitGameClicked()
        {
            var dataManager = ServiceLocator.GetService<IDataManager>();
            if (dataManager != null)
            {
                dataManager.AddOrUpdateHighScore(dataManager.CurrentPlayerUsername, _currentScore);
            }
            Application.Quit();
        }
        public void OnRestartGameClicked()
        {
            var dataManager = ServiceLocator.GetService<IDataManager>();
            if (dataManager != null)
            {
                dataManager.AddOrUpdateHighScore(dataManager.CurrentPlayerUsername, _currentScore);
            }

            // Destroy persistent player before reloading
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                Destroy(player.gameObject);
            }

            // Reset difficulty
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.ResetDifficulty();
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}