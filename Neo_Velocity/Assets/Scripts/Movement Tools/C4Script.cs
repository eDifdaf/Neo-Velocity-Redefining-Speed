using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class C4Script : MonoBehaviour
{
    [SerializeField] float Speed = 25;
    [SerializeField] float ExplosionRadius = 50;
    [SerializeField] float ExplosionForce = 25;
    [SerializeField] float LifeTime = 20; // Only while in the air
    [SerializeField] float Gravity = 25;
    [SerializeField] float TerminalVelocity = -30;

    public List<GameObject> Affected;
    float RemainingLifeTime;
    bool Landed;

    void Start()
    {
        Affected = new List<GameObject>();
        RemainingLifeTime = LifeTime;
        GetComponent<Rigidbody>().velocity = transform.rotation * Vector3.forward * Speed;
    }

    void FixedUpdate()
    {
        if (!Landed)
        {
            RemainingLifeTime -= 0.02f;
            if (RemainingLifeTime < 0)
                Destroy(gameObject);
            Vector3 temp = GetComponent<Rigidbody>().velocity;
            temp.y -= Gravity * Time.fixedDeltaTime;
            if (temp.y < TerminalVelocity)
                temp.y = TerminalVelocity;
            GetComponent<Rigidbody>().velocity = temp;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Landed = true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void Explode()
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
            o.GetComponent<PlayerScript>().velocity += (toObject.normalized + new Vector3(0, 0.1f, 0)).normalized * (ExplosionForce * ForceFactor);
        });

        Destroy(gameObject);
    }
}
