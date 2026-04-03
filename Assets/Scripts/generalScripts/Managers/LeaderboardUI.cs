using System.Collections.Generic;
using generalScripts.Interfaces;
using scriptableObjects;
using TMPro;
using UnityEngine;

namespace generalScripts.Managers
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Leaderboard Panel")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform entriesParent;
        [SerializeField] private GameObject entryPrefab;

        [Header("Settings")]
        [SerializeField] private int maxEntries = 10;

        public void ShowLeaderboard()
        {
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(true);
            }
            
            RefreshLeaderboard();
        }

        public void HideLeaderboard()
        {
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(false);
            }
        }

        public void RefreshLeaderboard()
        {
            if (entriesParent == null || entryPrefab == null) return;

            // Clear old entries
            foreach (Transform child in entriesParent)
            {
                Destroy(child.gameObject);
            }

            // Get the leaderboard data
            if (!ServiceLocator.TryGetService<IDataManager>(out var dataManager)) return;
            
            List<PlayerData> leaderboard = dataManager.GetSortedLeaderboard();

            int count = Mathf.Min(leaderboard.Count, maxEntries);
            for (int i = 0; i < count; i++)
            {
                var entry = Instantiate(entryPrefab, entriesParent);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{i + 1}. {leaderboard[i].username} - {leaderboard[i].highScore}";
                }
            }

            // If no entries, show a placeholder
            if (count == 0)
            {
                var entry = Instantiate(entryPrefab, entriesParent);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = "No scores yet. Play the game!";
                }
            }
        }
    }
}
