using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FlowManager : MonoBehaviour
{

    private List<Vector3Int> targets = new List<Vector3Int>();
    private HashSet<Vector3Int> obstacles = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, int> distanceMapMelee = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, int> distanceMapFlying = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, int> distanceMapRanged = new Dictionary<Vector3Int, int>();
    private Vector3Int[] directions = {
        Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left,
        new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)};
    private int newDist;
    private int currentDist;
    private int stepCost;
    private int bestIndex;
    public Tilemap gridTilemap;
    public TileBase targetTile;
    public TileBase wallTile;
    //I'm tired boss
    public int rangedTileRange = 1;
    void Start()
    {
        //Au cas où
        newDist = 1;
        currentDist = 1;
        stepCost = 1;
        bestIndex = 0;

    }

    void Update()
    {

    }

    public void AddTarget(Vector3 worldPos)
    {
        Vector3Int cellPos = gridTilemap.WorldToCell(worldPos);
        if (!targets.Contains(cellPos) && !obstacles.Contains(cellPos))
        {
            targets.Add(cellPos);
            gridTilemap.SetTile(cellPos, targetTile);
            CalculateAllFlows();
        }
    }

    public void AddWall(Vector3 worldPos)
    {
        Vector3Int cellPos = gridTilemap.WorldToCell(worldPos);
        if (!obstacles.Contains(cellPos) && !targets.Contains(cellPos))
        {
            obstacles.Add(cellPos);
            gridTilemap.SetTile(cellPos, wallTile);
            CalculateAllFlows();
        }
    }

    void CalculateAllFlows()
    {
        //Melee
        RunBFS(EnemyType.Melee, distanceMapMelee);
        //Flying
        RunBFS(EnemyType.Flying, distanceMapFlying);
        //Ranged
        RunBFS(EnemyType.Ranged, distanceMapRanged);
    }

    void RunBFS(EnemyType type, Dictionary<Vector3Int, int> map)
    {
        map.Clear();
        List<Vector3Int> queue = new List<Vector3Int>();
        bool isFlying = (type == EnemyType.Flying);
        int radius = (type == EnemyType.Ranged) ? rangedTileRange - 1 : 0;
        foreach (var target in targets)
        {
        //Gérer le ranged case
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Vector3Int t = target + new Vector3Int(dx, dy, 0);
                    if (!isFlying && obstacles.Contains(t)) continue;
                    if (!map.ContainsKey(t)) { queue.Add(t); map[t] = 0; }
                }
        }
        gridTilemap.CompressBounds();
        BoundsInt bounds = gridTilemap.cellBounds;
//Cpabien Djikstra multi source
        while (queue.Count > 0)
        {
            bestIndex = 0;
            //Clever. On prend le point avec le poids ? minimal et on regarde ses voisins.
            for (int i = 1; i < queue.Count; i++)
            {
                if (map[queue[i]] < map[queue[bestIndex]])
                {
                    bestIndex = i;
                }
            }
            Vector3Int current = queue[bestIndex];
            //On l'prend de la queue pour ensuite continuer
            queue.RemoveAt(bestIndex);
            currentDist = map[current];
            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                stepCost = (dir.x != 0 && dir.y != 0) ? 14 : 10;
                if (!bounds.Contains(neighbor) || !gridTilemap.HasTile(neighbor)) continue;
                if (!isFlying)
                { 
                    if (obstacles.Contains(neighbor)) continue;
                    //Diag issue, solution conne g pa le temps 
                    if (dir.x != 0 && dir.y != 0)
                    {
                        if (obstacles.Contains(current + new Vector3Int(dir.x, 0, 0)) ||
                            obstacles.Contains(current + new Vector3Int(0, dir.y, 0)))
                        {continue;}
                    }
                }
                newDist = currentDist + stepCost;
                if (!map.ContainsKey(neighbor) || newDist < map[neighbor])
                {
                    map[neighbor] = newDist;
                    if(!queue.Contains(neighbor))
                    {
                        queue.Add(neighbor); //COME HERE !! crochet de Chopper
                    }
                }
            }
        }
    }

    public Vector3 GetMoveDirection(Vector3 worldPos, EnemyType type)
    {
        Vector3Int cellPos = gridTilemap.WorldToCell(worldPos);
        Dictionary<Vector3Int, int> activeMap = 
            (type == EnemyType.Flying) ? distanceMapFlying : 
            (type == EnemyType.Ranged) ? distanceMapRanged : distanceMapMelee;
        if (!activeMap.ContainsKey(cellPos)) { return Vector3.zero; }

        
        //Deprecated you bozo !!!
        //le 30 est par rapport au prix, on peut changer (SerializeField !!) Actuellement inutile en sah
        if (type == EnemyType.Ranged && activeMap[cellPos] <= 1)
        { return Vector3.zero;}
        

        currentDist = activeMap[cellPos];
        Vector3Int bestNeighbor = cellPos;

        foreach (var dir in directions)
        {
            Vector3Int neighbor = cellPos + dir;
            if (!activeMap.ContainsKey(neighbor)) continue;
            //Diag issue, solution conne g pa le temps 
            if (type != EnemyType.Flying && dir.x != 0 && dir.y != 0)
            {
                if (obstacles.Contains(cellPos + new Vector3Int(dir.x, 0, 0)) ||
                    obstacles.Contains(cellPos + new Vector3Int(0, dir.y, 0)))
                {continue;}
            }
            if (activeMap[neighbor] < currentDist)
            {
                currentDist = activeMap[neighbor];
                bestNeighbor = neighbor;
            }
        }
        if (bestNeighbor == cellPos) { return Vector3.zero; }
        return (gridTilemap.GetCellCenterWorld(bestNeighbor) - worldPos).normalized;
    }
}