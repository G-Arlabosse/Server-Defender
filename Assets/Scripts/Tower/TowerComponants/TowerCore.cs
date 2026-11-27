using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public class TowerCore : MonoBehaviour, IFactoryObject
{
    [SerializeField] private CapsuleCollider detectionCollider;
    [SerializeField] private DetectionManager detectionManager;

    /// <summary>
    /// The actual target of the tower. Need to be seen and alive.
    /// </summary>
    [HideInInspector]
    public List<EnemyCore> targets = new List<EnemyCore>();

    private const float GROUND_HEIGHT = 2;
    private const float FLY_HEIGHT = 8;

    public float range
    {
        get { return range; }
        set
        {
            range = value;
            detectionCollider.radius = range;
        }
    }

    public bool flied
    {
        get { return flied; }
        set
        {
            flied = value;
            if (flied)
                detectionCollider.height = FLY_HEIGHT;
            else
                detectionCollider.height = GROUND_HEIGHT;
        }
    }

    private void SortTargets()
    {
        targets.Sort((a, b) =>
        {
            float distanceA = a.DistanceToBase();
            float distanceB = b.DistanceToBase();
            return distanceA.CompareTo(distanceB);
        });
    }

    public void Activate(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public EnemyCore GetOneTarget()
    {
        if (targets.Count > 0)
            return targets[0];
        else
            return null;
    }

    public void AddTarget(EnemyCore target)
    {
        targets.Add(target);
        SortTargets();
    }


}