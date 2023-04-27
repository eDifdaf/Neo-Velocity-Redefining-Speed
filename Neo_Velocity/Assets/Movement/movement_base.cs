using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement_base : MonoBehaviour
{
    private Vector3 movementVelocity;
    private Vector3 movementDirection;

    public float movementSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float wallRunGravity = 1f;
    public float wallRunSpeed = 10f;
    public float wallRunMaxDuration = 5f;
    public float wallRunMaxDistance = 5f;
    public LayerMask wallLayerMask;
    public Camera mainCamera;

    private Rigidbody rb;
    public bool isGrounded;
    public bool isWallRunning;
    public float wallRunDuration;
    public float wallRunDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isGrounded = true;
        isWallRunning = false;
        wallRunDuration = 0f;
        wallRunDistance = 0f;
    }

    void Update()
    {
        // Get input axis values
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Debug.Log("Horizontal: " + horizontal);
        Debug.Log("Vertical: " + vertical);

        // Determine movement direction based on camera orientation
        movementDirection = Vector3.Normalize(mainCamera.transform.forward * vertical + mainCamera.transform.right * horizontal);

        // Apply movement speed
        float speed = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }
        movementVelocity = movementDirection * speed;

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Wall run
        if (!isGrounded && Input.GetKey(KeyCode.LeftControl) && wallRunDuration < wallRunMaxDuration && wallRunDistance < wallRunMaxDistance)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, mainCamera.transform.forward, out hit, 2f, wallLayerMask) && Vector3.Dot(rb.velocity, hit.normal) < 0)
            {
                isWallRunning = true;
                rb.useGravity = false;
                Vector3 wallRunDirection = Vector3.Cross(hit.normal, Vector3.up);
                movementVelocity = wallRunDirection * wallRunSpeed;
                wallRunDuration += Time.deltaTime;
                wallRunDistance += movementVelocity.magnitude * Time.deltaTime;
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }

        // Apply gravity
        if (!isGrounded && !isWallRunning)
        {
            rb.useGravity = true;
            rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Acceleration);
        }
    }

    void FixedUpdate()
    {
        // Apply movement velocity to rigidbody
        rb.velocity = new Vector3(movementVelocity.x, rb.velocity.y, movementVelocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player is on the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            StopWallRun();
        }
    }

    void StopWallRun()
    {
        isWallRunning = false;
        wallRunDuration = 0f;
        wallRunDistance = 0f;
        rb.useGravity = true;
    }

}
