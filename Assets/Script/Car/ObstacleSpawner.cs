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

    public static ObstacleSpawner inst;

    float tempTime = 0;

    float waitTime = 1f;

    void Awake()
    {
        inst = this;
    }

    private void Update() {
        if(IsServer == false)
            return;

        if(GameManager.inst.isGameStart.Value == true)
        {
            if(Time.time - tempTime > waitTime)
            {
                int obstacleIndex = Random.Range(0,obstaclePrefabs.Count);
                SpawnObstacleRpc(obstacleIndex,obstacleSpeed);
                tempTime = Time.time;
                waitTime = Random.Range(2f,5f);
            }
            SetObstacleSpeed();
        }
    }

    void SetObstacleSpeed()
    {
        obstacleSpeed = 5f + Mathf.Pow(2f,GameManager.inst.elaspedTime.Value/100f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnObstacleRpc(int obstacleIndex,float speed)
    {
        float yPos = Camera.main.ViewportToWorldPoint(new Vector2(0, Random.Range(0f,.5f))).y;

        Obstacle _obstacle = Instantiate(obstaclePrefabs[obstacleIndex],new Vector2(spawnPosition.position.x,yPos),spawnPosition.transform.rotation);
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
