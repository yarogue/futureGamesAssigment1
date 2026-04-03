using UnityEngine;

namespace proceduralMaps
{
    public class NoiseGenerator : MonoBehaviour
    {
        public static float[,] Generate(int width, int height, float scale, Vector2 offset, Wave[] waves)
        {
            var noiseMap = new float[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    float sampleX = (float)x * scale + offset.x,
                          sampleY = (float)y * scale + offset.y,
                          normalization = 0.0f;
                    foreach (var wave in waves)
                    {
                        float waveSamplePosX = sampleX * wave.frequency + wave.seed,
                              waveSamplePosY = sampleY * wave.frequency + wave.seed;
                        
                        noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(waveSamplePosX, waveSamplePosY);
                        normalization += wave.amplitude;
                    }
                    noiseMap[x, y] /= normalization;
                }
            }
            return noiseMap;
        }
    }
    [System.Serializable]
    public class Wave
    {
        public float seed, frequency, amplitude;
        
    }
}