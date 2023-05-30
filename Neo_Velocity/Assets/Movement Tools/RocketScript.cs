using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    [SerializeField] float Speed = 5;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += transform.rotation * Vector3.forward * Speed;
    }
}
