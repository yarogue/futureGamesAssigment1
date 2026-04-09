using MainCharacterScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using generalScripts.Interfaces;
using System.Collections.Generic;
using scriptableObjects;

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

        [Header("Level-Up Panel")]
        [Tooltip("Root GameObject of the level-up upgrade picker panel")]
        [SerializeField] private GameObject levelUpPanel;
        [Tooltip("The three upgrade choice buttons (assign in order: 0, 1, 2)")]
        [SerializeField] private Button[] upgradeButtons = new Button[3];
        [Tooltip("TMP text on each button showing the upgrade name")]
        [SerializeField] private TextMeshProUGUI[] upgradeNameTexts = new TextMeshProUGUI[3];
        [Tooltip("TMP text on each button showing the upgrade description")]
        [SerializeField] private TextMeshProUGUI[] upgradeDescTexts = new TextMeshProUGUI[3];

        private int _currentScore,
                    _killCount;

        private float _currentHealth;

        private float _lastMissileFiredTime = -100f;
        private float _missileCooldownDuration = 0.5f;

        private bool _isPaused = false;
        private bool _isGameOver = false;
        private bool _isLevelUpOpen = false;   // blocks pause while picking upgrade
        private IInputManager _inputManager;

        // Holds the current set of upgrade options shown to the player
        private List<UpgradeDefinition> _currentUpgradeOptions = new List<UpgradeDefinition>();

        private void OnEnable()
        {
            PlayerShooting.OnMissileAmmoUpdated += UpdateMissileCountDisplay;
            PlayerShooting.OnMissileCooldownStarted += StartMissileCooldownDisplay;
            PlayerLevelController.OnLevelUp += ShowLevelUpPanel;

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
            PlayerLevelController.OnLevelUp -= ShowLevelUpPanel;

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
            // Block pause during game over or while level-up panel is open
            if (_isGameOver || _isLevelUpOpen) return;

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

        // ── Level-Up Upgrade Panel ────────────────────────────────────────────

        /// <summary>
        /// Called automatically when PlayerLevelController.OnLevelUp fires.
        /// Pauses the game and populates the upgrade choice panel.
        /// </summary>
        private void ShowLevelUpPanel()
        {
            Debug.Log($"[GameUIManager] ShowLevelUpPanel called. Panel={levelUpPanel}, UpgradeManager={PlayerUpgradeManager.Instance}");

            if (levelUpPanel == null)
            {
                Debug.LogWarning("[GameUIManager] levelUpPanel is not assigned in the Inspector!");
                return;
            }

            if (PlayerUpgradeManager.Instance == null)
            {
                Debug.LogWarning("[GameUIManager] PlayerUpgradeManager not found!");
                return;
            }

            // Pick 3 random upgrades
            _currentUpgradeOptions = PlayerUpgradeManager.Instance.GetRandomUpgrades(3);

            // Populate button texts
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                bool hasOption = i < _currentUpgradeOptions.Count;

                if (upgradeButtons[i] != null)
                    upgradeButtons[i].gameObject.SetActive(hasOption);

                if (!hasOption) continue;

                var upgrade = _currentUpgradeOptions[i];

                if (upgradeNameTexts[i] != null)
                    upgradeNameTexts[i].text =
                        $"{upgrade.upgradeName}  +{upgrade.percentageIncrease * 100f:F0}%";

                if (upgradeDescTexts[i] != null)
                    upgradeDescTexts[i].text = upgrade.description;
            }

            // Pause game and show panel
            _isLevelUpOpen = true;
            Time.timeScale = 0f;
            levelUpPanel.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Wire each upgrade button's OnClick to this with the button index (0, 1, or 2).
        /// Applies the chosen upgrade, hides the panel, and resumes the game.
        /// </summary>
        public void OnUpgradeChosen(int index)
        {
            if (index < 0 || index >= _currentUpgradeOptions.Count)
            {
                Debug.LogWarning($"[GameUIManager] Invalid upgrade index: {index}");
                return;
            }

            PlayerUpgradeManager.Instance.ApplyUpgrade(_currentUpgradeOptions[index]);

            // Close panel and resume
            _isLevelUpOpen = false;
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;

            _currentUpgradeOptions.Clear();
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