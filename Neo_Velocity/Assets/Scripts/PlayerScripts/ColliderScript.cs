using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderScript : MonoBehaviour
{
    public delegate void Collided(Collider other);
    public event Collided collided;

    void OnTriggerStay(Collider other)
    {
        if(other.isTrigger){
            return;
        }
        //Debug.Log(other.gameObject.name);
        collided.Invoke(other);
    }
}
