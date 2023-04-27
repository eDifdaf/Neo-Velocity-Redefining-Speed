using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_controller : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float smoothing = 0.1f;
    [SerializeField] private Transform playerTransform;

    private Vector3 smoothVelocity;
    private Vector3 currentRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        currentRotation.x -= mouseY;
        currentRotation.y += mouseX;
        currentRotation.x = Mathf.Clamp(currentRotation.x, -90f, 90f);

        Vector3 targetRotation = playerTransform.localRotation.eulerAngles + currentRotation;
        transform.localRotation = Quaternion.Euler(targetRotation);

        transform.position = Vector3.SmoothDamp(transform.position, playerTransform.position + offset, ref smoothVelocity, smoothing);
    }
}


