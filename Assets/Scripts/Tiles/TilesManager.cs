using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesManager : MonoBehaviour
{
    
    [SerializeField] private Tilemap tileMap;

    private Tile tileBlank;
    private Tile tileWall;

    [SerializeField] private GameObject TileBlankObject;
    [SerializeField] private GameObject TileWallObject;

    public enum TileType {NONE, BLANK, WALL}

    public Tile GetTile(TileType id)
    {
        switch (id)
        {
            case TileType.BLANK :
                return tileBlank;

            case TileType.WALL :
                return tileWall;

        }

        return null;
    }


    private void Start()
    {
        tileBlank = ScriptableObject.CreateInstance<Tile>();
        tileBlank.gameObject = TileBlankObject;

        tileWall = ScriptableObject.CreateInstance<Tile>();
        tileWall.gameObject = TileWallObject;
    }
}
