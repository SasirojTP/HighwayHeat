using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Obstacle : NetworkBehaviour
{
    public float speed = 5;
    public float damage = 5;

    public override void OnNetworkSpawn()
    {

    }

    void FixedUpdate()
    {
        if(IsServer)
        {
            Move();
        }
    }

    void Move()
    {
        
    }

    void OnTriggerEnter(Collider other) 
    {
        //make damage to car
        if(other.CompareTag("Car"))
        {
            other.GetComponent<CarController>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
