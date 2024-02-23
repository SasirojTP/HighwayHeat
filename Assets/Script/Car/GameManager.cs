using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public float elaspedTime = 0;

    public ObstacleSpawner obstacleSpawner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            elaspedTime += Time.deltaTime;
            SetObstacleSpeed();
        }
    }

    void SetObstacleSpeed()
    {
        obstacleSpawner.obstacleSpeed = 5f + (Mathf.Pow(2f,(elaspedTime/100f)));
    }
}
