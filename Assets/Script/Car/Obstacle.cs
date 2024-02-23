using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 20;
    public float damage = 5;

    public float timeElasped;

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        IsOutOffScreen();
        timeElasped += Time.deltaTime;
        float viewportPos = Mathf.PingPong(timeElasped,1);
        float yPos = Camera.main.ViewportToWorldPoint(new Vector3(0, viewportPos,1)).y;
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x,yPos,0);
    }

    void IsOutOffScreen()
    {
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPosition.x < 0)
        {
            // Destroy the object
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //make damage to car
        if(other.CompareTag("Car"))
        {
            if(!other.GetComponent<CarController>().isDie)
            {
                other.GetComponent<CarController>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
