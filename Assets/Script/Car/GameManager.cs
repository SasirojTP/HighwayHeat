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
    public RoadMoving buildingMoving;
    public LoginManagerScript loginManagerScript;

    public Button playButton;

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
        playButton.gameObject.SetActive(true);
    }

    void Start()
    {
        playButton.gameObject.SetActive(false);
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
        SetEnvironmentSpeed();
    }

    void IsGameEnd()
    {
        if(playerAlive.Value <= 1)
        {
            //gameend
            isGameStart.Value = false;
            ObstacleSpawner.inst.DestroyAllObstacleRpc();
            playButton.gameObject.SetActive(true);
            ShowTextEndGameRpc();
        }
    }

    void SetEnvironmentSpeed()
    {
        roadMoving.scrollSpeed.Value = (1f + (Mathf.Pow(2f,(elaspedTime.Value/100f)))) /3;
        buildingMoving.scrollSpeed.Value = -((1f + (Mathf.Pow(2f, (elaspedTime.Value / 100f))))) /6;
    }

    public void StartGame()
    {
        if(loginManagerScript.playerNum.Value < 2)
            return;

        playButton.gameObject.SetActive(false);
        foreach (CarController car in carList)
        {
            car.ResetGameRpc();
        }
        playerAlive.Value = carList.Count;
        HideTextRpc();
        isGameStart.Value = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void OnPlayerDieRpc()
    {
        if (IsServer)
        {
            playerAlive.Value--;
        }
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
                    winText.gameObject.SetActive(true);
                    loseText.gameObject.SetActive(false);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void HideTextRpc()
    {
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
    }
}
