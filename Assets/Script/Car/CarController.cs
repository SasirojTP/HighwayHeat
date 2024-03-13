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

    Vector2 startPos;
    float maxHitpoint;

    GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        gameManager = FindObjectOfType<GameManager>();
        startPos = transform.position;
        maxHitpoint = hitPoint;
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
        if(GameManager.inst.isGameStart.Value)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector2 direction = new Vector2(horizontal,vertical);

            transform.Translate(direction * speed * Time.deltaTime);
        }
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
            gameManager.OnPlayerDie();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void GameOverRpc()
    {
        isDie = true;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ResetGameRpc()
    {
        transform.position = startPos;
        isDie = false;
        GetComponent<SpriteRenderer>().enabled = true;
        hitPoint = maxHitpoint;
    }
}
