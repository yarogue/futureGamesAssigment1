using UnityEngine;

namespace scriptableObjects
{
    [CreateAssetMenu(fileName = "Biome Preset", menuName = "New biome preset")]
    public class BiomePreset : ScriptableObject
    {
        public Color debugColor;
        public Sprite[] tileSprites;

        public float minHeight,
                     minMoisture,
                     minHeat;

        public bool MatchCondition(float height, float moisture, float heat)
        {
            return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
        }

        public Sprite GetTileSprite()
        {
            return tileSprites[Random.Range(0, tileSprites.Length)];
        }
    }
}
