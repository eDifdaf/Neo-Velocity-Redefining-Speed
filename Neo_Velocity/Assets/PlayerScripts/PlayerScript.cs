using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] GameObject WallCollider;
    [SerializeField] GameObject GroundCollider;

    [SerializeField] float Gravity;
    [SerializeField] float LookSensitivity;
    [SerializeField] float AccelerationSpeed;
    [SerializeField] float MaxMoveSpeed;
    [SerializeField] float TerminalVelocity;
    [SerializeField] float GroundMercyTime;
    [SerializeField] float JumpDelay;
    [SerializeField] float JumpForce;
    [SerializeField] float MaxJumpVelocity;
    [SerializeField] float WallStickTime;
    [SerializeField] float WallJumpDelay;
    [SerializeField] float FrictionDelay;
    [SerializeField] float FrictionForce;

    new Rigidbody rigidbody;
    new GameObject camera;
    Vector3 velocity;
    Quaternion Rotation;
    Quaternion CameraRotation;
    Vector3 RotationEuler;
    Vector3 CameraRotationEuler;

    Collider OtherWall;
    Collider OtherFloor;

    float LastGroundedTime;
    float LastJumpTime;
    bool IsGrounded; // Wink
    GameObject CurrentWall;
    GameObject LastWall;
    float LastWallTouch;
    float LastWallJump;
    Vector3 LastVelocityAtTouch;
    bool WallTouched;
    bool WallRunning;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        camera = GetComponentInChildren<Camera>().gameObject;

        velocity = rigidbody.velocity;

        Rotation = transform.rotation;
        CameraRotation = camera.transform.localRotation;
        RotationEuler = Rotation.eulerAngles;
        CameraRotationEuler = CameraRotation.eulerAngles;

        WallCollider.GetComponent<ColliderScript>().collided += WallCollided;
        GroundCollider.GetComponent<ColliderScript>().collided += GroundCollided;

        LastGroundedTime = 0f;
        LastJumpTime = 0f;
        LastWallTouch = 0f;
        LastWallJump = 0f;
    }
    float MagnitudeInDirection(Vector3 DirectionGiver, Vector3 Magnitude)
    {
        Vector3 Projected = Vector3.Project(Magnitude, DirectionGiver);
        if (Vector3.Angle(DirectionGiver, Projected) < 90)
            return Projected.magnitude;
        return -Projected.magnitude;
    }

    void Update()
    {
        LastGroundedTime += Time.deltaTime;
        LastJumpTime += Time.deltaTime;
        LastWallTouch += Time.deltaTime;
        LastWallJump += Time.deltaTime;

        if (OtherFloor)
        {
            if (velocity.y < 0)
            {
                LastGroundedTime = 0f;
                velocity.y = 0;
            }
        }
        if (OtherWall && OtherWall != OtherFloor)
        {
            Vector3 WallAwayVector;
            Vector3 ParallelToWall;

            if (LastWall == OtherWall.gameObject)
            {
                LastWallTouch = 0f;
            }
            else if (CurrentWall == null)
            {
                LastVelocityAtTouch = velocity;
                LastWallTouch = 0f;
                CurrentWall = OtherWall.gameObject;
                WallTouched = true;
            }
            else if (CurrentWall == OtherWall.gameObject)
            {
                LastWallTouch = 0f;
            }
            else
            {
                LastVelocityAtTouch = velocity;
                LastWallTouch = 0f;
                LastWall = CurrentWall;
                CurrentWall = OtherWall.gameObject;
                WallTouched = true;
            }
            WallAwayVector = transform.position - OtherWall.gameObject.GetComponent<Collider>().ClosestPoint(transform.position);
            if (Vector3.Angle(velocity, -WallAwayVector) < 90)
            {
                ParallelToWall = Quaternion.AngleAxis(90, Vector3.up) * WallAwayVector;
                Vector3 planeAligned = new Vector3(velocity.x, 0, velocity.z);
                planeAligned = Vector3.Project(planeAligned, ParallelToWall);
                velocity.x = planeAligned.x;
                velocity.z = planeAligned.z;
            }
        }
        OtherFloor = null;
        OtherWall = null;

        IsGrounded = LastGroundedTime < GroundMercyTime;
        if (LastWallTouch > WallStickTime)
        {
            CurrentWall = null;
            LastWall = null;
        }

        float MouseX = Input.GetAxis("Mouse X") * LookSensitivity * Time.deltaTime;
        float MouseY = -Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;

        RotationEuler.y += MouseX;
        Rotation.eulerAngles = RotationEuler;
        transform.rotation = Rotation;

        CameraRotationEuler.x += MouseY;
        if (CameraRotationEuler.x > 90)
            CameraRotationEuler.x = 90;
        else if (CameraRotationEuler.x < -90)
            CameraRotationEuler.x = -90;
        CameraRotation.eulerAngles = CameraRotationEuler;
        camera.transform.localRotation = CameraRotation;

        velocity.y += Gravity * Time.deltaTime;

        float Vertical = Input.GetAxis("Vertical");
        float Horizontal = Input.GetAxis("Horizontal");
        Vector3 PlaneMovement = new Vector3(Horizontal, 0, Vertical).normalized;
        PlaneMovement *= AccelerationSpeed * Time.deltaTime;
        PlaneMovement = Quaternion.AngleAxis(RotationEuler.y, Vector3.up) * PlaneMovement;

        float MagnitudeInMovement = MagnitudeInDirection(PlaneMovement, velocity);

        if (IsGrounded) // Friction
        {
            if (PlaneMovement.magnitude < 0.01)
            {
                Vector3 flatVelocity = new Vector3(velocity.x, 0, velocity.z);
                velocity -= flatVelocity * FrictionForce * Time.deltaTime;
            }
            else
            {
                Vector3 flatVelocity = new Vector3(velocity.x, 0, velocity.z);
                float flatDirection = Vector3.Angle(PlaneMovement, flatVelocity) / 180;
                flatVelocity = flatVelocity * flatDirection * FrictionForce * Time.deltaTime;
                velocity -= flatVelocity;
            }
        }

        if (MagnitudeInMovement + PlaneMovement.magnitude < MaxMoveSpeed)
            velocity += PlaneMovement;
        else if (MagnitudeInMovement < MaxMoveSpeed)
            velocity += PlaneMovement.normalized * (MaxMoveSpeed - MagnitudeInMovement);

        float Space = Input.GetAxis("Jump");
        if (Space > 0.5f)
        {
            if (CurrentWall != null && LastWallJump > WallJumpDelay && LastJumpTime > JumpDelay && !IsGrounded)
            {
                Vector3 newVelocity = new Vector3(LastVelocityAtTouch.x, 0, LastVelocityAtTouch.z);
                Vector3 WallAwayVector = transform.position - CurrentWall.GetComponent<Collider>().ClosestPoint(transform.position);
                WallAwayVector.y = 0;
                WallAwayVector = WallAwayVector.normalized;
                newVelocity = newVelocity - 2 * Vector3.Dot(newVelocity, WallAwayVector) * WallAwayVector;
                newVelocity.y = velocity.y;
                velocity = newVelocity;
                if (velocity.y < MaxJumpVelocity)
                {
                    if (velocity.y + JumpForce < MaxJumpVelocity)
                        velocity.y += JumpForce;
                    else
                        velocity.y = MaxJumpVelocity;
                }
                WallRunning = false;
                LastWallJump = 0f;
            }
            else if (LastJumpTime > JumpDelay && IsGrounded)
            {
                velocity.y += JumpForce;
                LastJumpTime = 0f;
                LastGroundedTime = GroundMercyTime;
            }
        }
        if (velocity.y < TerminalVelocity)
        {
            velocity.y = TerminalVelocity;
        }

        // Wallrunning
        // WallCollider.GetComponent<Collider>().ClosestPoint()
        // Teleport next to wall

        if (Input.GetKeyDown(KeyCode.F))
        {
            velocity = Vector3.zero;
            transform.position = new Vector3(-5, 2, 5);
        }
        rigidbody.velocity = velocity;
        WallTouched = false;
    }

    void WallCollided(Collider other)
    {
        OtherWall = other;
    }
    void GroundCollided(Collider other)
    {
        OtherFloor = other;
    }
}
