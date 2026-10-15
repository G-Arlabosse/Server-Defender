using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;

    private List<Vector3> destinations = new List<Vector3>();
    private Vector3 destination;
    private bool isMoving = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns> the distance between a and b without considering the second component</returns>
    static private float distanceWithoutYS(Vector3 a, Vector3 b)
    {
        float distanceX = a.x - b.x;
        float distanceXS = distanceX * distanceX;
        float distanceZ = a.z - b.z;
        float distanceZS = distanceZ * distanceZ;
        return distanceZS + distanceXS;
    } 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="Speed"></param>
    /// <returns> the next position of a towards b at a speed given</returns>
    static private Vector3 MoveTowardsWithoutY(Vector3 a, Vector3 b, float Speed)
    {
        Vector3 aBis = new Vector3(a.x, a.y, a.z);
        Vector3 bBis = new Vector3(b.x, a.y, b.z);

        return Vector3.MoveTowards(aBis, bBis, Speed);

    }

    /// <summary>
    /// The Enemy has arrive and :
    /// <list type="bullet">
    /// <item> stop if it has no further instruction </item>
    /// <item> continue to it next destination in the list </item>
    /// </list>
    /// </summary>
    private void Arrived()
    {
        if (destinations.Count == 0)
        {
            isMoving = false;
            return;
        }
        else
        {
            GoTo(destinations[0]);
            destinations.RemoveAt(0);
        }

    }

    /// <summary>
    /// replace the current destination of the enemy with the parameter
    /// </summary>
    /// <param name="destination"></param>
    public void GoTo(Vector3 destination)
    {
        this.destination = destination;
        isMoving = true;
    }

    /// <summary>
    /// Add destination into the list of destinations of the enemy
    /// </summary>
    /// <param name="destination"></param>
    public void AddDestination(Vector3 destination)
    {
        if (isMoving)
            destinations.Add(destination);
        else
        {
            isMoving = true;
            this.destination = destination;
        }
    }


    private void Start()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();

            Assert.IsNotNull(rigidBody);
        }

        GoTo(new Vector3(3, 0, 3));
        AddDestination(new Vector3(-3, 0, 3));
        AddDestination(new Vector3(-3, 0, -3));
        AddDestination(new Vector3(3, 0, 3));
    }


    private void FixedUpdate()
    {

        if (isMoving)
        {
            if (distanceWithoutYS(transform.position, destination) < 0.1)
                Arrived();
            else
            {
                transform.position = MoveTowardsWithoutY(transform.position, destination, Time.deltaTime * 4);
            }
        }

    }
}
