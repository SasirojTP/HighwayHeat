using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMoving : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public float acceleration = .25f;
    [SerializeField] Material roadMaterial;
    void Start()
    {
        roadMaterial.mainTextureOffset = new Vector2(0, 0);
    }

    void Update()
    {
        //float offset = Time.time * -scrollSpeed;
        //scrollSpeed += acceleration;
        //roadMaterial.mainTextureOffset = new Vector2(offset,0);
    }
}
