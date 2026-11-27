using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class ShooterManager : MonoBehaviour
{

    [SerializeField] private TowerCore towerCore;

    /// <summary>
    /// Can't be null !
    /// </summary>
    [SerializeField] private GameObject projectile;
    /// <summary>
    /// Can be null. If it's null, the tower shoots from the center of it gameObject.
    /// </summary>
    [SerializeField] private GameObject head;
    private Vector3 shootFrom;



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

        Assert.IsNotNull(projectile);
        projectileFactory = Factory<Projectile>.instance;

        if (head == null)
            shootFrom = transform.position;
        else
            shootFrom = head.transform.position;

    }

    /// <summary>
    /// Call when need to shoot the next bullet
    /// </summary>
    private void shoot(EnemyCorp target)
    {
        reloadTime = fireRate;
        Projectile new_proj = projectileFactory.GetOne();
        new_proj.Activate(target, shootFrom);
    }

    void Update()
    {
        EnemyCorp target = towerCore.GetOneTarget();
        
        if (target != null && reloadTime == 0)
        {
            shoot(target);
        }
        else if (reloadTime != 0)
        {
            reloadTime -= Time.deltaTime;
            reloadTime = Mathf.Max(0, reloadTime);
        }
    }
}
