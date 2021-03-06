using System;
using UnityEngine;

public class RigidBodyPlayerMovement : MonoBehaviour
{
    public Transform playerCam;
    public Transform orientation, gunLookAt;
    public Transform playerBody;

    //Other
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    public float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    [HideInInspector]
    public float recoilRoughness = 5f;
    public static RigidBodyPlayerMovement instance;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    public Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public float crouchMoveSpeed = 0.4f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    private float reference;
    //Input
    float x, y;
    bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;
    private Vector3 refCrouchScale;

    void Awake()
    {
        refCrouchScale = transform.localScale;
        rb = GetComponent<Rigidbody>();
        instance = this;
    }

    void Start()
    {
        playerScale = playerBody.localScale;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void FixedUpdate()
    {
        if (GameManager.instance.gameIsPaused) return;
        Movement();
    }

    private void Update()
    {
        if (GameManager.instance.gameIsPaused) return;
        MyInput();
        Look();
        IncreaseBulletSpread();
        playerBody.localScale = Vector3.Lerp(playerBody.localScale, refCrouchScale, 10f * Time.smoothDeltaTime);
        WeaponHolder.instance.transform.localPosition = Vector3.Lerp(WeaponHolder.instance.transform.localPosition, new Vector3(0f, 0.86f, 0f), 10f * Time.smoothDeltaTime);
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);
        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }
    private void StartCrouch()
    {
        refCrouchScale = crouchScale;
    }

    private void StopCrouch()
    {
        refCrouchScale = new Vector3(1f, 1f, 1f);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.1f;
            multiplierV = 0.1f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = crouchMoveSpeed;

        //Apply forces to move player

        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);

        //send position to server
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void WeaopnBobbing()
    {
        WeaponHolder.instance.transform.localPosition = Vector3.Lerp(WeaponHolder.instance.transform.localPosition, new Vector3((Mathf.Sin(Time.time * 5f) * 0.07f), 0.86f + (Mathf.Sin(Time.time * 5f) * 0.1f), 0f), 5f * Time.smoothDeltaTime);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private float desiredX;
    public Transform recoilCamera;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        recoilCamera.localRotation = Quaternion.Lerp(recoilCamera.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 0f)), recoilRoughness * Time.smoothDeltaTime);
        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }


    private void IncreaseBulletSpread()
    {
        if (x > 0f || x < 0f || y < 0f || y > 0f)
        {
            if (grounded)
            {
                if (crouching)
                    for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                    {
                        WeaponHolder.instance.gun[i].IncreaseBulletSpread(WeaponHolder.instance.gun[i].crouchingBulletSpreadFactor);
                    }
                else if(rb.velocity.magnitude > 0.1f)
                {
                    for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                    {
                        WeaponHolder.instance.gun[i].IncreaseBulletSpread(WeaponHolder.instance.gun[i].movingBulletSpread);
                        WeaopnBobbing();
                    }
                }
                else
                {
                    for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                    {
                        WeaponHolder.instance.gun[i].IncreaseBulletSpread(1f);
                    }
                }
                
            }
            else
            {
                for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                {
                    WeaponHolder.instance.gun[i].IncreaseBulletSpread(WeaponHolder.instance.gun[i].inAirBulletSpreadFactor);
                }
            }
        }
        else
        {
            if (grounded)
            {
                if (crouching)
                    for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                    {
                        WeaponHolder.instance.gun[i].IncreaseBulletSpread(WeaponHolder.instance.gun[i].crouchingBulletSpreadFactor);
                    }
                else
                {
                    for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                    {
                        WeaponHolder.instance.gun[i].IncreaseBulletSpread(1f);
                    }
                }
            }
            else
            {
                for (int i = 0; i < WeaponHolder.instance.gun.Length; i++)
                {
                    WeaponHolder.instance.gun[i].IncreaseBulletSpread(WeaponHolder.instance.gun[i].inAirBulletSpreadFactor);
                }
            }
        }
    }

}
