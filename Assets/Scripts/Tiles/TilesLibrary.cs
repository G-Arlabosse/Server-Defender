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
    public enum TileType {NONE, BLANK, WALL, BASE, ENEMY_SPAWN}
    /// <summary>
    /// Number of type in the enum TileType above.
    /// Need to be incremented each time a new Tile type is added
    /// </summary>
    private const int MAX_TILE_TYPE = 5;

    [Serializable]
    private class TileUnitInput
    {
        public TileType id;
        public GameObject gameObject;
        public bool canBeCross;
    }
    public class TileInfo
    {
        public Tile tile;

        public bool canBeCross;
    }

    

    [SerializeField] private List<TileUnitInput> tiles = new List<TileUnitInput>();
    static private Dictionary<TileType, TileInfo> TileDictionary = new Dictionary<TileType, TileInfo>();

    private int lastNum = 0;

    static public TileInfo createTile(GameObject gameObject, bool canBeCross)
    {
        TileInfo newTile = new TileInfo();

        newTile.tile = ScriptableObject.CreateInstance<Tile>();
        newTile.tile.gameObject = gameObject;

        newTile.canBeCross = canBeCross;

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
            bool newCanBeCross = tiles[nmbTile - 1].canBeCross;

            TileDictionary[newType] = createTile(tileGameObject, newCanBeCross);

        }
        else
        {
            TileDictionary = new Dictionary<TileType, TileInfo>();
            foreach (TileUnitInput tileUnit in tiles)
            {
                if (tileUnit.id == TileType.NONE)
                    tileUnit.gameObject = null;

                TileDictionary[tileUnit.id] = createTile(tileUnit.gameObject, tileUnit.canBeCross);
            }
        }

        lastNum = nmbTile;
    }   

    static public TileInfo GetTile(TileType id)
    {
        Assert.IsTrue(TileDictionary.ContainsKey(id));

        return TileDictionary[id];
    }
}
