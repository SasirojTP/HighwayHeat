using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObstacleSpawner : NetworkBehaviour
{
    public List<Obstacle> obstaclePrefabs = new List<Obstacle>();
    public Transform spawnPosition;

    public float obstacleSpeed = 5;

    public Coroutine spawnObstacle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public IEnumerator StartSpawnObstacle()
    {
        float waitTime = Random.Range(1,5);
        yield return new WaitForSeconds(waitTime);

        int obstacleIndex = Random.Range(0,obstaclePrefabs.Count);
        SpawnObstacleRpc(obstacleIndex,obstacleSpeed);

        if(IsServer)
            spawnObstacle = StartCoroutine(StartSpawnObstacle());
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnObstacleRpc(int obstacleIndex,float speed)
    {
        float yPos = Camera.main.ViewportToWorldPoint(new Vector2(0, Random.Range(0f,1f))).y;

        Obstacle _obstacle = Instantiate(obstaclePrefabs[obstacleIndex],new Vector2(spawnPosition.position.x,yPos),Quaternion.identity);
        _obstacle.speed = speed;
        _obstacle.timeElasped = yPos;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DestroyAllObstacleRpc()
    {
        foreach(Obstacle obstacle in FindObjectsOfType<Obstacle>())
        {
            Destroy(obstacle.gameObject);
        }
    }
}
