using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class CarController : NetworkBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float hitPoint = 10;

    [SerializeField] float minPositionX = -15f;
    [SerializeField] float maxPositionX = 11f;
    [SerializeField] float minPositionY = -7f;
    [SerializeField] float maxPositionY = 9f;

    [SerializeField] TMP_Dropdown dropdown;

    public bool isDie = false;

    Vector2 startPos;
    float maxHitpoint;
    public NetworkVariable<int> playerCarColor = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        startPos = transform.position;
        maxHitpoint = hitPoint;

    }

    void Start()
    {
        GameManager.inst.carList.Add(this);
    }

    void Update()
    {
        if (IsOwner)
        {
            Move();
            ShowDropDown();
        }
    }

    void ShowDropDown()
    {
        if (GameManager.inst.isGameStart.Value == false)
        {
            dropdown.gameObject.SetActive(true);
        }
        else
        {
            dropdown.gameObject.SetActive(true);
        }
    }

    void Move()
    {
        if (GameManager.inst.isGameStart.Value)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if ((transform.position.x < minPositionX && horizontal < 0) || (transform.position.x > maxPositionX && horizontal > 0))
            {
                horizontal = 0;
            }
            if ((transform.position.y < minPositionY && vertical < 0) || (transform.position.y > maxPositionY && vertical > 0))
            {
                vertical = 0;
            }

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
            GameManager.inst.OnPlayerDieRpc();
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
        SetPlayerColorRpc(playerCarColor.Value);
        hitPoint = maxHitpoint;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetCarHealthToOneRpc()
    {
        hitPoint = 1;
        GetComponent<SpriteRenderer>().color = Color.red;
    }
    private void OnDestroy()
    {
        GameManager.inst.carList.Remove(this);
    }
    public void OnDropdownValueChange(TMP_Dropdown changeValue)
    {
        playerCarColor.Value = changeValue.value;
        SetPlayerColorRpc(changeValue.value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPlayerColorRpc(int value)
    {
        switch (value)
        {
            case 0:
                GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case 1:
                GetComponent<SpriteRenderer>().color = Color.gray;
                break;
            case 2:
                GetComponent<SpriteRenderer>().color = Color.green;
                break;
            case 3:
                GetComponent<SpriteRenderer>().color = Color.yellow;
                break;
        }
        
    }
}
