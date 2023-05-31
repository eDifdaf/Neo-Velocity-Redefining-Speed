using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    [SerializeField] float Speed = 2.5f;
    [SerializeField] float ExplosionRadius = 50;
    [SerializeField] float ExplosionForce = 25;
    [SerializeField] float LifeTime = 20;

    public List<GameObject> Affected;
    float RemainingLifeTime;

    void Start()
    {
        Affected = new List<GameObject>();
        RemainingLifeTime = LifeTime;
    }

    void FixedUpdate()
    {
        transform.position += transform.rotation * Vector3.forward * Speed;
        RemainingLifeTime -= 0.02f;
        if (RemainingLifeTime < 0)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Affected = Affected.Union(GameObject.FindGameObjectsWithTag("Player")).ToList();

        Affected.ForEach(o =>
        {
            // There is a Method for simulating an Explosion, but because of how velocity is handled it doesn't work
            Vector3 closestPoint = o.GetComponentInChildren<Collider>().ClosestPoint(transform.position);
            Vector3 toObject = closestPoint - transform.position;
            if (toObject.magnitude > ExplosionRadius)
                return;
            float ForceFactor = 1 - toObject.magnitude / ExplosionRadius;
            o.GetComponent<PlayerScript>().velocity += (toObject.normalized + new Vector3(0, 0.2f, 0)).normalized * (ExplosionForce * ForceFactor);
        });

        Destroy(gameObject);
    }
}
