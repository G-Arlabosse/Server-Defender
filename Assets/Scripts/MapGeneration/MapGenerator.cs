using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private TilesManager tilesManager;

    enum GenType {PERLIN, WORMS, VORONOI, BSP};

    [SerializeField] private GenType genType;

    [Range(10, 1000)]
    [SerializeField] private int width = 100;

    [Range(10, 1000)]
    [SerializeField] private int height = 100;

    [Range(0f, 1f)]
    [SerializeField] private float perlinTreshold = 0.5f;

    [Range(1f, 200f)]
    [SerializeField] private float scale = 10f;

    [Range(1, 20)]
    [SerializeField] private int wormWidth = 1;

    [Range(1f, 100f)]
    [SerializeField] private float wormDistTreshold = 20f;

    [Range(1, 100)]
    [SerializeField] private int nbWorms = 5;

    [Range(1, 500)]
    [SerializeField] private int wormLength = 10;

    [Range(1f, 100f)]
    [SerializeField] private float voronoiScale = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float voronoiTreshold = 0.5f;

    [Range(1, 100)]
    [SerializeField] private int BSPMaxIteration = 20;

    [Range(1, 50)]
    [SerializeField] private int BSPWallThickness = 5;

    [Range(1, 50)]
    [SerializeField] private int BSPMinimumSpace = 20;

    [Range(1, 50)]
    [SerializeField] private int BSPHoleSize = 5;

    [Range(5, 50)]
    [SerializeField] private int startRadius = 10;

    [SerializeField] private int minimumStartSpace = 3;
    private float[,] mapArray;
    private Vector2Int startPosition = Vector2Int.zero; 


    private void Start()
    {
        GenerateMap();
    }

    private void SetStartPosition()
    {
        if (mapArray == null)
        {
            Debug.LogAssertion("No map array");
            return;
        }

        if (startRadius > width/2 || startRadius > height/2)
        {
            Debug.LogAssertion("Start radius must not be greater than half the width or height");
            return;
        }

        

        Vector2Int middle = new Vector2Int(width / 2, height / 2);
        for (int i = 0; i < startRadius; i++)
        {
            for (int j = 0; j < startRadius; j++)
            {


                for (int x = -1; x < 1; x ++)
                {
                    for (int y = -1; y < 1; y ++)
                    {
                        var testX = middle.x + x * i;
                        var testY = middle.y + y * j;
                        if (CheckNeighboors(new Vector2Int(testX, testY), minimumStartSpace))
                        {
                            startPosition = new Vector2Int(testX, testY);
                            return;
                        }
                        
                    }
                }



            }
        }

        Debug.LogAssertion("No valid start position found in start radius");
        return;



    }

    /// <summary>
    /// Returns false if there are a cell weighting 0f in the radius of the pos
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private bool CheckNeighboors(Vector2Int pos, int radius)
    {
        for (int i = 0; i < radius; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                if (mapArray[pos.x + i, pos.y + j] == 0f || mapArray[pos.x - i, pos.y + j] == 0f || mapArray[pos.x + i, pos.y - j] == 0f || mapArray[pos.x - i, pos.y -j] == 0f)
                {
                    return false;
                }
            }
        }
        return true;

    }

    public void GenerateMap()
    {
        tilesManager.ResetTileMap();
        switch(genType)
        {
            case GenType.PERLIN:
                mapArray = NoiseGenerator.GetBinaryNoise(NoiseGenerator.GeneratePerlinNoise(width, height, scale), perlinTreshold);
                break;
            case GenType.WORMS:
                mapArray = NoiseGenerator.GeneratePerlinWorms(width, height, scale, wormWidth, wormDistTreshold, new Vector2Int(width / 2, height / 2), nbWorms, wormLength);
                break;
            case GenType.VORONOI:
                mapArray = NoiseGenerator.GetBinaryNoise(NoiseGenerator.GenerateVoronoiNoise(width, height, voronoiScale), voronoiTreshold);
                break;
            case GenType.BSP:
                mapArray = NoiseGenerator.GenerateBinarySpacePartition(width, height, BSPMaxIteration, BSPWallThickness, BSPMinimumSpace, BSPHoleSize);
                break;
        }

        SetStartPosition();
        print(startPosition);
        Vector3Int pos = Vector3Int.zero;
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for(int j = 0; j < mapArray.GetLength(1); j++)
            {
                pos.x = i;
                pos.y = j;
                if (startPosition != null && (i == startPosition.x && j == startPosition.y))
                {
                    tilesManager.SetTile(TilesLibrary.TileType.BASE, pos);
                } 
                else if (mapArray[i, j] == 0f)
                {
                    tilesManager.SetTile(TilesLibrary.TileType.WALL, pos);
                } else if (mapArray[i, j] == 1f)
                {
                    tilesManager.SetTile(TilesLibrary.TileType.BLANK, pos);
                }
            }
        }

    }



}
