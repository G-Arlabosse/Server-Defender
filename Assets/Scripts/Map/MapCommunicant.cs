using UnityEngine;

public class MapCommunicant : MonoBehaviour
{

    [SerializeField] private MapManager mapManager;

    private void Start()
    {
        if (mapManager == null)
        {
            mapManager = GetComponent<MapManager>();
            Debug.Assert(mapManager != null, "MapManager component is missing from the GameObject.");
        }
    }

    public bool CreateTower(Vector3 position)
    {

        
        return mapManager.CreateTower(position);
    }
}