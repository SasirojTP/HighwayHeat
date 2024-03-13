using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> elaspedTime = new NetworkVariable<float>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public RoadMoving roadMoving;
    public LoginManagerScript loginManagerScript;

    public Button playeButton;

    public NetworkVariable<bool> isGameStart = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerAlive = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public List<CarController> carList = new List<CarController>();

    public TextMeshProUGUI winText;
    public TextMeshProUGUI loseText;

    public static GameManager inst;

    float tempTime;

    private void Awake()
    {
        inst = this;
    }

    public void OnCreateServer()
    {
        playeButton.gameObject.SetActive(true);
    }

    void Start()
    {
        playeButton.gameObject.SetActive(false);
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
    }

    void Update()
    {
        if(IsServer)
        {
            if(isGameStart.Value)
            {
                elaspedTime.Value += Time.deltaTime;

                IsGameEnd();
            }
        }

        SetRoadSpeed();
    }

    void IsGameEnd()
    {
        if(playerAlive.Value <= 1)
        {
            //gameend
            isGameStart.Value = false;
            ObstacleSpawner.inst.DestroyAllObstacleRpc();
            playeButton.gameObject.SetActive(true);
            ShowTextEndGameRpc();
        }
    }

    void SetRoadSpeed()
    {
        roadMoving.scrollSpeed.Value = 1f + (Mathf.Pow(2f,(elaspedTime.Value/100f)));
    }

    public void StartGame()
    {
        if(loginManagerScript.playerNum.Value < 2)
            return;

        playeButton.gameObject.SetActive(false);
        ResetGame();

        isGameStart.Value = true;
    }

    void ResetGame()
    {
        foreach(CarController car in carList)
        {
            car.ResetGameRpc();
        }
    }

    public void OnPlayerDie()
    {
        playerAlive.Value--;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ShowTextEndGameRpc()
    {
        foreach(CarController n in carList)
        {
            if (n.IsOwner == true)
            {
                if(n.isDie == true)
                {
                    winText.gameObject.SetActive(false);
                    loseText.gameObject.SetActive(true);
                }
                else
                {
                    winText.gameObject.SetActive(false);
                    loseText.gameObject.SetActive(true);
                }
                print(n.isDie);
            }
        }
    }
}
