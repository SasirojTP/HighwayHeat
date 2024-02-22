using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObstacleSpawner : NetworkBehaviour
{
    public GameObject obstaclePrefab;
    public List<Transform> spawnPosition = new List<Transform>();
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

                SpawnObstacleRpc(spawnPos);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    void SpawnObstacleRpc(Vector3 spawnPos)
    {
        Instantiate(obstaclePrefab,spawnPos,Quaternion.identity);
    }
}
