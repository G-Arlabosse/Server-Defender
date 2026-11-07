using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;


static public class NoiseGenerator 
{
    //Renderer renderer;
    //InputAction click;

    //[SerializeField] [Range(0f,1f)] float treshold = 0.5f;

    //float[,] noise;
    //float[,] binaryNoise;
    ////for testing purpose, will be removed later
    //private void Start()
    //{
    //    click = InputSystem.actions.FindAction("Click");
    //    renderer = GetComponent<Renderer>();
    //    noise = GeneratePerlinNoise(200, 200, 20);
    //    binaryNoise = GetBinaryNoise(noise, treshold);
    //    renderer.material.mainTexture = GetTexture2DFromNoiseArray(binaryNoise);
    //}

    //private void Update()
    //{
    //    if (click.WasPressedThisFrame())
    //    {
    //        renderer.material.mainTexture = GetTexture2DFromNoiseArray(GetBinaryNoise(noise,treshold));
    //    }
    //}

    /// <summary>
    /// Generate an array of floats using Perlin Noise with Mathf.PerlinNoise function
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    static public float[,] GeneratePerlinNoise(int width, int height, float scale)
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

    
    /// <summary>
    /// Creates and returns the array with Perlin Worms, converging to the center of the array
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <param name="wormWidth"></param>
    /// <param name="distTreshold"></param>
    /// <returns></returns>
    static public float[,] GeneratePerlinWorms(int width, int height, float scale, int wormWidth, float distTreshold)
    {

        List<Vector2> wormPoints = new List<Vector2>();

        //perlin noise variables
        float[,] perlinNoise = GeneratePerlinNoise(width, height, scale);
        Vector2[,] perlinGrad = GetGradientFromNoise(perlinNoise);
        List<Vector2Int> localMaxima = GetLocalMaxima(perlinNoise);

        //get the maxima closest to the center of the map
        Vector2 center = new Vector2((float)width / 2, (float)height / 2);
        var startPoint = localMaxima.OrderBy(pos => Vector2.Distance(pos, center)).First();
        localMaxima.Remove(startPoint);


        Vector2Int currentPosition = startPoint;
        Vector2Int destination;
        int wormLength;
        PerlinWorm worm;
        
        while(localMaxima.Count > 0)
        {
            //set destination or return to startPoint
            if(currentPosition.Equals(startPoint))
            {
                destination = localMaxima.OrderBy(pos => Vector2.Distance(pos, startPoint)).First();
            } else
            {
                destination = localMaxima.OrderBy(pos => Vector2.Distance(pos, currentPosition)).First();
                if (Vector2.Distance(currentPosition, destination) > distTreshold)
                {
                    currentPosition = startPoint;
                    continue;
                }
            }
            //chose an adapted length for the worm
            wormLength = (int)Vector2.Distance(currentPosition, destination) * 2 ;
            //create the worm
            worm = new PerlinWorm(perlinGrad, currentPosition, destination);
            //get the points of the worm
            var results = worm.MoveLength(wormLength);
            //add the points to the list
            wormPoints.AddRange(results);
            //remove the destination from the list
            localMaxima.Remove(destination);
            //set current position at destination
            currentPosition = destination;

        }

        //place the points in the array
        float[,] worms = PlaceWorms(wormPoints, wormWidth, width, height);

        return worms;
    }

    /// <summary>
    /// Return an array with witdhen worms (1f : worm, 0f : nothing)
    /// </summary>
    /// <param name="worms"></param>
    /// <param name="wormWidth"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static private float[,] PlaceWorms(List<Vector2> worms, int wormWidth, int width, int height)
    {
        float[,] array = new float[width, height];

        int randomWidth;
        foreach (var pos in worms)
        {

            randomWidth = UnityEngine.Random.Range(width, width + width / 2);
            for(int i = 0; i < randomWidth; i++)
            {
                for (int j = 0; j < randomWidth; j++)
                {
                    int x = (int)pos.x + i;
                    int y = (int)pos.y + j;
                    if (x < 0 || x >= width || y < 0 || y >= height)
                    {
                        continue;
                    }
                    array[x, y] = 1f;
                }
            }

        }
        return array; 
    }

    /// <summary>
    /// Convert an array of noise value to a Texture2D in grayscale
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <returns></returns>
    static public Texture2D GetTexture2DFromNoiseArray(float[,] noiseArray)
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
    static public float[,] GetBinaryNoise(float[,] noiseArray, float treshold)
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
    static public Vector2[,] GetGradientFromNoise(float[,] noiseArray)
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
    static private float GenerateOffset()
    {
        return UnityEngine.Random.Range(0f, 999999f);
    }

    /// <summary>
    /// Return the list of the local maxima of the noise array
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <returns></returns>
    static public List<Vector2Int> GetLocalMaxima(float[,] noiseArray)
    {
        List<Vector2Int> maxima = new List<Vector2Int>();

        for (int i = 0; i < noiseArray.GetLength(0); i++)
        {
            for(int j = 0; j < noiseArray.GetLength(1); j++)
            {
                var noiseVal = noiseArray[i, j];
                if(TestNeighbours(i, j, noiseArray, (neighbourNoise) => neighbourNoise > noiseVal))
                {
                    maxima.Add(new Vector2Int(i, j));
                }
            }
        }

        return maxima;
    }

    /// <summary>
    /// Return a list of the local minima of the noise array
    /// </summary>
    /// <param name="noiseArray"></param>
    /// <returns></returns>
    static public List<Vector2Int> GetLocalMinima(float[,] noiseArray)
    {
        List<Vector2Int> minima = new List<Vector2Int>();

        for(int i = 0; i < noiseArray.GetLength(0); i++)
        {
            for (int j = 0; j < noiseArray.GetLength(1); j++)
            {
                var noiseVal = noiseArray[i, j];
                if (TestNeighbours(i, j,noiseArray, (neighboursNoise) => neighboursNoise < noiseVal))
                {
                    minima.Add(new Vector2Int(i, j));
                }
            }
        }

        return minima;
    }


    /// <summary>
    /// List of all directions in Vector2Int
    /// </summary>
    static List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int( 0, 1), //N
        new Vector2Int( 1, 1), //NE
        new Vector2Int( 1, 0), //E
        new Vector2Int(-1, 1), //SE
        new Vector2Int(-1, 0), //S
        new Vector2Int(-1,-1), //SW
        new Vector2Int( 0,-1), //W
        new Vector2Int( 1,-1)  //NW
    };

    /// <summary>
    /// Test if the neighbours of the position all verifiy the negative of failCondition 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="array"></param>
    /// <param name="testFunction"></param>
    /// <returns></returns>
    static private bool TestNeighbours(int x, int y , float[,] array, Func<float, bool> failCondition)
    {
        foreach (var dir in directions)
        {
            var newCoord = new Vector2Int(x + dir.x, y + dir.y);
            
            //test if the new coord is in the array
            if(newCoord.x < 0 || newCoord.x >= array.GetLength(0) || newCoord.y < 0 || newCoord.y >= array.GetLength(1))
            {
                continue;
            }

            //if the new coord is in the array we test the array value
            if (failCondition(array[newCoord.x, newCoord.y])) {
                return false;
            }

        }
        return true;

    }
}
