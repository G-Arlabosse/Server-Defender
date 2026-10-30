using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Factory<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] private int nmbPrepared;
    private List<T> components = new List<T>();

    private static readonly Vector3 DEFAULT_POSITION = new Vector3(-100, -100, -100);

    private void OnValidate()
    {
        if (prefab.GetComponent<T>() == null) { prefab = null; }
    }

    private T Create(bool available)
    {
        GameObject gameObject = Instantiate(prefab, DEFAULT_POSITION, Quaternion.identity);

        T component = gameObject.GetComponent<T>();

        components.Add(component);

        return component;
    }

    public virtual T getOne()
    {
        int count = components.Count;

        for (int i = 0 ; i < count ; i ++)
        {
            if (!components[i].gameObject.activeSelf)
            {
                return components[i];
            }
        }

        return Create(false);
    }

    public virtual void Remove(T objectToRemove)
    {
        objectToRemove.gameObject.SetActive(false);
    }

    private void Start()
    {
        for (int i=0; i<nmbPrepared ; i++)
        {
            Create(true);
        }
    }
}
