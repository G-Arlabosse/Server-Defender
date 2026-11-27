using UnityEngine;

public class Projectile : MonoBehaviour, IFactoryObject
{
    private float speed = 8;

    [HideInInspector]
    public EnemyCore target;


    /// <summary>
    /// Move toward the target
    /// </summary>
    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        else
        {
            Deactivate();
        }

    }


    /// <summary>
    /// Hit something. 
    /// Act more if the collider is the target intended
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.GetComponent<EnemyCore>().Equals(target))
            {
                print("hit enemy");
                if (target != null)
                    target.getHit(1);
                Deactivate();
            }
    }

    public void Activate(EnemyCore newTarget, Vector3 position)
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
