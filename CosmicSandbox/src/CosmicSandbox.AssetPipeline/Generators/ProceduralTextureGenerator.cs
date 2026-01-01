using CosmicSandbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CosmicSandbox.AssetPipeline.Generators;

/// <summary>
/// Generates procedural textures for variety and detail
/// </summary>
public class ProceduralTextureGenerator
{
    private readonly Random _random;

    public ProceduralTextureGenerator(string? seed = null)
    {
        _random = seed != null ? new Random(seed.GetHashCode()) : new Random();
    }

    /// <summary>
    /// Generate procedural rocky surface texture
    /// </summary>
    public async Task<Image<Rgba32>> GenerateRockyTextureAsync(int width, int height, Color baseColor)
    {
        var image = new Image<Rgba32>(width, height);

        // Generate multiple octaves of Perlin noise
        var noise = GeneratePerlinNoise(width, height, 4);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float n = noise[x, y];

                // Apply color variation
                byte r = (byte)Math.Clamp(baseColor.R * (0.7f + n * 0.6f), 0, 255);
                byte g = (byte)Math.Clamp(baseColor.G * (0.7f + n * 0.6f), 0, 255);
                byte b = (byte)Math.Clamp(baseColor.B * (0.7f + n * 0.6f), 0, 255);

                image[x, y] = new Rgba32(r, g, b, 255);
            }
        }

        return image;
    }

    /// <summary>
    /// Generate procedural asteroid texture
    /// </summary>
    public async Task<Image<Rgba32>> GenerateAsteroidTextureAsync(int width, int height)
    {
        var image = new Image<Rgba32>(width, height);

        var noise = GeneratePerlinNoise(width, height, 6);
        var craters = GenerateCraters(width, height, _random.Next(5, 20));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float n = noise[x, y];
                float crater = craters[x, y];

                // Gray rocky color with crater depressions
                float value = n * 0.6f + crater * 0.4f;
                byte gray = (byte)(value * 128 + 64);

                // Add color variation (brownish-gray)
                byte r = (byte)(gray * 1.1f);
                byte g = (byte)(gray * 1.0f);
                byte b = (byte)(gray * 0.9f);

                image[x, y] = new Rgba32(
                    (byte)Math.Clamp(r, 0, 255),
                    (byte)Math.Clamp(g, 0, 255),
                    (byte)Math.Clamp(b, 0, 255),
                    255
                );
            }
        }

        return image;
    }

    /// <summary>
    /// Generate procedural gas giant bands texture
    /// </summary>
    public async Task<Image<Rgba32>> GenerateGasGiantTextureAsync(
        int width,
        int height,
        Color[] bandColors)
    {
        var image = new Image<Rgba32>(width, height);

        var turbulence = GeneratePerlinNoise(width, height, 4);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create horizontal bands
                float bandPosition = (float)y / height + turbulence[x, y] * 0.1f;
                bandPosition = (bandPosition % 1.0f + 1.0f) % 1.0f;

                int bandIndex = (int)(bandPosition * bandColors.Length);
                bandIndex = Math.Clamp(bandIndex, 0, bandColors.Length - 1);

                var color = bandColors[bandIndex];

                // Add turbulence variation
                float variation = turbulence[x, y];
                byte r = (byte)Math.Clamp(color.R * (0.8f + variation * 0.4f), 0, 255);
                byte g = (byte)Math.Clamp(color.G * (0.8f + variation * 0.4f), 0, 255);
                byte b = (byte)Math.Clamp(color.B * (0.8f + variation * 0.4f), 0, 255);

                image[x, y] = new Rgba32(r, g, b, 255);
            }
        }

        return image;
    }

    /// <summary>
    /// Generate multi-octave Perlin noise
    /// </summary>
    private float[,] GeneratePerlinNoise(int width, int height, int octaves)
    {
        var noise = new float[width, height];
        var baseNoise = GenerateWhiteNoise(width, height);

        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;

        for (int octave = 0; octave < octaves; octave++)
        {
            int frequency = 1 << octave;
            var octaveNoise = GenerateSmoothNoise(baseNoise, octave);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noise[x, y] += octaveNoise[x, y] * amplitude;
                }
            }

            totalAmplitude += amplitude;
            amplitude *= 0.5f;
        }

        // Normalize
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noise[x, y] /= totalAmplitude;
            }
        }

        return noise;
    }

    private float[,] GenerateWhiteNoise(int width, int height)
    {
        var noise = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noise[x, y] = (float)_random.NextDouble();
            }
        }

        return noise;
    }

    private float[,] GenerateSmoothNoise(float[,] baseNoise, int octave)
    {
        int width = baseNoise.GetLength(0);
        int height = baseNoise.GetLength(1);
        var smoothNoise = new float[width, height];

        int samplePeriod = 1 << octave;
        float sampleFrequency = 1.0f / samplePeriod;

        for (int y = 0; y < height; y++)
        {
            int sample_y0 = (y / samplePeriod) * samplePeriod;
            int sample_y1 = (sample_y0 + samplePeriod) % height;
            float vertical_blend = (y - sample_y0) * sampleFrequency;

            for (int x = 0; x < width; x++)
            {
                int sample_x0 = (x / samplePeriod) * samplePeriod;
                int sample_x1 = (sample_x0 + samplePeriod) % width;
                float horizontal_blend = (x - sample_x0) * sampleFrequency;

                // Bilinear interpolation
                float top = Interpolate(
                    baseNoise[sample_x0, sample_y0],
                    baseNoise[sample_x1, sample_y0],
                    horizontal_blend
                );

                float bottom = Interpolate(
                    baseNoise[sample_x0, sample_y1],
                    baseNoise[sample_x1, sample_y1],
                    horizontal_blend
                );

                smoothNoise[x, y] = Interpolate(top, bottom, vertical_blend);
            }
        }

        return smoothNoise;
    }

    private float Interpolate(float a, float b, float alpha)
    {
        return a * (1 - alpha) + b * alpha;
    }

    private float[,] GenerateCraters(int width, int height, int craterCount)
    {
        var craters = new float[width, height];

        for (int i = 0; i < craterCount; i++)
        {
            int cx = _random.Next(width);
            int cy = _random.Next(height);
            float radius = _random.Next(10, Math.Min(width, height) / 10);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance < radius)
                    {
                        float depth = 1.0f - (distance / radius);
                        craters[x, y] = Math.Min(craters[x, y] + depth * 0.5f, 1.0f);
                    }
                }
            }
        }

        return craters;
    }
}
