using System;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TilesLibrary : MonoBehaviour
{
    
    /// <summary>
    /// Different type of Tiles
    /// <list type="bullet">
    /// <item>
    /// NONE -> Not to be use, default parameter
    /// </item>
    /// <item> 
    /// BLANK -> Tile with nothing on it
    /// </item>
    /// <item>
    /// WALL -> Tile with something that does nothing on it
    /// </item>
    /// </list>
    /// </summary>
    public enum TileType {NONE, BLANK, WALL}
    /// <summary>
    /// Number of type in the enum TileType above.
    /// Need to be incremented each time a new Tile type is added
    /// </summary>
    private const int MAX_TILE_TYPE = 3;

    [Serializable]
    private class TileUnit
    {
        public TileType id;
        public GameObject gameObject;
    }

    

    [SerializeField] private List<TileUnit> tiles = new List<TileUnit>();
    static private Dictionary<TileType, Tile> TileDictionary = new Dictionary<TileType, Tile>();

    private int lastNum = 0;

    static public Tile createTile(GameObject gameObject)
    {
        Tile newTile = ScriptableObject.CreateInstance<Tile>();
        newTile.gameObject = gameObject;

        return newTile;
    }

    private void OnValidate()
    {  

        int nmbTile = tiles.Count;

        if (nmbTile > MAX_TILE_TYPE)
        {
            while (tiles.Count > MAX_TILE_TYPE)
            {
                tiles.RemoveAt(MAX_TILE_TYPE);
            }
            return;
        }

        if (lastNum + 1 == nmbTile)
        {
            TileType newType = (TileType) (nmbTile - 1);
            tiles[nmbTile - 1].id = newType;
            GameObject tileGameObject = tiles[nmbTile - 1].gameObject;

            TileDictionary[newType] = createTile(tileGameObject);

        }
        else
        {
            TileDictionary = new Dictionary<TileType, Tile>();
            foreach (TileUnit tileUnit in tiles)
            {
                TileDictionary[tileUnit.id] = createTile(tileUnit.gameObject);
            }
        }

        lastNum = nmbTile;
    }   

    static public Tile GetTile(TileType id)
    {
        Assert.IsTrue(TileDictionary.ContainsKey(id));

        return TileDictionary[id];
    }
}
