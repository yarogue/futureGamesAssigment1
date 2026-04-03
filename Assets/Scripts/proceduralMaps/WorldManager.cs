using System;
using System.Collections.Generic;
using UnityEngine;
using MainCharacterScripts;

namespace proceduralMaps
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;

        [Header("References")]
        [SerializeField] private GameObject chunkPrefab;

        [Header("Settings")]
        public int chunkWidth;
        public int chunkHeight;
        public int viewDistance;

        private Transform _playerTransform;
        private Vector2Int _currentPlayerChunk;
        private readonly Dictionary<Vector2Int, GameObject> _loadedChunks = new Dictionary<Vector2Int, GameObject>();

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
            var newPlayer = FindObjectOfType<PlayerController>();
            if (newPlayer != null)
            {
                ResetWorld(newPlayer.transform);
            }
        }

        [Obsolete("Obsolete")]
        private void Start()
        {
            var newPlayer = FindObjectOfType<PlayerController>();
            if (newPlayer != null)
            {
                ResetWorld(newPlayer.transform);
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                return;
            }

            var newChunk = GetChunkCoordinates(_playerTransform.position);
            if (newChunk != _currentPlayerChunk)
            {
                _currentPlayerChunk = newChunk;
                GenerateWorld();
            }
        }

        public void ResetWorld(Transform newPlayerTransform)
        {
            foreach (var chunk in _loadedChunks.Values)
            {
                Destroy(chunk);
            }
            _loadedChunks.Clear();

            _playerTransform = newPlayerTransform;
            if (_playerTransform == null)
            {
                return;
            }

            _currentPlayerChunk = GetChunkCoordinates(_playerTransform.position);
            GenerateWorld();
        }

        private void GenerateWorld()
        {
            var chunksToKeep = new HashSet<Vector2Int>();

            for (var x = -viewDistance; x <= viewDistance; x++)
            {
                for (var y = -viewDistance; y <= viewDistance; y++)
                {
                    var chunkCoords = new Vector2Int(_currentPlayerChunk.x + x, _currentPlayerChunk.y + y);
                    chunksToKeep.Add(chunkCoords);

                    if (!_loadedChunks.ContainsKey(chunkCoords))
                    {
                        GenerateChunk(chunkCoords);
                    }
                }
            }

            var chunksToUnload = new List<Vector2Int>();
            foreach (var chunk in _loadedChunks)
            {
                if (!chunksToKeep.Contains(chunk.Key))
                {
                    chunksToUnload.Add(chunk.Key);
                }
            }

            foreach (var chunkCoords in chunksToUnload)
            {
                UnloadChunk(chunkCoords);
            }
        }

        private void GenerateChunk(Vector2Int coords)
        {
            if (chunkPrefab == null)
            {
                return;
            }

            var chunkInstance = Instantiate(chunkPrefab, new Vector3(coords.x * chunkWidth, coords.y * chunkHeight, 0), Quaternion.identity);
            chunkInstance.name = $"Chunk_{coords.x}_{coords.y}";

            var mapComponent = chunkInstance.GetComponent<Map>();
            if (mapComponent == null)
            {
                return;
            }

            mapComponent.width = chunkWidth;
            mapComponent.height = chunkHeight;
            mapComponent.offset = new Vector2(coords.x * chunkWidth, coords.y * chunkHeight);

            _loadedChunks.Add(coords, chunkInstance);

            mapComponent.GenerateMap();
        }

        private void UnloadChunk(Vector2Int coords)
        {
            if (_loadedChunks.TryGetValue(coords, out var chunk))
            {
                Destroy(chunk);
                _loadedChunks.Remove(coords);
            }
        }

        private Vector2Int GetChunkCoordinates(Vector3 position)
        {
            var chunkX = Mathf.FloorToInt(position.x / chunkWidth);
            var chunkY = Mathf.FloorToInt(position.y / chunkHeight);
            return new Vector2Int(chunkX, chunkY);
        }
    }
}