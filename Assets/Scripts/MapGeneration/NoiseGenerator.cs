using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoiseGenerator : MonoBehaviour
{
    Renderer renderer;
    InputAction click;

    [SerializeField] [Range(0f,1f)] float treshold = 0.5f;

    float[,] noise;
    float[,] binaryNoise;
    //for testing purpose, will be removed later
    private void Start()
    {
        click = InputSystem.actions.FindAction("Click");
        renderer = GetComponent<Renderer>();
        noise = GeneratePerlinNoise(200, 200, 20);
        binaryNoise = GetBinaryNoise(noise, treshold);
        renderer.material.mainTexture = GetTexture2DFromNoiseArray(binaryNoise);
    }

    private void Update()
    {
        if (click.WasPressedThisFrame())
        {
            renderer.material.mainTexture = GetTexture2DFromNoiseArray(GetBinaryNoise(noise,treshold));
        }
    }

    /// <summary>
    /// Generate an array of floats using Perlin Noise with Mathf.PerlinNoise function
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public float[,] GeneratePerlinNoise(int width, int height, float scale)
    {
        float[,] perlin = new float[width, height];

        float xOffSet = GenerateOffset();
        float yOffSet = GenerateOffset();

        for(int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float xCoord = (float) x / height * scale + xOffSet;
                float yCoord = (float) y / width * scale + yOffSet;

                float pix = Mathf.PerlinNoise(xCoord, yCoord);

                perlin[x,y] = pix;
            }
        }
        return perlin;
    }

    public float[,] GeneratePerlinWorms(int width, int height, float scale)
    {
        float[,] worms = new float[width, height];
        return worms;
    }

    /// <summary>
    /// Convert an array of noise value to a Texture2D in grayscale
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <returns></returns>
    public Texture2D GetTexture2DFromNoiseArray(float[,] noiseArray)
    {
        Texture2D noise = new Texture2D(noiseArray.GetLength(0), noiseArray.GetLength(1));

        for(int x = 0; x < noiseArray.GetLength(0); x++)
        {
            for(int y = 0;y < noiseArray.GetLength(1); y++)
            {
                float value = noiseArray[x,y];
                Color color = new Color(value, value, value);

                noise.SetPixel(x, y, color);
            }
        }
        noise.Apply();
        return noise;

    }

    /// <summary>
    /// Convert a noise array into a binary noise array based on the threshold
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <param name="treshold"></param>
    /// <returns></returns>
    public float[,] GetBinaryNoise(float[,] noiseArray, float treshold)
    {
        float[,] binaryNoise = new float[noiseArray.GetLength(0), noiseArray.GetLength(1)];
        for (int x = 0; x < noiseArray.GetLength(0); x++)
        {
            for (int y = 0; y < noiseArray.GetLength(1); y++)
            {
                float value = noiseArray[x, y] > treshold ? 1f : 0f;
                binaryNoise[x, y] = value;
            }
        }
        return binaryNoise;
    }

    /// <summary>
    /// Return an array with the gradient of the noise passed in argument
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <returns></returns>
    public Vector2[,] GetGradientFromNoise(float[,] noiseArray)
    {
        int width = noiseArray.GetLength(0);
        int height = noiseArray.GetLength(1);

        Vector2[,] grad = new Vector2[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                int right = Math.Max(0, x - 1);
                int left = Math.Min(width - 1, x + 1);

                int top = Math.Max(0, y - 1);
                int down = Math.Min(height - 1, y + 1);

                float dx = (noiseArray[left, y] -  noiseArray[right, y])/(left - right == 0 ? 1 : 2 );
                float dy = (noiseArray[x, top] - noiseArray[x, down])/(top - down == 0 ? 1 : 2 );

                grad[x, y] = new Vector2(dx, dy);

            }
        }

        return grad;
    }

    /// <summary>
    /// Generate a random float to add randomness to the noise generation
    /// </summary>
    /// <returns></returns>
    private float GenerateOffset()
    {
        return UnityEngine.Random.Range(0f, 999999f);
    }
}
