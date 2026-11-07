using System;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;


public class TilesManager : MonoBehaviour
{
    [SerializeField] private Tilemap tileMap;

    [SerializeField] public Vector2Int baseLocation = Vector2Int.zero;
    [SerializeField] public List<Vector2Int> enemySpawnerLocation = new List<Vector2Int>();

    static private TilesManager singleton;

    public static TilesManager get()
    { return singleton; }

    static public int distanceInInt(Vector2Int position, Vector2Int destination)
    {
        return Math.Abs(destination.x - position.x) + Math.Abs(destination.y - position.y);
    }

    static public int distanceInInt(Vector3 position, Vector3 destination)
    {
        Vector2Int positionInt = Vector2Int.zero;
        positionInt.x = (int) position.x;
        positionInt.y = (int) position.y;

        Vector2Int destinationInt = Vector2Int.zero;
        destinationInt.x = (int) destination.x;
        destinationInt.y = (int) destination.y;

        return distanceInInt(positionInt, destinationInt);
    }

    public void SetTile(TilesLibrary.TileType id, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.GetTile(id).tile);
    }

    public void SetTile(GameObject tileObject, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.createTile(tileObject, false).tile);
    }

    public void setTiles(TilesLibrary.TileType id, Vector2Int begin, Vector2Int end)
    {
        TilesLibrary.TileInfo tile = TilesLibrary.GetTile(id);

        for (int x = Mathf.Min(begin.x, end.x); x < Mathf.Max(begin.x, end.x); x++)
            for (int y = Mathf.Min(begin.y, end.y); y < Mathf.Max(begin.y, end.y); y++)
                tileMap.SetTile(new Vector3Int(x, y, 0), tile.tile);
        
    }
    public void setTiles(TilesLibrary.TileType id, int beginX, int beginY, int endX, int endY) 
    { setTiles(id, new Vector2Int(beginX, beginY), new Vector2Int(endX, endY));}

    public GameObject getPhysicalGameObject(Vector3Int position)
    {
        return tileMap.GetTile<Tile>(position).gameObject;
    }

    public GameObject getGameObject(TilesLibrary.TileType id)
    {
        return TilesLibrary.GetTile(id).tile.gameObject;
    }

    /// <summary>
    /// Create a rectangle with (0,0) as top left corner and length as bottom right corner
    /// </summary>
    /// <param name="length"> </param>
    private void CreateRectangle(Vector2Int length)
    {
        Vector3Int pos = Vector3Int.zero;
        for (int i=0; i<length.x + 1; i++)
        for (int j=0; j<length.y + 1; j++)
            {
                pos.x = i; pos.y = j;

                if (pos.x == baseLocation.x && pos.y == baseLocation.y)
                    SetTile(TilesLibrary.TileType.BASE, pos);

                else if (enemySpawnerLocation.Contains(new Vector2Int(pos.x,pos.y)))
                    SetTile(TilesLibrary.TileType.ENEMY_SPAWN, pos);

                else
                    SetTile(TilesLibrary.TileType.BLANK, pos);
            }
    }
    private void CreateRectangle(int x, int y) { CreateRectangle(new Vector2Int(x, y)); }

    public Vector3 getPositionSpawner(int n)
    {
        Assert.IsTrue(n < enemySpawnerLocation.Count);

        Vector2Int positionInTiles = enemySpawnerLocation[n];
        
        return new Vector3(positionInTiles.x + 0.5f, 1, positionInTiles.y + 0.5f);
    }

    public Vector3 getPositionBase()
    {
        return new Vector3(baseLocation.x + 0.5f, 0, baseLocation.y + 0.5f);
    }

    private void Start()
    {
        singleton = this;

        CreateRectangle(10,10);
    }
}
