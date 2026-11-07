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

        baseGoal = tilesManager.getPositionBase();

        StartCoroutine(spawner());

        
    }


    public void SpawnEnemy()
    {


        EnemyCorp newEnemy = enemyFactory.getOne();
        newEnemy.Activate(tilesManager.getPositionSpawner(0));
        newEnemy.GoTo(baseGoal);
        newEnemy.baseLocation = baseGoal;
    }


    private IEnumerator spawner()
    {

        while(true)
        {
            print("spawn in 2");
            yield return new WaitForSeconds(2);
            print("spawn in 0");
            SpawnEnemy();
        }
        
    }

}
