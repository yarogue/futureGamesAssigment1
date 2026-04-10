using System.Collections.Generic;
using generalScripts.Interfaces;
using scriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace generalScripts.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject mainMenu;

        [SerializeField]
        private TMP_InputField nameInputField;

        [SerializeField]
        private List<GameObject> panels = new List<GameObject>();

        [Header("Leaderboard")]
        [SerializeField] private Transform leaderboardEntriesParent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        private enum CurrentPanel
        {
            MainMenu = 0,
            StartGame = 1,
            Leaderboard = 2
        }

        private CurrentPanel _currentPanel;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _currentPanel = CurrentPanel.MainMenu;
            UpdateUI();
        }

        private void UpdateUI()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].SetActive(i == (int)_currentPanel);
            }
        }

        public void OnStartButtonClick()
        {
            _currentPanel = CurrentPanel.StartGame;
            UpdateUI();
        }

        public void OnLeaderboardButtonClick()
        {
            _currentPanel = CurrentPanel.Leaderboard;
            UpdateUI();
            RefreshLeaderboard();
        }
        
        public void OnExtrasButtonClick()
        {
            OnLeaderboardButtonClick();
        }

        public void OnExitButtonClick()
        {
            Application.Quit();
        }

        public void OnBackButtonClick()
        {
            _currentPanel = CurrentPanel.MainMenu;
            UpdateUI();
        }

        public void OnConfirmExitClicked()
        {
            Application.Quit();
        }

        public void OnStartGameConfirmed()
        {
            var playerName = nameInputField.text;

            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("Player name is empty");
                return;
            }

            var dataManager = ServiceLocator.GetService<IDataManager>();
            dataManager.SetCurrentPlayer(playerName);

            //Debug.Log($"[MainMenu] Starting game for player: {playerName}");
            SceneManager.LoadScene((int)SceneIndex.GameScene);
        }

        private void RefreshLeaderboard()
        {
            if (leaderboardEntriesParent == null || leaderboardEntryPrefab == null)
            {
                //Debug.LogWarning("[MainMenu] Leaderboard entries parent or prefab not assigned");
                return;
            }

            // Clear old entries
            foreach (Transform child in leaderboardEntriesParent)
            {
                Destroy(child.gameObject);
            }

            if (!ServiceLocator.TryGetService<IDataManager>(out var dataManager)) return;

            List<PlayerData> leaderboard = dataManager.GetSortedLeaderboard();
            int count = Mathf.Min(leaderboard.Count, 10);

            for (int i = 0; i < count; i++)
            {
                var entry = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{i + 1}. {leaderboard[i].username} - {leaderboard[i].highScore}";
                }
            }

            if (count == 0)
            {
                var entry = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = "No scores yet. Play the game!";
                }
            }
        }
    }
}
