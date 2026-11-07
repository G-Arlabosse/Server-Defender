using UnityEngine;

public class Projectile : MonoBehaviour
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
                print("target hits");
                Destroy(this.gameObject);
            }
    }

}
