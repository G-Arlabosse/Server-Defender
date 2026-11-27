using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class ShooterAreaManager : MonoBehaviour
{

    [SerializeField] private TowerCore towerCore;


    /// <summary>
    /// The time between two shoot
    /// </summary>
    private float fireRate = 1;
    /// <summary>
    /// Decrease until next shoot
    /// </summary>
    private float reloadTime = 0;

    private Factory<Projectile> projectileFactory;

    private void Start()
    {
        if (towerCore == null)
        {
            towerCore = GetComponent<TowerCore>();

            Assert.IsNotNull(towerCore);
        }

    }


    void Update()
    {


        if (towerCore.targets.Count != 0 && reloadTime == 0)
        {
            foreach(EnemyCore target in towerCore.targets)
            {
                target.getHit(2);
            }
            reloadTime = fireRate;
        }
        else if (reloadTime != 0)
        {
            reloadTime -= Time.deltaTime;
            reloadTime = Mathf.Max(0, reloadTime);
        }
    }
}
