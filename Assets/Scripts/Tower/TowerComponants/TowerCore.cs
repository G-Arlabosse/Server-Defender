using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public class TowerCore : MonoBehaviour
{
 

    void Start()
    {
        Deactivate();
    }


    public void Activate(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        print("activated tower at: " + position);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }


}