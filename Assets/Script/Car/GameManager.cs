using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public float elaspedTime = 0;

    public ObstacleSpawner obstacleSpawner;
    public LoginManagerScript loginManagerScript;

    public Button playeButton;

    bool isGameStart;

    public List<CarController> carList = new List<CarController>();

    // Start is called before the first frame update
    public void OnCreateServer()
    {
        playeButton.gameObject.SetActive(true);
    }

    void Start()
    {
        playeButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(isGameStart)
            {
                elaspedTime += Time.deltaTime;
                SetObstacleSpeed();

                IsGameEnd();
            }
        }
    }

    void IsGameEnd()
    {
        int playerAlive = 0;
        foreach(CarController car in carList)
        {
            if(!car.isDie)
                playerAlive++;
        }

        if(playerAlive <= 1)
        {
            //gameend
            isGameStart = false;
            StopCoroutine(obstacleSpawner.spawnObstacle);
            obstacleSpawner.DestropAllObstacleRpc();
            playeButton.gameObject.SetActive(true);
        }
    }

    void SetObstacleSpeed()
    {
        obstacleSpawner.obstacleSpeed = 5f + (Mathf.Pow(2f,(elaspedTime/100f)));
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
        isGameStart= true;
    }

    void ResetGame()
    {
        foreach(CarController car in carList)
        {
            car.ResetGameRpc();
        }
    }
}
