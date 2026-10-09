using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TestTilesManager : MonoBehaviour
{

    [SerializeField] private Tilemap tileMap;

    [SerializeField] private GameObject objectTile1;
    [SerializeField] private GameObject objectTile2;

    private Tile tile1;
    private Tile tile2;

    private Vector3Int location = Vector3Int.zero;


 private void Start()
 {
    tile1 = ScriptableObject.CreateInstance<Tile>();
    tile1.gameObject = objectTile1;

    tile2 = ScriptableObject.CreateInstance<Tile>();
    tile2.gameObject = objectTile2;
 }

 // Update is called once per frame
 void Update()
    {
        tileMap.SetTile(location, tile1);
        tileMap.SetTile(location + new Vector3Int(0,1), tile2);
        tileMap.SetTile(location + new Vector3Int(0,-1), tile2);
        tileMap.SetTile(location + new Vector3Int(1,0), tile2);
    }
}
