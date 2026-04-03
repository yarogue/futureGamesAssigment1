using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using generalScripts.Interfaces;
using MainCharacterScripts;
using scriptableObjects;

namespace generalScripts
{
    public class LocalJsonBackend : IDataBackend
    {
        private PlayerLeaderboard leaderboard = new PlayerLeaderboard();
        private string saveFilePath;
        private const string fileName = "leaderboard.json";
        private string currentPlayerUsername;

        public LocalJsonBackend()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
            LoadLeaderboard();
        }

        public void SavePlayerData(string playerName)
        {
            currentPlayerUsername = playerName;
        }

        public void SaveScore(string playerName, int score)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            var existingPlayer = leaderboard.players.FirstOrDefault(p => p.username == playerName);

            if (existingPlayer != null)
            {
                if (score > existingPlayer.highScore)
                {
                    existingPlayer.highScore = score;
                }
                else
                {
                    return;
                }
            }
            else
            {
                leaderboard.players.Add(new PlayerData(playerName, score));
            }

            SaveLeaderboard();
        }

        public List<PlayerData> GetLeaderboard()
        {
            return leaderboard.players.OrderByDescending(p => p.highScore).ToList();
        }

        private void LoadLeaderboard()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    var json = File.ReadAllText(saveFilePath);
                    leaderboard = JsonUtility.FromJson<PlayerLeaderboard>(json);
                }
                catch (System.Exception e)
                {
                    leaderboard = new PlayerLeaderboard();
                }
            }
            else
            {
                leaderboard = new PlayerLeaderboard();
            }
        }

        private void SaveLeaderboard()
        {
            try
            {
                leaderboard.players = leaderboard.players.OrderByDescending(p => p.highScore).ToList();
                var json = JsonUtility.ToJson(leaderboard, true);
                File.WriteAllText(saveFilePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalJsonBackend] Failed to save leaderboard: {e.Message}");
            }
        }

        public string GetCurrentPlayerUsername()
        {
            return currentPlayerUsername;
        }
    }
}