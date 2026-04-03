using System.Collections.Generic;
using UnityEngine;
using generalScripts.Interfaces;
using scriptableObjects;

namespace generalScripts.Managers
{
    public class DatabaseManager : MonoBehaviour, IDataManager
    {
        private IDataBackend _currentBackend;
        private PlayFabBackend _onlineBackend;
        private LocalJsonBackend _offlineBackend;

        public string CurrentPlayerUsername { get; private set; }
        public bool IsOnline { get; private set; }

        private void Awake()
        {
            _offlineBackend = new LocalJsonBackend();
            _onlineBackend = new PlayFabBackend();

            ServiceLocator.RegisterService<IDataManager>(this);

            _currentBackend = _offlineBackend;
            IsOnline = false;
        }

        private void Start()
        {
            // PlayFab score API not configured — use local JSON for showcase
            // TryConnectPlayFab();
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterService<IDataManager>(this);
        }

        private void TryConnectPlayFab()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }
            

            try
            {
                _onlineBackend.LoginWithDeviceId((success) =>
                {
                    if (success)
                    {
                        _currentBackend = _onlineBackend;
                        IsOnline = true;
                    }
                    else
                    {
                        _currentBackend = _offlineBackend;
                        IsOnline = false;
                    }
                });
            }
            catch (System.Exception ex)
            {
                _currentBackend = _offlineBackend;
                IsOnline = false;
            }
        }

        public void SetCurrentPlayer(string username)
        {
            CurrentPlayerUsername = username;
            _currentBackend.SavePlayerData(username);
        }

        public void LoadLeaderboard()
        {
        }

        public void AddOrUpdateHighScore(string username, int score)
        {
            _currentBackend.SaveScore(username, score);
        }

        public List<PlayerData> GetSortedLeaderboard()
        {
            return _currentBackend.GetLeaderboard();
        }

        public void ForceOfflineMode()
        {
            _currentBackend = _offlineBackend;
            IsOnline = false;
        }

        public void RetryOnlineConnection()
        {
            TryConnectPlayFab();
        }
    }
}
