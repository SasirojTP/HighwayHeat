using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 20;
    public float damage = 5;

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //make damage to car
        if(other.CompareTag("Car"))
        {
            other.GetComponent<CarController>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
