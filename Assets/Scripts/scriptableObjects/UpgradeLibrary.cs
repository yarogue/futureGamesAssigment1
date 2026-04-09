using System.Collections.Generic;
using UnityEngine;

namespace scriptableObjects
{
    /// <summary>
    /// Master list of all upgrade options available in the game.
    /// Create via: Right-click Project > Player > Upgrade Library
    /// Assign UpgradeDefinition assets to the list in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeLibrary", menuName = "Player/Upgrade Library")]
    public class UpgradeLibrary : ScriptableObject
    {
        [Tooltip("All possible upgrades that can appear on level-up. Add UpgradeDefinition assets here.")]
        public List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();
    }
}
