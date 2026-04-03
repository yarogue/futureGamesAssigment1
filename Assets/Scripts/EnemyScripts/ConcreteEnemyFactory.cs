using UnityEngine;
using scriptableObjects;
using generalScripts;

namespace EnemyScripts
{
    public class ConcreteEnemyFactory : EnemyFactory
    {
        [Header("Enemy prefabs")]
        [SerializeField] private GameObject meleeEnemyPrefab;
        [SerializeField] private GameObject rangedEnemyPrefab;

        public override IEnemy CreateMeleeEnemy(Vector3 spawnPosition, EnemyData enemyData, Transform playerTransform)
        {
            GameObject enemyGo;

            if (ObjectPool.Instance != null)
                enemyGo = ObjectPool.Instance.Get(meleeEnemyPrefab, spawnPosition, Quaternion.identity);
            else
                enemyGo = Instantiate(meleeEnemyPrefab, spawnPosition, Quaternion.identity);

            IEnemy newEnemy = enemyGo.GetComponent<IEnemy>();
            if (newEnemy != null)
            {
                newEnemy.Initialize(playerTransform, enemyData);
            }

            return newEnemy;
        }

        public override IEnemy CreateRangedEnemy(Vector3 spawnPosition, EnemyData enemyData, Transform playerTransform)
        {
            GameObject enemyGo;

            if (ObjectPool.Instance != null)
                enemyGo = ObjectPool.Instance.Get(rangedEnemyPrefab, spawnPosition, Quaternion.identity);
            else
                enemyGo = Instantiate(rangedEnemyPrefab, spawnPosition, Quaternion.identity);

            IEnemy newEnemy = enemyGo.GetComponent<IEnemy>();
            if (newEnemy != null)
            {
                newEnemy.Initialize(playerTransform, enemyData);
            }
            return newEnemy;
        }
    }
}