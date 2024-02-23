using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlBase : MonoBehaviour
{
    [Header("Object Declarations")]
    [SerializeField] Rigidbody RB;
    [SerializeField] Transform playerTransform;
    [SerializeField] Transform cameraTransform;
    [SerializeField] SphereCollider sphereCollider;
    private PlayerInputActions playerInputActions;

    [Header("Movement")]
    [SerializeField] float acceleration;
    [SerializeField] float accelerationFriction;
    [SerializeField] float groundMaxSpeed;
    float turnSpeed;
    [SerializeField, Range(0, 1)] float sidewaysDampening;

    [Header("Ground detection")]
    [SerializeField] float rayOriginOffset;
    [SerializeField] float groundHitRange;
    [SerializeField] LayerMask groundLayer;

    //physics variables
    private Vector3 velocity;

    //ground detection variables
    private bool grounded;
    private RaycastHit groundedRayPoint;

    //Miscalenous values used to tune raycasts

    private enum State
    {
        Ground,
        Air
    }
    private State playerState;

    /// <summary>
    /// 
    /// 
    /// 
    /// 
    /// Main Method Declarations
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    
    private void Awake()
    {
        //enable player input script.
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PhysicsCalcInit();
        GroundedCheck();
        StateSwitch();
        if (playerState == State.Ground) { Snap(); }
        Vector2 moveDir = GetMovementVectorNormalized();
        TurnSpeedCalc();
        Turn(moveDir);
        Move(moveDir);
        Friction();
        //SideVelocityDamping();
        ApplyForces();
        //Debug.Log("speed diff: " + Mathf.Clamp(0.3f * Mathf.Pow(Mathf.Pow(0.075f / 0.3f, 1 / groundMaxSpeed), RB.velocity.magnitude), 0.025f, 0.3f));
    }

    /// <summary>
    /// 
    /// 
    /// 
    /// 
    /// Supplemental Method Declarations
    /// 
    /// 
    /// ************
    /// stuff to add:
    /// the turn speed needs to be calculated real time depending on the player speed in TurnSpeedCalc()
    /// the player acceleration needs to be affected be strong turns, potentially implement this in some helper method used in Move().
    /// Both the above systems should only be present on ground.
    /// *****************
    /// 
    /// 
    /// </summary>

    // This function takes the player's input and returns a vector2 output that represents the direction
    // the player wants to move in where 0,0 is forward
    private Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Keyboard.Move.ReadValue<Vector2>();
        inputVector = inputVector.magnitude > 1 ? inputVector.normalized : inputVector;
        return inputVector;
    }

    //resets the value of the physics after each cycle to allow for a new calculation
    private void PhysicsCalcInit()
    {
        velocity = Vector3.zero;
    }

    private void TurnSpeedCalc()
    {
        //turnSpeed = Mathf.Clamp((float)((groundMaxSpeed - RB.velocity.magnitude) / groundMaxSpeed + 0.08), 0.025f, 0.65f);
        float b = Mathf.Pow(0.075f / 0.3f, 1 / groundMaxSpeed);
        turnSpeed = Mathf.Clamp(0.3f * Mathf.Pow(b,RB.velocity.magnitude), 0.025f, 0.3f);
    }

    //this function rotates the player to face the inputted direction whilst leving them facing that way otherwise
    private void Turn(Vector2 direction)
    {
        //Check if input is active
        if (direction != Vector2.zero) 
        {
            // calculate the forward orientation of the camera
            Vector3 cameraFlatForward = Vector3.ProjectOnPlane(cameraTransform.forward, playerTransform.up).normalized;

            // calculate the angle of rotation of the player
            float rawRotationAngle = Mathf.Atan2(direction.x, direction.y);

            // convert the rotation to a quaternion and apply the rotation with interpolation to smooth motion
            Quaternion rotation = Quaternion.AngleAxis(rawRotationAngle * Mathf.Rad2Deg, playerTransform.up);
            Vector3 finalDirection = Vector3.Slerp(playerTransform.forward, ((rotation * cameraFlatForward).normalized), turnSpeed);
            playerTransform.forward = finalDirection;

            //side sway elimination (works)
            Vector3 tempV = RB.velocity;
            RB.velocity = Vector3.zero;
            RB.velocity = finalDirection * tempV.magnitude;

            
        } 
    }

    // applies the calculated velocity of the current cycle to the Player
    private void ApplyForces()
    {
        RB.velocity += velocity;
    }

    // Adds force in forward direction if player is moving
    private void Move(Vector2 direction)
    {
        if (direction.magnitude > 0) {
            float rawInputRotationAngle = Mathf.Abs(Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            Vector3 cameraFlatForward = Vector3.ProjectOnPlane(cameraTransform.forward, playerTransform.up).normalized;
            Vector3 playerFlatForward = Vector3.ProjectOnPlane(playerTransform.forward, playerTransform.up).normalized;
            float angleDiff = Mathf.Abs(Vector3.Angle(cameraFlatForward, playerFlatForward));
            Debug.Log("inputAngle: " + (rawInputRotationAngle - angleDiff));
            if ((rawInputRotationAngle - angleDiff) < 50)
            {
                velocity += acceleration * playerTransform.forward.normalized;
            } 
            else
            {
                //velocity += acceleration / 4 * playerTransform.forward.normalized;
            }
            
        }
    }

    // funtion to apply friction to motion of player on ground 
    private void Friction()
    {
        if (RB.velocity.magnitude != 0f)
        {
            if (RB.velocity.magnitude >= groundMaxSpeed)
            {
                velocity -= RB.velocity.normalized * acceleration;
            }
            else
            {

                if (RB.velocity.magnitude < accelerationFriction)
                {
                    velocity -= RB.velocity;
                }
                else
                {
                    velocity -= RB.velocity.normalized * accelerationFriction;
                }

            }
        }
    }

    // Checks if player is in grounding range
    private void GroundedCheck()
    {
        grounded = Physics.Raycast(playerTransform.position + (playerTransform.up.normalized * rayOriginOffset), -playerTransform.up, out groundedRayPoint, groundHitRange + rayOriginOffset, groundLayer);
    }

    // Switches player's state depending on if they are grounded or not
    private void StateSwitch()
    {
        if (grounded)
        {
            playerState = State.Ground;
        }
        else
        {
            playerState = State.Air;
        }
    }

    // snaps player position to ground so they dont end up floating
    private void Snap()
    {
        playerTransform.position = groundedRayPoint.point;
    }
}
