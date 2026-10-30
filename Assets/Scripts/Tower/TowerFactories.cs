using System;
using NUnit.Framework;
using UnityEngine;

public class TowerFactories : MonoBehaviour
{
    [SerializeField] private GameObject baseTower;

    [SerializeField] private TilesManager tilesManager;

    private void OnValidate()
    {


        if (baseTower != null && baseTower.GetComponent<TowerCorp>() == null) { baseTower = null; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(baseTower, "Forget to put baseTower in Tower Factories");

        if (tilesManager == null)
        {
            tilesManager = TilesManager.get();

            Assert.IsNotNull(tilesManager, "Forget to put TilesManager in Tower Factories");
        }

        Instantiate(baseTower, tilesManager.getPositionBase(), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
