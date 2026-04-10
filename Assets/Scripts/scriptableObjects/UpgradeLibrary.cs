using System.Collections.Generic;
using UnityEngine;

namespace scriptableObjects
{
    [CreateAssetMenu(fileName = "UpgradeLibrary", menuName = "Player/Upgrade Library")]
    public class UpgradeLibrary : ScriptableObject
    {
        [Tooltip("All possible upgrades that can appear on level-up. Add UpgradeDefinition assets here.")]
        public List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();
    }
}
