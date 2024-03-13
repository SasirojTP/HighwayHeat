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

    // Start is called before the first frame update
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
                SpawnObstacleRpc(obstacleSpeed);
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
    void SpawnObstacleRpc(float speed)
    {
        float yPos = Camera.main.ViewportToWorldPoint(new Vector2(0, Random.Range(0f,1f))).y;
        int obstacleIndex = Random.Range(0,obstaclePrefabs.Count);

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
