using System.Collections.Generic;
using scriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


namespace proceduralMaps
{
    public enum MapDisplay
    {
        Height,
        Moisture,
        Heat,
        Biome
    }

    public class Map : MonoBehaviour
    {
        public MapDisplay displayType;
        public BiomePreset[]  biomes;
        public GameObject tilePrefab;
        
        [SerializeField]
        private RawImage debugImage;
        
        [Header("Dimensions")]
        public int width;
        public int height;
        public float scale;
        public Vector2 offset;

        [Header("Height map")]
        public Wave[] heightWaves;
        public Gradient heightDebugColors;
        public float[,] HeightMap;
        
        [Header("Moisture map")]
        public Wave[] moistureWaves;
        public Gradient moistureDebugColors;
        public float[,] MoistureMap;
        
        [Header("Heat map")]
        public Wave[] heatWaves;
        public Gradient heatDebugColors;
        public float[,] HeatMap;
        

        public void GenerateMap()
        {
            //Generating the height map
            HeightMap = NoiseGenerator.Generate(width, height, scale, offset, heightWaves);
            
            //Generating the moisture map
            MoistureMap = NoiseGenerator.Generate(width, height, scale, offset, moistureWaves);
            
            //Generating the heat map
            HeatMap = NoiseGenerator.Generate(width, height, scale, offset, heatWaves);
            
            var pixels = new Color[width * height];
            var i = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    switch (displayType)
                    {
                        case MapDisplay.Height:
                            pixels[i] = heightDebugColors.Evaluate(HeightMap[x, y]);
                            break;
                        case MapDisplay.Moisture:
                            pixels[i] = moistureDebugColors.Evaluate(MoistureMap[x, y]);
                            break;
                        case MapDisplay.Heat:
                            pixels[i] = heatDebugColors.Evaluate(HeatMap[x, y]);
                            break;

                        case MapDisplay.Biome:
                        {
                            var biome = GetBiome(HeightMap[x, y], MoistureMap[x, y], HeatMap[x, y]);
                            pixels[i] = biome.debugColor;

                            // We're now instantiating tiles relative to the chunk's origin
                            var tile = Instantiate(tilePrefab, new Vector3(x + offset.x, y + offset.y, 0), Quaternion.identity, transform);
                            tile.GetComponent<SpriteRenderer>().sprite = biome.GetTileSprite();
                            
                            break;
                        }
                        
                        default:
                            break;
                    }

                    i++;
                    
                }
            }
            
            if (Application.isEditor)
            {
                var tex = new Texture2D(width, height);
                tex.SetPixels(pixels);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                
                if (debugImage != null)
                {
                    debugImage.texture = tex;
                }
            }
        }

        BiomePreset GetBiome(float height, float moisture, float heat)
        {
            BiomePreset biomeToReturn = null;
            var tempBiomes = new List<BiomePreset>();

            foreach (var biome in biomes)
            {
                if (biome.MatchCondition(height, moisture, heat))
                {
                    tempBiomes.Add(biome);
                }
            }

            var curValue = 0.0f;

            foreach (var biome in tempBiomes)
            {
                var diffValue = (height - biome.minHeight) + (moisture - biome.minMoisture) +  (heat - biome.minHeat);

                if (biomeToReturn == null)
                {
                    biomeToReturn = biome;
                    curValue = diffValue;
                }
                else if (diffValue < curValue)
                {
                    biomeToReturn = biome;
                    curValue = diffValue;
                }
            }

            if (biomeToReturn == null)
                return biomes[0];
            
            return biomeToReturn;
        }
    }
}
