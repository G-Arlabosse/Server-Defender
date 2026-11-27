using UnityEngine;

public enum EnemyType { Melee, Flying, Ranged }

public class Enemybis : MonoBehaviour
{
    //Speeb
    public float speed = 4f;
    public float maxForce = 10f;
    public EnemyType type = EnemyType.Melee;
    private Vector3 velocity;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Move(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Vector3 desiredVelocity = direction * speed;
            Vector3 steering = desiredVelocity - velocity;
            steering = Vector3.ClampMagnitude(steering, maxForce);
            velocity += steering * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, speed);
            transform.position += velocity * Time.deltaTime;
        }
        else { velocity = Vector3.zero; }
    }
}