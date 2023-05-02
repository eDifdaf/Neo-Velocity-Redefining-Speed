using JetBrains.Rider.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// Wall Detect+
// PastWalls List
// set delay till again touchable
// reset delay if touch in delay

// Wallrunning
// WallCollider.GetComponent<Collider>().ClosestPoint()
// Teleport next to wall

public class PlayerScript : MonoBehaviour
{
    #region Field Declaration

    [SerializeField] GameObject PlayerCamera;
    [SerializeField] GameObject WallCollider;
    [SerializeField] GameObject GroundCollider;

    [SerializeField] float Gravity;
    [SerializeField] float LookSensitivity;
    [SerializeField] float AccelerationSpeed;
    [SerializeField] float ApproachingCooeficient = 60000; // ChatGPT too dumb for this
    [SerializeField] float MaxMoveSpeed = 10000;
    [SerializeField] float TerminalVelocity;
    [SerializeField] float GroundMercyTime;
    [SerializeField] float JumpDelay;
    [SerializeField] float JumpForce;
    [SerializeField] float MaxJumpVelocity;
    [SerializeField] float WallStickTime;
    [SerializeField] float WallJumpDelay;
    [SerializeField] float WallRunDistanceFromWall = 0.51f; // How far a wall teleports you (Bean is r=0.5, so 0.51 works well)
    [SerializeField] float FrictionDelay; // Doesn't actually do anything
    [SerializeField] float FrictionForce; // Decelerate movement
    [SerializeField] float FrictionCompensation; // Accelerate normal movement by apllied Friction
    [SerializeField] float WallRunDelay; // If you fall of a Wall, how long until you can run again
    [SerializeField] float WallJumpForgetDelay; // How long you have to stick to a wall, for it to forget your initial touchspeed
    [SerializeField] float FOV;

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
    bool IsGrounded; // Wink   <- Why did I write this?
    GameObject CurrentWall;
    GameObject LastWall;
    float LastWallTouch;
    float LastWallJump;
    Vector3 LastVelocityAtTouch;
    bool WallRunning;
    float LastWallRun;
    float WallJumpForgetTime;

    #endregion

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
        LastWallRun = 0f;
    }

    #region MagnitudeInDirection
    /// <summary>
    /// Outputs the Magnitude of the Magnitude Vector, after being projected on the DirectionGiver
    /// </summary>
    /// <param name="DirectionGiver"></param>
    /// <param name="Magnitude"></param>
    /// <returns></returns>
    float MagnitudeInDirection(Vector3 DirectionGiver, Vector3 Magnitude)
    {
        Vector3 Projected = Vector3.Project(Magnitude, DirectionGiver);
        if (Vector3.Angle(DirectionGiver, Projected) < 90)
            return Projected.magnitude;
        return -Projected.magnitude;
    }
    #endregion

    void Update()
    {
        #region Advance Timers
        LastGroundedTime += Time.deltaTime;
        LastJumpTime += Time.deltaTime;
        LastWallTouch += Time.deltaTime;
        LastWallJump += Time.deltaTime;
        LastWallRun += Time.deltaTime;
        WallJumpForgetTime += Time.deltaTime;
        #endregion

        // Floor and Wall
        #region Collision Handling

        // Checks if collided with a Floor
        if (OtherFloor)
        {
            if (velocity.y < 0)
            {
                LastGroundedTime = 0f;
                velocity.y = 0;
            }
        }

        // Checks if collided with a Wall (that isn't the Floor)
        if (OtherWall && OtherWall != OtherFloor)
        {
            if (LastWallJump > WallJumpDelay && LastWallRun > WallRunDelay)
                WallRunning = true;
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
                WallJumpForgetTime = 0f;
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
                WallJumpForgetTime = 0f;
            }

            // Aligns Momentum in direction of the wall, if facing towards the wall
            #region AlignMomentumToWall
            WallAwayVector = transform.position - OtherWall.gameObject.GetComponent<Collider>().ClosestPoint(transform.position);
            if (Vector3.Angle(velocity, -WallAwayVector) < 90)
            {
                ParallelToWall = Quaternion.AngleAxis(90, Vector3.up) * WallAwayVector;
                Vector3 planeAligned = new Vector3(velocity.x, 0, velocity.z);
                planeAligned = Vector3.Project(planeAligned, ParallelToWall);
                velocity.x = planeAligned.x;
                velocity.z = planeAligned.z;
            }
            #endregion
        }

        OtherFloor = null;
        OtherWall = null;
        #endregion

        IsGrounded = LastGroundedTime < GroundMercyTime;

        if (LastWallTouch > WallStickTime)
        {
            CurrentWall = null;
            LastWall = null;
            WallRunning = false;
        } // Reset the touched walls

        // Axis - Sensi: 0,1
        float MouseX = Input.GetAxis("Mouse X") * LookSensitivity * Time.deltaTime; // <- Time.deltaTime might be wrong, depending on how Mouse Axis work
        float MouseY = -Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;

        // Mouse Movement doesn't line up with Desktop Movement, don't know why

        #region General Rotation
        RotationEuler.y += MouseX;
        Rotation.eulerAngles = RotationEuler;
        transform.rotation = Rotation;
        #endregion
        #region Camera Rotation
        CameraRotationEuler.x += MouseY;
        if (CameraRotationEuler.x > 90)
            CameraRotationEuler.x = 90;
        else if (CameraRotationEuler.x < -90)
            CameraRotationEuler.x = -90;
        CameraRotation.eulerAngles = CameraRotationEuler;
        camera.transform.localRotation = CameraRotation;
        #endregion

        velocity.y += Gravity * Time.deltaTime;

        #region PlaneMovement Vector
        // for Vertical and Horizontal set:
        // Gravity: 100
        // Dead: 0.001
        // Sensitivity: 15
        float Vertical = Input.GetAxis("Vertical");
        float Horizontal = Input.GetAxis("Horizontal");
        Vector3 PlaneMovement = new Vector3(Horizontal, 0, Vertical).normalized;
        PlaneMovement *= AccelerationSpeed * Time.deltaTime;
        PlaneMovement = Quaternion.AngleAxis(RotationEuler.y, Vector3.up) * PlaneMovement;
        #endregion

        float MagnitudeInMovement = MagnitudeInDirection(PlaneMovement, velocity);

        #region Friction
        if (IsGrounded)
        {
            Vector3 flatVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (PlaneMovement.magnitude < 0.01)
            {
                velocity -= flatVelocity * FrictionForce * Time.deltaTime;
            }
            else
            {
                float flatDirection = Vector3.Angle(PlaneMovement, flatVelocity) / 180;
                velocity -= flatVelocity * flatDirection * FrictionForce * Time.deltaTime;
                PlaneMovement *= 1 + flatDirection / 2 * FrictionCompensation;
            }
        }
        #endregion

        #region Apply Movement
        if (MagnitudeInMovement + PlaneMovement.magnitude < MaxMoveSpeed)
            velocity += PlaneMovement;
        else if (MagnitudeInMovement < MaxMoveSpeed)
            velocity += PlaneMovement.normalized * (MaxMoveSpeed - MagnitudeInMovement);
        #endregion

        if (WallRunning && CurrentWall != null)
        {
            LastWallRun = 0f;
            Vector3 WallAwayVector = transform.position - CurrentWall.GetComponent<Collider>().ClosestPoint(transform.position);
            float AngleFromWallAndMovement = Vector3.Angle(PlaneMovement, WallAwayVector);
            float AngleFromWallAndVelocity = Vector3.Angle(new Vector3(velocity.x, 0, velocity.z), WallAwayVector);
            if (AngleFromWallAndMovement < 45 && PlaneMovement.magnitude > 0.01)
            {
                WallRunning = false;
            }
            else if (AngleFromWallAndVelocity < 20)
            {
                WallRunning = false;
            }
            else if (WallAwayVector.magnitude > WallRunDistanceFromWall)
            {
                WallAwayVector = WallAwayVector / WallAwayVector.magnitude * (WallAwayVector.magnitude - WallRunDistanceFromWall);
                transform.position -= WallAwayVector;
            }
            velocity.y = 0;
        } // everything wall

        if (WallJumpForgetTime > WallJumpForgetDelay)
        {
            LastVelocityAtTouch = velocity;
        }// Reset Walljump reflexion-direction to velocity

        if (Input.GetKey(KeyCode.Space))
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
                velocity += WallAwayVector * JumpForce;
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
        } // Jump

        if (velocity.y < TerminalVelocity)
        {
            velocity.y = TerminalVelocity;
        } // Caps falling-speed at TerminalVelocity

        if (Input.GetKeyDown(KeyCode.F))
        {
            velocity = Vector3.zero;
            transform.position = new Vector3(-5, 2, 5);
        } // Reset Character / Debug Purpose

        Vector3 ActualVelocity = new Vector3(velocity.x, 0, velocity.z);
        ActualVelocity = ActualVelocity.normalized * DecreasedReturn(ActualVelocity.magnitude);
        rigidbody.velocity = new Vector3(ActualVelocity.x, velocity.y, ActualVelocity.z);
        //camera.GetComponent<Camera>().fieldOfView = Mathf.Floor(FOV + FOV * Mathf.Pow(velocity.magnitude / 100, 0.1f));
    }

    #region Save Collided
    void WallCollided(Collider other)
    {
        OtherWall = other;
    }
    void GroundCollided(Collider other)
    {
        OtherFloor = other;
    }
    #endregion

    float DecreasedReturn(float value)
    {
        return 200 - ApproachingCooeficient / (value + 300);
    }
}
