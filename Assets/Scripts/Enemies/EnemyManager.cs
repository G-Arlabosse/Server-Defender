using System.Threading.Tasks;
using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private TilesManager tilesManager;
    [SerializeField] private EnemyFactory enemyFactory;

    private Vector3 baseGoal; 

    private void Start()
    {
        if (enemyFactory == null)
        {
            enemyFactory = GetComponent<EnemyFactory>();
            Assert.IsNotNull(enemyFactory);
        }

        if (tilesManager != null)
        {
            baseGoal = tilesManager.getPositionBase();
            StartCoroutine(spawner());
        }
        else
        {
            Debug.LogError("TilesManager is not assigned in EnemyManager.");
        }

        
    }


    public void SpawnEnemy()
    {
        EnemyCore newEnemy = enemyFactory.GetOne();
        newEnemy.Activate(tilesManager.getPositionSpawner(0));
        newEnemy.GoTo(baseGoal);
        newEnemy.baseLocation = baseGoal;
    }


    private IEnumerator spawner()
    {

        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy();
        }
        
    }

}
