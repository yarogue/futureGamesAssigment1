using UnityEngine;
using MainCharacterScripts;
using scriptableObjects;
using System.Collections;
using System.Collections.Generic;
using generalScripts;
using generalScripts.Managers;
using generalScripts.Interfaces;

namespace EnemyScripts
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Factory Reference")]
        [SerializeField] private EnemyFactory enemyFactory;

        [Header("Spawn Settings")]
        [SerializeField]
        public float spawnInterval;
        [SerializeField]
        private float spawnRadius;

        private Transform _playerTransform;
        private int _enemiesAliveCount = 0;
        private Coroutine _spawnCoroutine;

        private void Start()
        {
            if (enemyFactory == null)
            {
                return;
            }

            if (DifficultyManager.Instance == null)
            {
                return;
            }

            if (ServiceLocator.TryGetService<IPlayerController>(out var playerController))
            {
                _playerTransform = playerController.transform;
                if (_spawnCoroutine != null)
                {
                    StopCoroutine(_spawnCoroutine);
                }
                _spawnCoroutine = StartCoroutine(SpawnEnemies());
            }
        }
        public void Initialize(Transform player)
        {
            if (_playerTransform == null)
            {
                _playerTransform = player;

                if (_spawnCoroutine != null)
                {
                    StopCoroutine(_spawnCoroutine);
                }
                _spawnCoroutine = StartCoroutine(SpawnEnemies());
            }
        }

        private IEnumerator SpawnEnemies()
        {
            if (_playerTransform == null)
            {
                Debug.LogError("Player transform not set. Spawner cannot run.");
                yield break;
            }

            var appManager = ServiceLocator.TryGetService<IApplicationManager>(out var app) ? app : null;

            while (true)
            {
                // Only spawn during Gameplay state
                if (appManager != null && appManager.CurrentGameState != GameState.Gameplay)
                {
                    yield return null;
                    continue;
                }

                var rateMultiplier = DifficultyManager.Instance != null
                    ? DifficultyManager.Instance.GetCurrentSpawnRateMultiplier()
                    : 1f;
                var actualSpawnInterval = spawnInterval / rateMultiplier;

                yield return new WaitForSeconds(actualSpawnInterval);

                List<EnemyData> availableEnemies = DifficultyManager.Instance != null
                    ? DifficultyManager.Instance.GetAvailableEnemyTypes()
                    : new List<EnemyData>();

                if (availableEnemies.Count == 0)
                {
                    continue;
                }

                var spawnPosition = _playerTransform.position + (Vector3)(Random.insideUnitCircle.normalized * spawnRadius);

                EnemyData enemyData = availableEnemies[Random.Range(0, availableEnemies.Count)];

                IEnemy newEnemy = null;
                var type = enemyData.enemyType;

                if (type == EnemyType.Melee)
                {
                    newEnemy = enemyFactory.CreateMeleeEnemy(spawnPosition, enemyData, _playerTransform);
                }
                else if (type == EnemyType.Ranged)
                {
                    newEnemy = enemyFactory.CreateRangedEnemy(spawnPosition, enemyData, _playerTransform);
                }

                if (newEnemy == null)
                {
                    Debug.LogWarning("Failed to spawn enemy :(");
                }
                else
                {
                    _enemiesAliveCount++;
                }
            }
        }

        public void OnEnemyDestroyed()
        {
            _enemiesAliveCount--;

            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.EnemyKilled();
            }
        }
    }
}