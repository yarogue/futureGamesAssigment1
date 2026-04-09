using System;
using proceduralMaps;
using UnityEngine;

namespace generalScripts.Managers
{
    public class GameSceneManager : MonoBehaviour
    {
        /// <summary>Fired after the player is spawned. Subscribe to wire camera, etc.</summary>
        public static event Action<Transform> OnPlayerSpawned;

        [Header("Player")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        private void Start()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[GameSceneManager] Player Prefab is not assigned!");
                return;
            }

            var spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            // Notify subscribers (camera, etc.)
            OnPlayerSpawned?.Invoke(player.transform);

            // Tell WorldManager to generate the map around the new player
            if (WorldManager.Instance != null)
            {
                WorldManager.Instance.ResetWorld(player.transform);
                Debug.Log("[GameSceneManager] WorldManager reset to new player.");
            }
            else
            {
                Debug.LogWarning("[GameSceneManager] WorldManager.Instance is null — map won't generate.");
            }
        }
    }
}