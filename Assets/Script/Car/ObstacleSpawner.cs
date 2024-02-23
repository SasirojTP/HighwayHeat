using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObstacleSpawner : NetworkBehaviour
{
    public Obstacle obstaclePrefab;
    public List<Transform> spawnPosition = new List<Transform>();

    public float obstacleSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 spawnPos = spawnPosition[Random.Range(0,spawnPosition.Count)].position;

                SpawnObstacleRpc(spawnPos,obstacleSpeed);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnObstacleRpc(Vector3 spawnPos,float speed)
    {
        Obstacle _obstacle = Instantiate(obstaclePrefab,spawnPos,Quaternion.identity);
        _obstacle.speed = speed;
    }
}
