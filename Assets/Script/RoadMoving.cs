using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoadMoving : NetworkBehaviour
{
    public NetworkVariable<float> scrollSpeed = new NetworkVariable<float>(1f,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    [SerializeField] Material roadMaterial;
    void Start()
    {
        roadMaterial.mainTextureOffset = new Vector2(0, 0);
    }

    void Update()
    {
        float offset = Time.time * -scrollSpeed.Value;
        roadMaterial.mainTextureOffset = new Vector2(offset,0);
    }
}
