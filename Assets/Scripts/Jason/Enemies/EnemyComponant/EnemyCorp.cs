using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class EnemyCorp : MonoBehaviour
{
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private AttackManager attackManager;

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

        Deactivate();
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
        gameObject.SetActive(true);

        transform.position = position;
        print(transform.position);

    }

    public void GoTo(Vector3 destination) {movementManager.GoTo(destination); }


    private void FixedUpdate()
    {
        if (TilesManager.distanceInInt(transform.position, baseLocation) > 1)
            movementManager.FixedMovement();
        else
            attackManager.FixedAttack();
    }
}
