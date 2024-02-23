using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CarController : NetworkBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float hitPoint = 10;

    Rigidbody2D rb;

    public bool isDie = false;

    public override void OnNetworkSpawn()
    {
        
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(IsOwner)
        {
            Move();
        }
    }

    void FixedUpdate()
    {

    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(horizontal,vertical);

        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if(!IsOwner)
            return;

        if(isDie)
            return;

        hitPoint -= damage;

        if(hitPoint <= 0)
        {
            GameOverRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void GameOverRpc()
    {
        isDie = true;
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
