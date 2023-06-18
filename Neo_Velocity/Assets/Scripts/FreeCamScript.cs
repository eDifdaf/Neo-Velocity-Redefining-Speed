using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FreeCamScript : MonoBehaviour
{
    [SerializeField] public bool ConstantSpeed = false;
    [SerializeField] public float SensiX = 4f;
    [SerializeField] public float SensiY = 7f;
    [SerializeField] public float Speed = 10f;
    [SerializeField] public float Ramp = 1f;
    Vector3 RotationEuler;
    Quaternion Rotation;

    float currentRamp;

    List<GameObject> UIElements;


    private void Start()
    {
        UIElements = new List<GameObject>();
        FindObjectsOfType<GameObject>().ToList().ForEach(o =>
        {
            if (o.layer != 5)
                return;
            o.SetActive(false);
            UIElements.Add(o);
        });

        Rotation = transform.localRotation;
        RotationEuler = Rotation.eulerAngles;

        currentRamp = 0f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            UIElements.ForEach(o => o.SetActive(true));

        if (Input.GetKeyDown(KeyCode.G))
            ConstantSpeed = !ConstantSpeed;

        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
            Speed += 0.1f;
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            Speed -= 0.1f;

        currentRamp += Time.deltaTime * Ramp;

        float x = Input.GetAxis("Mouse X") * SensiX;
        float y = Input.GetAxis("Mouse Y") * SensiY;

        RotationEuler += new Vector3(-y, x);
        Rotation.eulerAngles = RotationEuler;
        transform.rotation = Rotation;

        Vector3 movement = new Vector3(Input.GetKey(KeyCode.D) ? 1 : 0 - (Input.GetKey(KeyCode.A) ? 1 : 0), Input.GetKey(KeyCode.Space) ? 1 : 0 - (Input.GetKey(KeyCode.LeftShift) ? 1 : 0), Input.GetKey(KeyCode.W) ? 1 : 0 - (Input.GetKey(KeyCode.S) ? 1 : 0));
        movement = Rotation * movement;
        if (ConstantSpeed)
            transform.position += movement * Time.deltaTime * Speed;
        else
            transform.position += movement * Time.deltaTime * Speed * currentRamp;

        if (movement.magnitude < 0.1f)
            currentRamp = 0;
    }
}
