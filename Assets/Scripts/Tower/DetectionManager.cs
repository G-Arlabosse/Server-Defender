using System;
using NUnit.Framework;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{

    [SerializeField] private ShooterManager shooterManager;

    private void Start()
    {
        if (shooterManager == null)
        {
            shooterManager = GetComponent<ShooterManager>();

            Assert.IsNotNull(shooterManager);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject body = other.gameObject;

        if (body.tag == "Enemy")
        {
            print("enemy located");
            shooterManager.target = body;
        }
    }

}
