using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class EnemyCorp : MonoBehaviour, IFactoryObject
{

    private int hp = 5;

    [SerializeField] 
    private MovementManager movementManager;
    [SerializeField] 
    private AttackManager attackManager;

    [HideInInspector]
    public Vector3 baseLocation;

    [HideInInspector]
    public bool alive = false;

    private void Start()
    {
        if (movementManager == null)
        {
            movementManager = GetComponent<MovementManager>();

            Assert.IsNotNull(movementManager);
        }

        if (attackManager == null)
        {
            attackManager = GetComponent<AttackManager>();

            Assert.IsNotNull(attackManager);
        }

    }

    public void Deactivate()
    {
        alive = false;
        movementManager.Clear();
        gameObject.SetActive(false);
    }

    public void Activate(Vector3 position)
    {
        alive = true;
        hp = 5;
        gameObject.SetActive(true);

        transform.position = position;

    }

    public void GoTo(Vector3 destination) {movementManager.GoTo(destination); }


    private void FixedUpdate()
    {
        if (TilesManager.distanceInInt(transform.position, baseLocation) > 1)
            movementManager.FixedMovement();
        else
            attackManager.FixedAttack();
    }

    public void getHit(int dmg)
    {
        hp -= dmg;

        if (hp <= 0)
            Deactivate();
    }
}
