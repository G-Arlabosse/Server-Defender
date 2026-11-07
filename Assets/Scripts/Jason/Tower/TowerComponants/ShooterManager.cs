using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class ShooterManager : MonoBehaviour
{

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
    /// The actual target of the tower. Need to be seen and alive.
    /// </summary>
    [HideInInspector]
    public List<GameObject> targets = new List<GameObject>();

    /// <summary>
    /// The time between two shoot
    /// </summary>
    private float fireRate = 1;
    /// <summary>
    /// Decrease until next shoot
    /// </summary>
    private float reloadTime = 0;

    private void Start()
    {
        Assert.IsNotNull(projectile);

        if (head == null)
            shootFrom = transform.position;
        else
            shootFrom = head.transform.position;

    }

    /// <summary>
    /// Call when need to shoot the next bullet
    /// </summary>
    private void shoot()
    {
        reloadTime = fireRate;
        Projectile new_proj = Instantiate(projectile, shootFrom, Quaternion.identity).GetComponent<Projectile>();
        new_proj.target = targets[0];
    }

    void Update()
    {
        
        if (targets.Count > 0 && reloadTime == 0)
        {
            shoot();
        }
        else if (reloadTime != 0)
        {
            reloadTime -= Time.deltaTime;
            reloadTime = Mathf.Max(0, reloadTime);
        }
    }
}
