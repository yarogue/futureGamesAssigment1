using generalScripts;
using UnityEngine;
using System;
using generalScripts.Managers;

namespace MainCharacterScripts
{
    public class PlayerLevelController : MonoBehaviour
    {
        public static PlayerLevelController Instance {get; private set;}

        /// <summary>
        /// Fired every time the player gains a level.
        /// GameUIManager subscribes to show the upgrade picker panel.
        /// </summary>
        public static event Action OnLevelUp;
        
        [Header("Current stats")]
        [SerializeField]
        private int currentLevel = 0;
        [SerializeField]
        private float currentXp = 0;
        
        private GameUIManager _uiManager;

        private const float BaseXp = 5f;

        private const float XpPower = 2f;

        private float _xpToNextLevel,
                      _xpInCurrentLevel;

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

        private void Start()
        {
            if (_uiManager == null)
            {
                _uiManager = FindObjectOfType<GameUIManager>();
            }
            
            _xpToNextLevel = GetRequiredXpForLevel(currentLevel);
            //UpdateUI();
        }

        private float GetRequiredXpToReachLevel(int level)
        {
            if (level <= 0) return 0f;
            
            return BaseXp * (float)Math.Pow(level, XpPower);
        }

        private float GetRequiredXpForLevel(int level)
        {
            var xpToStartNext = GetRequiredXpToReachLevel(level + 1);
            var xpToStartCurrent = GetRequiredXpToReachLevel(level);
            return xpToStartCurrent + xpToStartNext;
        }
        
        public void AddExperience(float amount)
        {
            if (amount <= 0) return;
            
            currentXp += amount;
            _xpInCurrentLevel += amount;
            
            Debug.Log($"Gained {amount} XP. Total XP: {currentXp}");

            CheckForLevelUp();
            UpdateUI();
        }

        private void CheckForLevelUp()
        {
            if (_xpInCurrentLevel >= _xpToNextLevel)
            {
                var remainingXp = _xpInCurrentLevel - _xpToNextLevel;
                
                currentLevel++;
                
                Debug.Log($"Level {currentLevel} has been upgraded.");

                // Notify listeners (GameUIManager will show upgrade picker)
                OnLevelUp?.Invoke();
                
                _xpToNextLevel = GetRequiredXpForLevel(currentLevel);
                
                _xpInCurrentLevel = remainingXp;
                
                CheckForLevelUp();
            }
        }

        private void UpdateUI()
        {
            if (_uiManager == null) return;
            
            var progression = _xpInCurrentLevel / _xpToNextLevel;
            
            _uiManager.UpdateLevelDisplay
            (
                currentLevel,
                progression,
                (int) _xpInCurrentLevel,
                (int) _xpToNextLevel
            );
        }
    }
}
