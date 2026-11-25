using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public class TowerCore : MonoBehaviour, IFactoryObject
{


    public void Activate(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }


}