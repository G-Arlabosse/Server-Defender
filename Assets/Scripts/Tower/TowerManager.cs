using UnityEngine;

public class TowerManager : MonoBehaviour
{
    
    [SerializeField] private MapManager mapManager;


    [SerializeField] private TowerFactory towerFactory;





    private void Start()
    {
        if (mapManager == null)
        {
            mapManager = GetComponent<MapManager>();
            Debug.Assert(mapManager != null, "MapManager component is missing from the GameObject.");
        }

        if (towerFactory == null)
        {
            towerFactory = GetComponent<TowerFactory>();
            Debug.Assert(towerFactory != null, "TowerFactory component is missing from the GameObject.");
        }
    }

    public TowerCore CreateTower(Vector3 position)
    {

        TowerCore tower = towerFactory.GetOne().GetComponent<TowerCore>();
        tower.Activate(position);
        
        return tower;
    }

}
