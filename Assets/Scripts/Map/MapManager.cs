using UnityEngine;

public class MapManager : MonoBehaviour
{

    [SerializeField] private MapCommunicant mapCommunicant;
    [SerializeField] private TilesManager tilesManager;
    [SerializeField] private TowerManager towerManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mapCommunicant == null)
        {
            mapCommunicant = GetComponent<MapCommunicant>();
            Debug.Assert(mapCommunicant != null, "MapCommunicant component is missing from the GameObject.");
        }
        if (tilesManager == null)
        {
            tilesManager = TilesManager.Instance;
        }
        if (towerManager == null)
        {
            towerManager = GetComponent<TowerManager>();
            Debug.Assert(towerManager != null, "TowerManager component is missing from the GameObject.");
        }
    }

    public bool CreateTower(Vector3 position)
    {

        // Can't create a tower if the TilesManager doesn't exist
        if (tilesManager == null) return false;
        
        if (!tilesManager.CanPlaceTower(position)) return false;

        Vector3 towerPosition = TilesManager.getCenterPosition(position);

        towerManager.CreateTower(towerPosition);

        return true;
    }

}
