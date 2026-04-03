using System.Collections.Generic;
using UnityEngine;

namespace generalScripts
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            int key = prefab.GetInstanceID();

            if (_pools.TryGetValue(key, out var pool) && pool.Count > 0)
            {
                var obj = pool.Dequeue();
                
                // Skip destroyed objects
                if (obj == null)
                {
                    return Instantiate(prefab, position, rotation);
                }
                
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }

            return Instantiate(prefab, position, rotation);
        }
        
        public void ReturnToPool(GameObject prefab, GameObject instance)
        {
            int key = prefab.GetInstanceID();

            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }

            instance.SetActive(false);
            _pools[key].Enqueue(instance);
        }
        
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            _pools.Clear();
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ClearAll();
        }
    }
}
