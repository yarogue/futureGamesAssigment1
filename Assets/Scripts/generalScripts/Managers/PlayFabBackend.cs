using System.Collections.Generic;
using UnityEngine;
using generalScripts.Interfaces;
using scriptableObjects;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;

namespace generalScripts
{
    public class PlayFabBackend : IDataBackend
    {
        private const string TITLE_ID = "ADD76";
        private string currentPlayerUsername;
        private bool isLoggedIn = false;
        private bool isLoggingIn = false;

        public PlayFabBackend()
        {
            PlayFabSettings.TitleId = TITLE_ID;
            Debug.Log($"[PlayFabBackend] Initialized with Title ID: {TITLE_ID}");
        }

        public void LoginWithDeviceId(System.Action<bool> onComplete)
        {
            if (isLoggingIn)
            {
                Debug.LogWarning("[PlayFabBackend] Login already in progress");
                onComplete?.Invoke(false);
                return;
            }

            isLoggingIn = true;

            try
            {
                var customId = SystemInfo.deviceUniqueIdentifier;

                var request = new LoginWithCustomIDRequest
                {
                    CustomId = customId,
                    CreateAccount = true,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                    {
                        GetPlayerProfile = true
                    }
                };

                Debug.Log($"[PlayFabBackend] Attempting login with CustomID: {customId.Substring(0, 8)}...");

                PlayFabClientAPI.LoginWithCustomID(request,
                    result => OnLoginSuccess(result, onComplete),
                    error => OnLoginFailure(error, onComplete)
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayFabBackend] Exception during login attempt: {ex.Message}");
                isLoggingIn = false;
                isLoggedIn = false;
                onComplete?.Invoke(false);
            }
        }

        private void OnLoginSuccess(LoginResult result, System.Action<bool> onComplete)
        {
            isLoggedIn = true;
            isLoggingIn = false;

            Debug.Log($"[PlayFabBackend] ✓ Login successful! PlayFab ID: {result.PlayFabId}");

            if (result.InfoResultPayload?.PlayerProfile != null)
            {
                Debug.Log($"[PlayFabBackend] Display Name: {result.InfoResultPayload.PlayerProfile.DisplayName ?? "Not Set"}");
            }

            onComplete?.Invoke(true);
        }

        private void OnLoginFailure(PlayFabError error, System.Action<bool> onComplete)
        {
            isLoggedIn = false;
            isLoggingIn = false;

            Debug.LogWarning($"[PlayFabBackend] Login failed - falling back to offline mode");
            Debug.LogWarning($"[PlayFabBackend] Reason: {error.ErrorMessage}");

            // Check for common errors and give helpful hints
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                Debug.LogWarning("[PlayFabBackend] Hint: Check if 'Disable player creation using CLIENT/LoginWithCustomID' is unchecked in PlayFab settings");
            }
            else if (error.Error == PlayFabErrorCode.InvalidParams)
            {
                Debug.LogWarning("[PlayFabBackend] Invalid parameters in login request");
            }

            // Log full details only in editor for debugging
#if UNITY_EDITOR
            Debug.Log($"[PlayFabBackend] Full error details:\n{error.GenerateErrorReport()}");
#endif

            onComplete?.Invoke(false);
        }

        public void SavePlayerData(string playerName)
        {
            if (!isLoggedIn)
            {
                Debug.LogWarning("[PlayFabBackend] Cannot save player data - not logged in");
                return;
            }

            currentPlayerUsername = playerName;

            // Validate display name for PlayFab requirements
            // PlayFab requires: 3-25 characters, alphanumeric + some special chars, no spaces
            if (string.IsNullOrEmpty(playerName) || playerName.Length < 3 || playerName.Length > 25)
            {
                Debug.LogWarning($"[PlayFabBackend] Display name '{playerName}' invalid (must be 3-25 characters) - skipping PlayFab update");
                return;
            }

            // Remove spaces (PlayFab doesn't allow them)
            string sanitizedName = playerName.Replace(" ", "_");

            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = sanitizedName
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(request,
                result =>
                {
                    Debug.Log($"[PlayFabBackend] ✓ Display name set to: {sanitizedName}");
                },
                error =>
                {
                    Debug.LogWarning($"[PlayFabBackend] Failed to set display name: {error.ErrorMessage}");
                }
            );
        }

        public void SaveScore(string playerName, int score)
        {
            if (!isLoggedIn)
            {
                Debug.LogWarning("[PlayFabBackend] Cannot save score - not logged in");
                return;
            }

            // Submit score to leaderboard
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "HighScore",
                        Value = score
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request,
                result =>
                {
                    Debug.Log($"[PlayFabBackend] ✓ Score submitted: {playerName} = {score}");
                },
                error =>
                {
                    Debug.LogWarning($"[PlayFabBackend] Failed to submit score: {error.ErrorMessage}");
                }
            );
        }

        public List<PlayerData> GetLeaderboard()
        {
            if (!isLoggedIn)
            {
                Debug.LogWarning("[PlayFabBackend] Cannot get leaderboard - not logged in");
                return new List<PlayerData>();
            }

            var leaderboardData = new List<PlayerData>();

            var request = new GetLeaderboardRequest
            {
                StatisticName = "HighScore",
                StartPosition = 0,
                MaxResultsCount = 10
            };

            PlayFabClientAPI.GetLeaderboard(request,
                result =>
                {
                    Debug.Log($"[PlayFabBackend] ✓ Leaderboard fetched: {result.Leaderboard.Count} entries");

                    foreach (var entry in result.Leaderboard)
                    {
                        leaderboardData.Add(new PlayerData(
                            entry.DisplayName ?? "Unknown",
                            entry.StatValue
                        ));
                    }
                },
                error =>
                {
                    Debug.LogError($"[PlayFabBackend] Failed to fetch leaderboard: {error.ErrorMessage}");
                }
            );

            return leaderboardData;
        }

        public string GetCurrentPlayerUsername()
        {
            return currentPlayerUsername;
        }

        public bool IsLoggedIn()
        {
            return isLoggedIn;
        }
    }
}