using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> elaspedTime = new NetworkVariable<float>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);


    public ObstacleSpawner obstacleSpawner;
    public RoadMoving roadMoving;
    public LoginManagerScript loginManagerScript;

    public Button playeButton;

    public NetworkVariable<bool> isGameStart = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerAlive = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public List<CarController> carList = new List<CarController>();

    public Image WinImage;
    public Image LoseImage;
    

    // Start is called before the first frame update
    public void OnCreateServer()
    {
        playeButton.gameObject.SetActive(true);
    }

    void Start()
    {
        playeButton.gameObject.SetActive(false);
        WinImage.gameObject.SetActive(false);
        LoseImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
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

        SetObstacleSpeed();
        SetRoadSpeed();
    }

    void IsGameEnd()
    {
        if(playerAlive.Value <= 1)
        {
            //gameend
            isGameStart.Value = false;
            WinImage.gameObject.SetActive(true);
            StopCoroutine(obstacleSpawner.spawnObstacle);
            obstacleSpawner.DestropAllObstacleRpc();
            playeButton.gameObject.SetActive(true);
        }
    }

    void SetObstacleSpeed()
    {
        obstacleSpawner.obstacleSpeed = 5f + (Mathf.Pow(2f,(elaspedTime.Value/100f)));
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
        //start game
        obstacleSpawner.spawnObstacle = StartCoroutine(obstacleSpawner.StartSpawnObstacle());

        foreach(CarController car in carList)
        {
            car.isGameStart.Value = true;
        }
        isGameStart.Value= true;
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
        LoseImage.gameObject.SetActive(true);
    }
}
