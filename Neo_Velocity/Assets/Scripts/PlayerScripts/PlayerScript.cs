using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public enum Tools
{
    Rocket,
    C4
}


public class PlayerScript : MonoBehaviour
{
    #region Field Declaration


    [SerializeField] bool PlayReplay = false;
    [SerializeField] bool SaveReplay = true;
    [SerializeField] bool SettingsInputs = false;

    [SerializeField] GameObject Body;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] GameObject WallCollider;
    [SerializeField] GameObject GroundCollider;

    [SerializeField] GameObject Rocket;
    [SerializeField] GameObject C4;

    [SerializeField] GameObject HandRocket;
    [SerializeField] GameObject HandC4;

    [SerializeField] float Gravity;
    [SerializeField] float AccelerationSpeed;
    [SerializeField] float SpeedCapModifier;
    [SerializeField] float TerminalVelocity;
    [SerializeField] float GroundMercyTime;
    [SerializeField] float JumpDelay;
    [SerializeField] float JumpForce;
    [SerializeField] float JumpForceAwaysFromWall;
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
    //[SerializeField] KeyCode SlideKey;     Moved to InputScript
    [SerializeField] float SlideCameraTransitionTime;
    [SerializeField] float SlidingAccelerationSpeed; // Used instead of AccelerationSpeed when sliding
    [SerializeField] float CameraTiltWhileWallrunning = 15f; // Max Angle of Camertilt, when parallel to Wall
    [SerializeField] float CameraTiltBufferChangeSpeed = 10f; // How fast the actual Tilt approaches the Buffer
    [SerializeField] float ShootDelay;
    [SerializeField] float ChangeDelay = 0.2f;
    [SerializeField] int MaxNumberOfC4 = 5;

    Func<Dictionary<string, float>> GetInput;
    Func<Vector2> GetCameraMovement;
    Action ResetInputs;
    Action<float> Finished;

    new Rigidbody rigidbody;
    new GameObject camera;
    public Vector3 velocity;
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
    float CameraTiltBuffer;
    float LastShoot;
    float LastChange;
    public bool Respawn;

    public Tools SelectedTool = Tools.Rocket;

    float RevertToCameraY;
    float RevertToCameraZ;

    float RevertToRotation;

    public bool IsGhost = false;

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

        if (SettingsInputs)
        {
            GetInput = GetComponent<Settings>().GetInput;
            GetCameraMovement = GetComponent<Settings>().GetMouseInput;
            ResetInputs = GetComponent<Settings>().ResetInputs;
            Finished = t => { };
            ResetInputs();
        }
        else if (PlayReplay)
        {
            GetInput = GetComponent<ReplayInputScript>().GetInput;
            GetCameraMovement = GetComponent<ReplayInputScript>().GetMouseInput;
            ResetInputs = GetComponent<ReplayInputScript>().ResetInputs;
            Finished = t => { };
            ResetInputs();
        }
        else
        {
            GetInput = GetComponent<PlayerInputScript>().GetInput;
            GetCameraMovement = GetComponent<PlayerInputScript>().GetMouseInput;
            ResetInputs = GetComponent<PlayerInputScript>().ResetInputs;
            GetComponent<PlayerInputScript>().SaveReplay = SaveReplay;
            Finished = GetComponent<PlayerInputScript>().Finished;
            if (SaveReplay)
                ResetInputs();
        }
        
        RevertToCameraY = 0;
        RevertToCameraZ = 0;
        RevertToRotation = 0;

        OtherWalls = new List<Collider>();

        LastGroundedTime = 0f;
        LastJumpTime = 0f;
        LastWallTouch = 0f;
        LastWallJump = 0f;
        LastWallRun = 0f;
        SlidingAnimationTimer = 0f;
        LastShoot = 0f;

        HandC4.SetActive(false);
        HandRocket.SetActive(true);

        if (!SettingsInputs)
            Cursor.lockState = CursorLockMode.Locked;
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

    // Camera Movement, unrelated from the recorded inputs
    private void Update()
    {
        Vector2 MouseMovement = GetCameraMovement();

        float AmpMouseX = MouseMovement.x;
        float AmpMouseY = -MouseMovement.y;

        #region Camera Rotation

        #region Approach Cameratiltbuffer
        if (CameraRotationEuler.z - CameraTiltBuffer < 0)
        {
            CameraRotationEuler.z += CameraTiltBufferChangeSpeed * Time.deltaTime;
            if (CameraRotationEuler.z > CameraTiltBuffer)
                CameraRotationEuler.z = CameraTiltBuffer;
        }
        else if (CameraRotationEuler.z - CameraTiltBuffer > 0)
        {
            CameraRotationEuler.z -= CameraTiltBufferChangeSpeed * Time.deltaTime;
            if (CameraRotationEuler.z < CameraTiltBuffer)
                CameraRotationEuler.z = CameraTiltBuffer;
        }
        #endregion

        CameraRotationEuler.x += AmpMouseY;
        if (CameraRotationEuler.x > 90)
            CameraRotationEuler.x = 90;
        else if (CameraRotationEuler.x < -90)
            CameraRotationEuler.x = -90;

        Quaternion temp = Rocket.transform.rotation;
        temp.eulerAngles = new Vector3(CameraRotationEuler.x, Rocket.transform.rotation.eulerAngles.y);
        Rocket.transform.rotation = temp;
        temp = C4.transform.rotation;
        temp.eulerAngles = new Vector3(CameraRotationEuler.x, C4.transform.rotation.eulerAngles.y);
        C4.transform.rotation = temp;

        RotationEuler.y += AmpMouseX;
        Rotation.eulerAngles = RotationEuler;
        transform.rotation = Rotation;

        CameraRotation.eulerAngles = CameraRotationEuler;
        camera.transform.localRotation = CameraRotation;
        #endregion
    }

    // Everthing else
    void FixedUpdate()
    {
        Dictionary<string, float> input = GetInput();
        if (input == null)
        {
            rigidbody.velocity = Vector3.zero;
            return;
        }
            

        // This causes as many problems as it fixes, so no
        // velocity = rigidbody.velocity;

        IsSliding = input["Sliding"] == 1f;
        if (IsSliding)
        {
            // SOUND-Sliding
            LastWallTouch = WallStickTime;
        }

        #region Advance Timers
        LastGroundedTime += Time.fixedDeltaTime;
        LastJumpTime += Time.fixedDeltaTime;
        LastWallTouch += Time.fixedDeltaTime;
        LastWallJump += Time.fixedDeltaTime;
        LastWallRun += Time.fixedDeltaTime;
        WallJumpForgetTime += Time.fixedDeltaTime;
        LastShoot += Time.fixedDeltaTime;
        LastChange += Time.fixedDeltaTime;
        if (IsSliding)
            SlidingAnimationTimer = Math.Min(SlidingAnimationTimer + Time.deltaTime, SlideCameraTransitionTime);
        else
            SlidingAnimationTimer = Math.Max(SlidingAnimationTimer - Time.deltaTime, 0);
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
        float MouseX = input["Mouse X"];
        float MouseY = -input["Mouse Y"];

        CameraRotationEuler.y = 0;
        #region General Rotation
        RotationEuler.y = RevertToRotation;
        RotationEuler.y += MouseX;
        Rotation.eulerAngles = RotationEuler;
        transform.rotation = Rotation;
        RevertToRotation = RotationEuler.y;
        #endregion
        // Camera Rotation moved down to Wallrunning Section for Camera tilting

        // Change Collider Size/Camera Position depending on Sliding
        camera.transform.localPosition = new Vector3(0, 2.5f - SlidingAnimationTimer / SlideCameraTransitionTime, 0); // 2.5 => Default, 1.5 => Sliding
        if (IsSliding)
        {
            Body.GetComponent<CapsuleCollider>().height = 1; // 2 => Default
            Body.GetComponent<CapsuleCollider>().center = new Vector3(0, -0.5f, 0); // 0 => Default

            WallCollider.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f); // 0.6 => Default
            WallCollider.transform.localPosition = new Vector3(0, 1.5f, 0); // 2 => Default
        } // Colliders change instantly, to make Sliding feel better
        else
        {
            Body.GetComponent<CapsuleCollider>().height = 2; // 1 => Sliding
            Body.GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0); // -0.5f => Sliding

            WallCollider.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f); // 0.4 => Sliding
            WallCollider.transform.localPosition = new Vector3(0, 2f, 0); // 1.5 => Sliding
        }

        // For better Interaction with Slopes, Gravity is increased when on the Ground, don't know if this actually works
        if (IsGrounded)
            velocity.y += Gravity * 2 * Time.deltaTime;
        else
            velocity.y += Gravity * Time.deltaTime;

        #region PlaneMovement Vector
        // for Vertical and Horizontal set:
        // Gravity: 100
        // Dead: 0.001
        // Sensitivity: 15
        float Vertical = input["Vertical"];
        float Horizontal = input["Horizontal"];
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
        velocity += PlaneMovement / (1 + (new Vector3(velocity.x, 0, velocity.z).magnitude * SpeedCapModifier) * (1 - Vector3.Angle(new Vector3(velocity.x, 0, velocity.z), PlaneMovement.normalized) / 180));
        #endregion

        if (IsGrounded) // No Wallrunning while on the Ground
            WallRunning = false;

        CameraTiltBuffer = 0; // Reset Camera-tilt before Wallrun-eval

        if (CurrentWall != null)
            if (CurrentWall.tag == "No_Wallrun") // No Wallruns on this Wall
                WallRunning = false;

        if (WallRunning && CurrentWall != null)
        {
            // SOUND-Wallrun
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

            // Camera-tilting
            float AngleLookToNormalWall = Vector3.Angle(WallAwayVector, transform.rotation * Vector3.forward);
            float AngleLookToParallelWall = Vector3.Angle(Quaternion.AngleAxis(90, Vector3.up) * WallAwayVector, transform.rotation * Vector3.forward);
            float TiltFactor; // How strong to tilt
            if (AngleLookToNormalWall > 180)
                TiltFactor = Math.Min((360 - AngleLookToNormalWall) / 90, 1);
            else
                TiltFactor = Math.Min(AngleLookToNormalWall / 90, 1);
            float TiltDirection = 1;
            if (AngleLookToParallelWall > 90)
                TiltDirection = -1;
            CameraTiltBuffer = TiltDirection * TiltFactor * CameraTiltWhileWallrunning;
        } // everything with Wallrunning (letting go, snapping position to Wall, stopping down velocity, also Camera-tilting)

        #region Camera Rotation
        #region Approach Cameratiltbuffer
        CameraRotationEuler.z = RevertToCameraZ;
        if (CameraRotationEuler.z - CameraTiltBuffer < 0)
        {
            CameraRotationEuler.z += CameraTiltBufferChangeSpeed * Time.deltaTime;
            if (CameraRotationEuler.z > CameraTiltBuffer)
                CameraRotationEuler.z = CameraTiltBuffer;
        }
        else if (CameraRotationEuler.z - CameraTiltBuffer > 0)
        {
            CameraRotationEuler.z -= CameraTiltBufferChangeSpeed * Time.deltaTime;
            if (CameraRotationEuler.z < CameraTiltBuffer)
                CameraRotationEuler.z = CameraTiltBuffer;
        }
        RevertToCameraZ = CameraRotationEuler.z;
        #endregion

        CameraRotationEuler.x = RevertToCameraY;
        CameraRotationEuler.x += MouseY;
        if (CameraRotationEuler.x > 90)
            CameraRotationEuler.x = 90;
        else if (CameraRotationEuler.x < -90)
            CameraRotationEuler.x = -90;
        CameraRotation.eulerAngles = CameraRotationEuler;
        camera.transform.localRotation = CameraRotation;
        RevertToCameraY = CameraRotationEuler.x;

        /*
        Quaternion temp = Rocket.transform.localRotation;
        temp.eulerAngles = new Vector3(CameraRotationEuler.x, Rocket.transform.localRotation.eulerAngles.y);
        Rocket.transform.localRotation = temp;
        temp = C4.transform.localRotation;
        temp.eulerAngles = new Vector3(CameraRotationEuler.x, C4.transform.localRotation.eulerAngles.y);
        C4.transform.localRotation = temp;
        */
        #endregion

        if (WallJumpForgetTime > WallJumpForgetDelay)
        {
            LastVelocityAtTouch = velocity;
        } // Reset Walljump reflexion-direction to velocity

        if (input["Jump"] == 1f)
        {
            // Walljump
            if (CurrentWall != null && LastWallJump > WallJumpDelay && LastJumpTime > JumpDelay && !IsGrounded)
            {
                // SOUND-Walljump
                Vector3 newVelocity = new Vector3(LastVelocityAtTouch.x, 0, LastVelocityAtTouch.z);
                Vector3 WallAwayVector = transform.position - CurrentWall.GetComponent<Collider>().ClosestPoint(transform.position);
                WallAwayVector.y = 0;
                WallAwayVector = WallAwayVector.normalized;
                newVelocity = newVelocity - 2 * Vector3.Dot(newVelocity, WallAwayVector) * WallAwayVector; // mirror Speed at the Wall
                newVelocity += WallAwayVector * (JumpForceAwaysFromWall / (1 + (new Vector3(newVelocity.x, 0, newVelocity.z).magnitude * SpeedCapModifier)));
                newVelocity += PlaneMovement.normalized * (JumpForce / (1 + (new Vector3(newVelocity.x, 0, newVelocity.z).magnitude * SpeedCapModifier) * (1 - Vector3.Angle(newVelocity, PlaneMovement.normalized) / 180) )) * 0.5f;
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
            // Grounded Jump
            else if (LastJumpTime > JumpDelay && IsGrounded)
            {
                // SOUND-Grounded_Jump
                velocity.y += JumpForce;
                LastJumpTime = 0f;
                LastGroundedTime = GroundMercyTime;
            }
        } // Jump

        if (input["Change"] == 1f && LastChange >= ChangeDelay) {
            LastChange = 0f;
            if (SelectedTool == Tools.Rocket)
            {
                SelectedTool = Tools.C4;
                HandC4.SetActive(true);
                HandRocket.SetActive(false);
            }
            else
            {
                SelectedTool = Tools.Rocket;
                HandC4.SetActive(false);
                HandRocket.SetActive(true);
            }
        }

        if (input["Shoot"] == 1f && LastShoot >= ShootDelay) // SHOOT
        {
            GameObject Projectile = null;
            if (SelectedTool == Tools.Rocket)
            {
                // SOUND-Rocketlaunch
                Projectile = Instantiate(Rocket, camera.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation);
                if (IsGhost)
                    Projectile.GetComponent<RocketScript>().MakeGhost();
                LastShoot = 0f;
            }
            else if (SelectedTool == Tools.C4)
            {
                // SOUND-C4_Throw
                string tag = IsGhost ? "Ghost_C4" : "C4";
                if (GameObject.FindGameObjectsWithTag(tag).Length < MaxNumberOfC4)
                {
                    Projectile = Instantiate(C4, camera.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation);
                    if (IsGhost)
                        Projectile.GetComponent<C4Script>().MakeGhost();
                    LastShoot = 0f;
                }
            }
        }

        if (input["Activate"] == 1f)
        {
            // SOUND-C4_Activate
            GameObject.FindGameObjectsWithTag("C4").ToList().ForEach(o => o.GetComponent<C4Script>().Explode());
        }

        if (velocity.y < TerminalVelocity)
        {
            velocity.y = TerminalVelocity;
        } // Caps falling-speed at TerminalVelocity

        rigidbody.velocity = velocity;

        if (GetComponent<TimeMeasure>().TimeToFinish != null)
        {
            Finished((float)GetComponent<TimeMeasure>().TimeToFinish);
        }

        if (Respawn || input["Respawn"] == 1f)
        {
            GameObject.FindGameObjectsWithTag("Player").ToList().Where(o => o.GetComponent<PlayerScript>().IsGhost)
                .ToList().ForEach(o => o.GetComponent<PlayerScript>().Respawn = true);
            GameObject.FindGameObjectsWithTag("C4").ToList().ForEach(o => Destroy(o));
            GameObject.FindGameObjectsWithTag("Rocket").ToList().ForEach(o => Destroy(o));
            velocity = Vector3.zero;
            rigidbody.velocity = velocity;
            transform.position = GameObject.FindGameObjectWithTag("Spawn").transform.position;
            RotationEuler = new Vector3(0, GameObject.FindGameObjectWithTag("Spawn").transform.rotation.eulerAngles.y);
            Rotation.eulerAngles = RotationEuler;
            CameraRotationEuler = new Vector3(GameObject.FindGameObjectWithTag("Spawn").transform.rotation.eulerAngles.x, 0);
            CameraRotation.eulerAngles = CameraRotationEuler;
            camera.transform.rotation = CameraRotation;
            RevertToCameraY = CameraRotationEuler.x;
            RevertToCameraZ = CameraRotationEuler.z;

            GetComponent<TimeMeasure>().TimeToFinish = null;

            if (SaveReplay || PlayReplay)
                ResetInputs();
        }
        Respawn = false;

        /*
         * temp
        Debug.Log(CurrentWall);
        if (LastPainted)
            LastPainted.GetComponent<MeshRenderer>().material.SetColor("BLUE", new Color(0, 0, 255));
        if (CurrentWall)
        {
            CurrentWall.GetComponent<MeshRenderer>().material.SetColor("RED", new Color(255, 0, 0));
            LastPainted = CurrentWall;
        }
        */

        // FOV setting / more than 60 clips the camera through Walls :(
        //camera.GetComponent<Camera>().fieldOfView = Mathf.Floor(FOV + FOV * Mathf.Pow(velocity.magnitude / 100, 0.1f));

        // Issue with the way wallrunning is handled,
        // if you touch 2 Walls and move slightly away from the main one,
        // it doesn't snap you to the second one
    }

    public void MakeGhost()
    {
        // Transparency
        IsGhost = true;
        var children = GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            child.gameObject.layer = 8;
        }
        gameObject.layer = 8; // Ghost Layer (spooky)
        GetComponentInChildren<AudioListener>().enabled = false;
        GetComponentInChildren<Camera>().enabled = false;
        SaveReplay = false;
        GetComponent<TimeMeasure>().enabled = false;
        PlayReplay = true;
    }
    public void WatchGhost()
    {
        SaveReplay = false;
        PlayReplay = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        List<ContactPoint> contacts = new List<ContactPoint>();
        collision.GetContacts(contacts);

        contacts.ForEach(c =>
        {
            if (Vector3.Angle(c.normal, Vector3.up) >= 150f)
            {
                velocity.y = 0;
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
            }
        });
    }

    #region Save Collided
    void WallCollided(Collider other)
    {
        if (other.gameObject.tag == "Laser")
            Respawn = true;
        OtherWalls.Add(other);
    }
    void GroundCollided(Collider other)
    {
        if (other.gameObject.tag == "Laser")
            Respawn = true;
        OtherFloor = other;
    }
    #endregion

}
