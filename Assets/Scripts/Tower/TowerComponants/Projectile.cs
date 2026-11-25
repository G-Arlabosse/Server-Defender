using UnityEngine;

public class Projectile : MonoBehaviour, IFactoryObject
{
    private float speed = 8;

    [HideInInspector]
    public GameObject target;


    /// <summary>
    /// Move toward the target
    /// </summary>
    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }

    }


    /// <summary>
    /// Hit something. 
    /// Act more if the collider is the target intended
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.Equals(target))
            {
                EnemyCorp enemyCorp = target.GetComponent<EnemyCorp>();
                if (enemyCorp != null)
                    enemyCorp.getHit(1);
                Deactivate();
            }
    }

    public void Activate(GameObject newTarget, Vector3 position)
    {
        target = newTarget;
        transform.position = position;
        gameObject.SetActive(true);
    }


    public void Deactivate()
    {
        target = null;
        transform.position = new Vector3(-100, -100, -100);
        gameObject.SetActive(false);
    }

}
