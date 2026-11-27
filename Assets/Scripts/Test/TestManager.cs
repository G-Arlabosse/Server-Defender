using NUnit.Framework;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField] private TowerManager towerManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(towerManager);

        towerManager.CreateTower(new Vector3(0, 1, 5));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
