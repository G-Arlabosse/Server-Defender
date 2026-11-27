using System;
using NUnit.Framework;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{

    [SerializeField] private TowerCore towerCore;

    private void Start()
    {
        if (towerCore == null)
        {
            towerCore = GetComponent<TowerCore>();

            Assert.IsNotNull(towerCore);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject body = other.gameObject;

        if (body.tag == "Enemy")
        {
            print("enemy located");
            towerCore.AddTarget(body.GetComponent<EnemyCorp>());
        }
    }

}
