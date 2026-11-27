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

    /// <summary>
    /// The base location in the game, represented as a Vector2Int. 
    /// This is the position of the base tile in the grid.
    /// </summary>
    /// <typeparam name="Vector2Int"></typeparam>
    /// <returns></returns>
    [SerializeField] public Vector2Int baseLocation = Vector2Int.zero;
    
    /// <summary>
    /// A list of enemy spawner locations in the game, represented as a list of Vector2Int.
    /// Each Vector2Int in the list corresponds to the position of an enemy spawner tile
    /// </summary>
    /// <typeparam name="Vector2Int"></typeparam>
    /// <returns></returns>
    [SerializeField] public List<Vector2Int> enemySpawnerLocation = new List<Vector2Int>();

    private string defaultTileName;

    static public TilesManager Instance { get; private set; }

    /// <summary>
    /// Calculates the Manhattan distance between two positions in a grid.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    static public int distanceInInt(Vector2Int position, Vector2Int destination)
    {
        return Math.Abs(destination.x - position.x) + Math.Abs(destination.y - position.y);
    }

    /// <summary>
    /// Calculates the Manhattan distance between two positions in a grid.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    static public int distanceInInt(Vector3 position, Vector3 destination)
    {
        Vector2Int positionInt = Vector2Int.zero;
        positionInt.x = (int) position.x;
        positionInt.y = (int) position.z;

        Vector2Int destinationInt = Vector2Int.zero;
        destinationInt.x = (int) destination.x;
        destinationInt.y = (int) destination.z;

        return distanceInInt(positionInt, destinationInt);
    }

    /// <summary>
    /// Calculates the center position of a tile in world coordinates based on its grid position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    static public Vector3 getCenterPosition(Vector2Int position)
    {
        return new Vector3(position.x + 0.5f, 0, position.y + 0.5f);
    }

    /// <summary>
    /// Calculates the center position of a tile in world coordinates based on its grid position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    static public Vector3 getCenterPosition(Vector3 position)
    {
        return new Vector3( ((int) position.x) + 0.5f, 0, ((int) position.z) + 0.5f);
    }

    /// <summary>
    /// Sets a tile at the specified grid position using the provided tile id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    public void SetTile(TilesLibrary.TileType id, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.GetTile(id).tile);
    }

    /// <summary>
    /// Sets a tile at the specified grid position using the provided tile GameObject.
    /// </summary>
    /// <param name="tileObject"></param>
    /// <param name="position"></param>
    public void SetTile(GameObject tileObject, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.createTile(tileObject, false).tile);
    }

    /// <summary>
    /// Sets multiple tiles in a rectangular area using the provided tile id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public void setTiles(TilesLibrary.TileType id, Vector2Int begin, Vector2Int end)
    {
        TilesLibrary.TileInfo tile = TilesLibrary.GetTile(id);

        for (int x = Mathf.Min(begin.x, end.x); x < Mathf.Max(begin.x, end.x); x++)
            for (int y = Mathf.Min(begin.y, end.y); y < Mathf.Max(begin.y, end.y); y++)
                tileMap.SetTile(new Vector3Int(x, y, 0), tile.tile);
        
    }

    /// <summary>
    /// Sets multiple tiles in a rectangular area using the provided tile id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="beginX"></param>
    /// <param name="beginY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    public void setTiles(TilesLibrary.TileType id, int beginX, int beginY, int endX, int endY) 
    { setTiles(id, new Vector2Int(beginX, beginY), new Vector2Int(endX, endY));}

    /// <summary>
    /// Returns the GameObject associated with the tile at the specified grid position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject getPhysicalGameObject(Vector3Int position)
    {
        return tileMap.GetTile<Tile>(position).gameObject;
    }

    /// <summary>
    /// Returns the GameObject associated with the specified tile id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a rectangle with the specified dimensions.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void CreateRectangle(int x, int y) { CreateRectangle(new Vector2Int(x, y)); }

    /// <summary>
    /// Returns the world position of the specified enemy spawner based on its index in the list.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public Vector3 getPositionSpawner(int n)
    {
        if (!(n < enemySpawnerLocation.Count))
        {
            return Vector3.zero;
        }

        Vector2Int positionInTiles = enemySpawnerLocation[n];
        
        return new Vector3(positionInTiles.x + 0.5f, 1, positionInTiles.y + 0.5f);
    }

    /// <summary>
    /// Returns the world position of the base in the game.
    /// </summary>
    /// <returns></returns>
    public Vector3 getPositionBase()
    {
        return new Vector3(baseLocation.x + 0.5f, 0, baseLocation.y + 0.5f);
    }

    private void Start()
    {
        Instance = this;

        defaultTileName = TilesLibrary.GetTile(TilesLibrary.TileType.BLANK).tile.gameObject.name;

        int x = baseLocation.x;
        int y = baseLocation.y;

        foreach (Vector2Int position in enemySpawnerLocation)
        {
            x = Mathf.Max(position.x, x);
            y = Mathf.Max(position.y, y);
        }

        CreateRectangle(x,y);
    }

    /// <summary>
    /// Checks if a tower can be placed at the specified grid position based on the tile type.
    /// A tower can only be placed on blank tiles.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool CanPlaceTower(Vector3Int position)
    {
        Tile tile = tileMap.GetTile<Tile>(position);
        if (tile == null)
        {
            return false; // Can't place tower on non-existent tiles
        }

        GameObject tileGameObject = tileMap.GetTile<Tile>(position).gameObject;

        // Check if the tile is a blank tile
        if (tileGameObject == null || tileGameObject.name != defaultTileName)
        {
            return false; // Can't place tower on non-blank tiles
        }

        tileMap.SetTile(position, TilesLibrary.GetTile(TilesLibrary.TileType.TOWER).tile);
        return true; // Can place tower on blank tiles
    }

    /// <summary>
    /// Checks if a tower can be placed at the specified world position based on the tile type.
    /// A tower can only be placed on blank tiles.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool CanPlaceTower(Vector3 position)
    {
        Vector3Int tilePosition = tileMap.WorldToCell(position);
        return CanPlaceTower(tilePosition);
    }

    /// <summary>
    /// Checks if a tower can be placed at the specified world position and places it if possible.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool CanPlaceTowerAndPlace(Vector3 position)
    {
        Vector3Int tilePosition = tileMap.WorldToCell(position);

        if (!CanPlaceTower(tilePosition))
        {
            return false; // Can't place tower on non-blank tiles
        }
        
        SetTile(TilesLibrary.TileType.TOWER, tilePosition);

        return true; // Tower placed successfully
    }
}
