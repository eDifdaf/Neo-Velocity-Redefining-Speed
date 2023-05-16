using JetBrains.Rider.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerScript : MonoBehaviour
{
    #region Field Declaration

    [SerializeField] GameObject Body;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] GameObject WallCollider;
    [SerializeField] GameObject GroundCollider;

    [SerializeField] float Gravity;
    [SerializeField] float LookSensitivity;
    [SerializeField] float AccelerationSpeed;
    [SerializeField] float ApproachingCooeficient = 60000; // ChatGPT too dumb for this
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
    [SerializeField] float WallRunDelay; // If you fall off a Wall, how long until you can run again
    [SerializeField] float WallJumpForgetDelay; // How long you have to stick to a wall, for it to forget your initial touchspeed (important for smooth Walljumps)
    [SerializeField] float FOV;
    [SerializeField] KeyCode SlideKey;
    [SerializeField] float SlideCameraTransitionTime;
    [SerializeField] float SlidingAccelerationSpeed; // Used instead of AccelerationSpeed when sliding

    new Rigidbody rigidbody;
    new GameObject camera;
    Vector3 velocity;
    Quaternion Rotation;
    Quaternion CameraRotation;
    Vector3 RotationEuler;
    Vector3 CameraRotationEuler;

    List<Collider> OtherWalls;
    Collider OtherFloor;

    float LastGroundedTime;
    float LastJumpTime;
    bool IsGrounded; // Wink   <- Why did I write this?
    GameObject CurrentWall;
    GameObject LastWall;
    List<GameObject> TouchingWalls;
    float LastWallTouch;
    float LastWallJump;
    Vector3 LastVelocityAtTouch;
    bool WallRunning;
    float LastWallRun;
    float WallJumpForgetTime;
    bool IsSliding;
    float SlidingAnimationTimer;

    #endregion

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        camera = GetComponentInChildren<Camera>().gameObject;

        velocity = rigidbody.velocity;

        camera.GetComponent<Camera>().fieldOfView = FOV;

        Rotation = transform.rotation;
        CameraRotation = camera.transform.localRotation;
        RotationEuler = Rotation.eulerAngles;
        CameraRotationEuler = CameraRotation.eulerAngles;

        WallCollider.GetComponent<ColliderScript>().collided += WallCollided;
        GroundCollider.GetComponent<ColliderScript>().collided += GroundCollided;

        OtherWalls = new List<Collider>();

        LastGroundedTime = 0f;
        LastJumpTime = 0f;
        LastWallTouch = 0f;
        LastWallJump = 0f;
        LastWallRun = 0f;
        SlidingAnimationTimer = 0f;
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
        IsSliding = Input.GetKey(SlideKey);
        if (IsSliding)
        {
            LastWallTouch = WallStickTime;
        }

        #region Advance Timers
        LastGroundedTime += Time.deltaTime;
        LastJumpTime += Time.deltaTime;
        LastWallTouch += Time.deltaTime;
        LastWallJump += Time.deltaTime;
        LastWallRun += Time.deltaTime;
        WallJumpForgetTime += Time.deltaTime;
        if (IsSliding)
            SlidingAnimationTimer = Math.Min(SlidingAnimationTimer + Time.deltaTime, SlideCameraTransitionTime);
        else
            SlidingAnimationTimer = Math.Max(SlidingAnimationTimer - Time.deltaTime, 0);
        #endregion

        // Update POV
        camera.GetComponent<Camera>().fieldOfView = FOV;

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
        // The WallCollider stops working properly at heights less than 1.4 (double the Radius of 0.7), so you can't consider it's collided as correct
        // Unity should add Cylinder Colliders, even if they're inefficient, then this wouldn't be necessary
        // One way to fix this would be to make a cylinder mesh and use a mesh collider, I just don't want to do that right now
        OtherWalls.OrderBy((c) => (transform.position - c.gameObject.GetComponent<Collider>().ClosestPoint(transform.position)).magnitude)
        .ToList().ForEach(OtherWall =>
        {
            if (OtherWall == OtherFloor)
                return;
            if (LastWallJump > WallJumpDelay && LastWallRun > WallRunDelay)
                WallRunning = true;
            
            Vector3 WallAwayVector;
            Vector3 ParallelToWall;
            

            if (LastWall == OtherWall.gameObject)
            {
                LastWallTouch = 0f;
            }
            else if (CurrentWall == OtherWall.gameObject)
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
            
        });

        OtherFloor = null;
        OtherWalls = new List<Collider>();
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
        float MouseY = -Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;// Why can't I find any good Info for this
        // Mouse Movement doesn't line up with Desktop Movement, don't know why (Removing timeDelta fix it)

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

        // Change Collider Size/Camera Position depending on Sliding, allthough the Wall Collider is useless while Sliding
        camera.transform.localPosition = new Vector3(0, 2.5f - SlidingAnimationTimer / SlideCameraTransitionTime, 0); // 2.5 => Default Camera Height
        Body.GetComponent<CapsuleCollider>().height = 2 - SlidingAnimationTimer / SlideCameraTransitionTime; // 2 => Default Height
        Body.GetComponent<CapsuleCollider>().center = new Vector3(0, -SlidingAnimationTimer / SlideCameraTransitionTime / 2, 0);

        // For Cylinder Mesh Wall Collider
        WallCollider.transform.localScale = new Vector3(WallCollider.transform.localScale.x, 0.6f - SlidingAnimationTimer / SlideCameraTransitionTime / 5, WallCollider.transform.localScale.z);
        WallCollider.transform.localPosition = new Vector3(0, 2-SlidingAnimationTimer / SlideCameraTransitionTime / 2, 0);

        // For better Interaction with Slopes, Gravity is increased when on the Ground
        if (IsGrounded)
            velocity.y += Gravity * 2 * Time.deltaTime;
        else
            velocity.y += Gravity * Time.deltaTime;

        #region PlaneMovement Vector
        // for Vertical and Horizontal set:
        // Gravity: 100
        // Dead: 0.001
        // Sensitivity: 15
        float Vertical = Input.GetAxis("Vertical");
        float Horizontal = Input.GetAxis("Horizontal");
        Vector3 PlaneMovement = new Vector3(Horizontal, 0, Vertical).normalized;
        if (IsSliding)
            PlaneMovement *= SlidingAccelerationSpeed * Time.deltaTime;
        else
            PlaneMovement *= AccelerationSpeed * Time.deltaTime;
        PlaneMovement = Quaternion.AngleAxis(RotationEuler.y, Vector3.up) * PlaneMovement;
        #endregion

        float MagnitudeInMovement = MagnitudeInDirection(PlaneMovement, velocity);

        #region Friction
        if (IsGrounded && !IsSliding)
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
        velocity += PlaneMovement;
        #endregion

        if (IsGrounded) // No Wallrunning while on the Ground
            WallRunning = false;
        if (WallRunning && CurrentWall != null)
        {
            LastWallRun = 0f;
            Vector3 WallAwayVector = transform.position - CurrentWall.GetComponent<Collider>().ClosestPoint(transform.position);
            float AngleFromWallAndMovement = Vector3.Angle(PlaneMovement, WallAwayVector);
            float AngleFromWallAndVelocity = Vector3.Angle(new Vector3(velocity.x, 0, velocity.z), WallAwayVector);
            if (AngleFromWallAndMovement < 45 && PlaneMovement.magnitude > 0.01)
            {
                CurrentWall = null;
                LastWall = null;
                WallRunning = false;
            }
            else if (AngleFromWallAndVelocity < 20)
            {
                CurrentWall = null;
                LastWall = null;
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
            // Walljump
            if (CurrentWall != null && LastWallJump > WallJumpDelay && LastJumpTime > JumpDelay && !IsGrounded)
            {
                Vector3 newVelocity = new Vector3(LastVelocityAtTouch.x, 0, LastVelocityAtTouch.z);
                Vector3 WallAwayVector = transform.position - CurrentWall.GetComponent<Collider>().ClosestPoint(transform.position);
                WallAwayVector.y = 0;
                WallAwayVector = WallAwayVector.normalized;
                newVelocity = newVelocity - 2 * Vector3.Dot(newVelocity, WallAwayVector) * WallAwayVector; // Geschwindigkeit an Wand spiegeln
                newVelocity.y = velocity.y;
                velocity = newVelocity;
                velocity += WallAwayVector * JumpForce;
                velocity += PlaneMovement.normalized * JumpForce * 0.5f;
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
            // Grounded Jump
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
            Debug.Log(camera.transform.rotation * Vector3.forward);
            velocity += camera.transform.rotation * Vector3.forward * 1000;
        } // Boost in Look Direction / Simulate Rocket-Jump without Rockets being implemented

        Vector3 ActualVelocity = new Vector3(velocity.x, 0, velocity.z);
        ActualVelocity = ActualVelocity.normalized * DecreasedReturn(ActualVelocity.magnitude);
        rigidbody.velocity = new Vector3(ActualVelocity.x, velocity.y, ActualVelocity.z);

        // FOV setting / more than 60 clips the camera through Walls :(
        //camera.GetComponent<Camera>().fieldOfView = Mathf.Floor(FOV + FOV * Mathf.Pow(velocity.magnitude / 100, 0.1f));

        Debug.Log(WallRunning);
    }

    #region Save Collided
    void WallCollided(Collider other)
    {
        OtherWalls.Add(other);
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
