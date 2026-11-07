using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


static public class NoiseGenerator 
{
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

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float) x / width * scale + xOffSet;
                float yCoord = (float) y / height * scale + yOffSet;

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
    static public float[,] GeneratePerlinWorms(int width, int height, float scale, int wormWidth, float distTreshold, Vector2 centerPos, int nbWorms, int wormLength)
    {

        List<Vector2> wormPoints = new List<Vector2>();
        int wormCount = 0;
        //perlin noise variables
        float[,] perlinNoise = GeneratePerlinNoise(width, height, scale);
        Vector2[,] perlinGrad = GetGradientFromNoise(perlinNoise);
        List<Vector2Int> localMaxima = GetLocalMaxima(perlinNoise);

        //get the maxima closest to the center of the map
        var startPoint = localMaxima.OrderBy(pos => Vector2.Distance(pos, centerPos)).First();
        localMaxima.Remove(startPoint);


        Vector2Int currentPosition = startPoint;
        Vector2Int destination;
        PerlinWorm worm;
        List<Vector2Int> visitedPoints = new List<Vector2Int>();
        while(wormCount < nbWorms && localMaxima.Count > 0)
        {
            if (wormCount < nbWorms / 2)
            {
                //set destination or return to startPoint
                if (currentPosition.Equals(startPoint))
                {
                    destination = localMaxima.OrderBy(pos => Vector2.Distance(pos, startPoint)).First();

                }
                if (Vector2.Distance(currentPosition, startPoint) > distTreshold)
                {
                    currentPosition = startPoint;
                    continue;
                }

                destination = localMaxima.OrderBy(pos => Vector2.Distance(pos, currentPosition)).Where(pos => Vector2.Distance(startPoint, pos) >= Vector2.Distance(currentPosition, startPoint)).First();
                if (destination == null)
                {
                    currentPosition = startPoint;
                    continue;
                }

                //create the worm
                worm = new PerlinWorm(perlinGrad, currentPosition, destination);
                
                //remove the visited position from the list
                localMaxima.Remove(currentPosition);
                visitedPoints.Add(currentPosition);
                //set current position at destination
                currentPosition = destination;
                
            }
            else
            {
                currentPosition = localMaxima.ElementAt(UnityEngine.Random.Range(0, localMaxima.Count - 1));
                destination = visitedPoints.OrderByDescending(pos => Vector2.Distance(pos, currentPosition)).Where(pos => Vector2.Distance(pos, currentPosition) < distTreshold).Any() ? visitedPoints.OrderByDescending(pos => Vector2.Distance(pos, currentPosition)).Where(pos => Vector2.Distance(pos, currentPosition) < distTreshold).First() : startPoint;
                worm = new PerlinWorm(perlinGrad, currentPosition, destination);
                localMaxima.Remove(currentPosition);
                visitedPoints.Add(currentPosition);
            }
            //get the points of the worm
            var results = worm.MoveLength(wormLength);
            //add the points to the list
            wormPoints.AddRange(results);
            wormCount++;

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

        foreach (var pos in worms)
        {

            
            int x = (int)pos.x;
            int y = (int)pos.y;
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                continue;
            }
            array[x, y] = 1f;
            for(int i = 0; i < wormWidth; i++)
            {
                if(x + i < width)
                {
                    array[x + i, y] = 1f;
                }
                if (x - i >= 0)
                {
                    array[x - i, y] = 1f;                
                }
                if (y + i < height)
                {
                    array[x, y + i] = 1f;
                }
                if (y - i >= 0)
                {
                    array[x, y - i] = 1f;
                }
            }

        }
        return array; 
    }


    /// <summary>
    /// Generate Voronoi noise
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    static public float[,] GenerateVoronoiNoise(int width, int height, float scale)
    {
        float[,] noise = new float[width, height];

        Vector2[,] points = new Vector2[Mathf.CeilToInt(scale) + 2, Mathf.CeilToInt(scale) + 2];
        int gridSize = points.GetLength(0);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                points[x, y] = new Vector2(x + UnityEngine.Random.value, y + UnityEngine.Random.value);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 uv = new Vector2((float)i / width * scale,(float) j / height * scale);
                float minDist = float.MaxValue;

                int x = (int)uv.x;
                int y = (int)uv.y;

                for (int k = -1; k < 1; k++)
                {
                    for (int l = -1; l < 1; l++)
                    {
                        int coordX = x + k;
                        int coordY = y + l;

                        if (coordX < 0 || coordX >= gridSize || coordY < 0 || coordY >= gridSize)
                        {
                            continue;
                        }
                        float dist = Vector2.Distance(uv, points[coordX, coordY]);
                        minDist = MathF.Min(minDist, dist);


                    }
                }
                float noiseValue = Mathf.Clamp01(minDist);
                noise[i, j] = noiseValue;
 
            }
        }

        return noise;
    }

    /// <summary>
    /// Create a binary space partition with holes to connect the spaces
    /// </summary>
    /// <param name="width"></param>
    /// <param name="heigth"></param>
    /// <param name="maxIteration"></param>
    /// <param name="wallThickness"></param>
    /// <param name="minimumSpace"></param>
    /// <param name="holeSize"></param>
    /// <returns></returns>
    static public float[,] GenerateBinarySpacePartition(int width, int height, int maxIteration, int wallThickness, int minimumSpace, int holeSize)
    {
        float[,] spacePartition = new float[width, height];

        List<Vector2Int> holesPositions = new List<Vector2Int>();
        List<Vector4> rectangles = new List<Vector4>();

        rectangles.Add(new Vector4(0, 0, width, height));

        float cutOrientation;
        int cutSize;
        Vector4 currentRectangle;
        for (int i = 0; i < maxIteration; i++)
        {
            currentRectangle = rectangles.ElementAt((int)UnityEngine.Random.Range(0, rectangles.Count - 1));

            cutOrientation = UnityEngine.Random.value;
            if (cutOrientation > 0.5f)
            {
                //vertical cut
                if (currentRectangle.z - wallThickness - 2 * minimumSpace < 0 )
                {
                    continue;
                }
                cutSize = (int)UnityEngine.Random.Range(minimumSpace, currentRectangle.z - wallThickness - minimumSpace);
                Vector4 rectLeft = new Vector4(currentRectangle.x, currentRectangle.y, cutSize, currentRectangle.w);
                Vector4 rectRight = new Vector4(currentRectangle.x + cutSize + wallThickness, currentRectangle.y, currentRectangle.z - cutSize - wallThickness, currentRectangle.w);
                rectangles.Remove(currentRectangle);
                rectangles.Add(rectLeft);
                rectangles.Add(rectRight);

                holesPositions.Add(new Vector2Int((int)rectLeft.x + cutSize, (int)UnityEngine.Random.Range(rectLeft.y, rectLeft.y + rectLeft.w)));

            }
            else
            {
                //horizonal cut
                if (currentRectangle.w - wallThickness - 2 * minimumSpace < 0)
                {
                    continue;
                }
                cutSize = (int)UnityEngine.Random.Range(minimumSpace, currentRectangle.w - wallThickness - minimumSpace);
                Vector4 rectUp = new Vector4(currentRectangle.x, currentRectangle.y, currentRectangle.z, cutSize);
                Vector4 rectDown = new Vector4(currentRectangle.x, currentRectangle.y + cutSize + wallThickness, currentRectangle.z, currentRectangle.w - cutSize - wallThickness);
                rectangles.Remove(currentRectangle);
                rectangles.Add(rectUp);
                rectangles.Add(rectDown);

                holesPositions.Add(new Vector2Int((int)UnityEngine.Random.Range(rectUp.x, rectUp.x + rectUp.z), (int)rectUp.y + cutSize));
            }

        }

        foreach (var rect in rectangles)
        {
            int x = (int)rect.x;
            int y = (int)rect.y;
            for (int i = 0; i < rect.z; i++)
            {
                for (int j = 0; j < rect.w; j++)
                {
                    spacePartition[x + i, y + j] = 1f;
                }
            }
        }

        foreach (var pos in holesPositions)
        {
            for (int i = 0; i < holeSize; i++)
            {
                for (int j = 0; j < holeSize; j++)
                {
                    if(pos.x + i < width && pos.y + j < height)
                    {
                        spacePartition[pos.x + i, pos.y + j] = 1f;
                    } 
                    
                }
            }
        }


        return spacePartition;
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
            for(int y = 0; y < noiseArray.GetLength(1); y++)
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
