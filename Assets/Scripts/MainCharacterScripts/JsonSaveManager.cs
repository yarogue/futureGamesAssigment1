using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using scriptableObjects;

namespace MainCharacterScripts
{
    public class JsonSaveManager : MonoBehaviour
{
    // A single instance of our save manager (Singleton pattern).
    public static JsonSaveManager Instance { get; private set; }

    // This will hold the username of the current player.
    public string CurrentPlayerUsername { get; private set; }

    // The leaderboard data we will be working with.
    private PlayerLeaderboard leaderboard = new PlayerLeaderboard();

    // The file path where the data will be saved.
    private string saveFilePath;
    private const string fileName = "leaderboard.json";

    private void Awake()
    {
        // Implement the Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
            // Prevents the object from being destroyed when a new scene loads.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Define the save file path using Unity's built-in application data path.
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
    }
    
    private void Start()
    {
        // Load the leaderboard data as soon as the game starts.
        LoadLeaderboard();
    }

    // --- New method to set the current player's username ---
    public void SetCurrentPlayer(string username)
    {
        CurrentPlayerUsername = username;
    }

    /// <summary>
    /// Loads the leaderboard data from a JSON file.
    /// </summary>
    public void LoadLeaderboard()
    {
        if (File.Exists(saveFilePath))
        {
            var json = File.ReadAllText(saveFilePath);
            leaderboard = JsonUtility.FromJson<PlayerLeaderboard>(json);
        }
        else
        {
            // If the file doesn't exist, create a new, empty leaderboard.
            leaderboard = new PlayerLeaderboard();
        }
    }

    /// <summary>
    /// Saves the current leaderboard data to a JSON file.
    /// </summary>
    private void SaveLeaderboard()
    {
        // Sort the players by high score before saving.
        leaderboard.players = leaderboard.players.OrderByDescending(p => p.highScore).ToList();
        
        var json = JsonUtility.ToJson(leaderboard, true);
        File.WriteAllText(saveFilePath, json);
    }

    /// <summary>
    /// Adds or updates a player's high score.
    /// </summary>
    /// <param name="username">The player's username.</param>
    /// <param name="highScore">The new high score to save.</param>
    public void AddOrUpdateHighScore(string username, int newScore)
    {
        // Check if the player already exists in the leaderboard.
        var existingPlayer = leaderboard.players.FirstOrDefault(p => p.username == username);

        if (existingPlayer != null)
        {
            // If the player exists, update their score if the new one is higher.
            if (newScore > existingPlayer.highScore)
            {
                existingPlayer.highScore = newScore;
            }
        }
        else
        {
            // If the player doesn't exist, add them to the leaderboard.
            leaderboard.players.Add(new PlayerData(username, newScore));
        }

        // After adding or updating, save the leaderboard.
        SaveLeaderboard();
    }

    /// <summary>
    /// Retrieves the sorted list of players for display on a leaderboard UI.
    /// </summary>
    /// <returns>A sorted list of PlayerData objects.</returns>
    public List<PlayerData> GetSortedLeaderboard()
    {
        return leaderboard.players.OrderByDescending(p => p.highScore).ToList();
    }
}
}