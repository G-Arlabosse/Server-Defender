using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoiseGenerator : MonoBehaviour
{
    Renderer renderer;
    InputAction click;
    //for testing purpose, will be removed later
    private void Start()
    {
        click = InputSystem.actions.FindAction("Click");
        renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GeneratePerlinNoise(200, 200, 20);
    }

    private void Update()
    {
        if(click.WasPressedThisFrame())
        {
            renderer.material.mainTexture = GeneratePerlinNoise(200, 200, 20);
        }
    }

    /// <summary>
    /// Generate a greyscale 2D Texture using Perlin Noise with Mathf.PerlinNoise function
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public Texture2D GeneratePerlinNoise(int width, int height, float scale)
    {
        Texture2D perlin = new Texture2D(width, height);

        float xOffSet = GenerateOffset();
        float yOffSet = GenerateOffset();

        for(int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float xCoord = (float) x / height * scale + xOffSet;
                float yCoord = (float) y / width * scale + yOffSet;

                float pix = Mathf.PerlinNoise(xCoord, yCoord);

                Color color = new Color(pix,pix, pix);

                perlin.SetPixel(x , y , color);
            }
        }
        perlin.Apply();
        return perlin;
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
