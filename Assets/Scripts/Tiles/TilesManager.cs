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


    public void SetTile(TilesLibrary.TileType id, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.GetTile(id));
    }

    public void SetTile(GameObject tileObject, Vector3Int position)
    {
        tileMap.SetTile(position, TilesLibrary.createTile(tileObject));
    }

    public void setTiles(TilesLibrary.TileType id, Vector2Int begin, Vector2Int end)
    {
        Tile tile = TilesLibrary.GetTile(id);

        for (int x = Mathf.Min(begin.x, end.x); x < Mathf.Max(begin.x, end.x); x++)
            for (int y = Mathf.Min(begin.y, end.y); y < Mathf.Max(begin.y, end.y); y++)
                tileMap.SetTile(new Vector3Int(x, y, 0), tile);
        
    }
    public void setTiles(TilesLibrary.TileType id, int beginX, int beginY, int endX, int endY) 
    { setTiles(id, new Vector2Int(beginX, beginY), new Vector2Int(endX, endY));}

    public GameObject getPhysicalGameObject(Vector3Int position)
    {
        return tileMap.GetTile<Tile>(position).gameObject;
    }

    public GameObject getGameObject(TilesLibrary.TileType id)
    {
        return TilesLibrary.GetTile(id).gameObject;
    }


    private void Start()
    {
        setTiles(TilesLibrary.TileType.BLANK, 0, 0, 4, 4);
    }
}
