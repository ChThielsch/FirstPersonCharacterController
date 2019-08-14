using UnityEngine;

//Script by Christian Thielsch 2019
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    #region public variables
    //Debug
    public bool m_debugView = false;
    public Color m_crouchColliderColor = Color.green;
    public Color m_groundCheckRaycastColor = Color.green;
    public Color m_groundNormalRaycastColor = Color.green;
    public Color m_groundSlopeRaycastColor = Color.green;
    public Color m_crouchSpherecastColor = Color.green;

    //General
    public float m_health = 100;
    public float m_stamina = 100;
    public float m_breath = 20;


    //Movement
    public bool m_freezeInputs = false;
    public float m_walkSpeed = 10.0f;
    public float m_sprintSpeed = 15.0f;
    public float m_crouchSpeed = 8.0f;
    public float m_velocityChange = 10.0f;

    //Advanced Settings
    public float m_playerHeight = 1.7f;
    public float m_crouchHeight = 1f;
    public float m_playerWidth = 0.5f;
    public float m_playerMass = 1f;
    public float m_staminaFactor = 1f;
    public float m_crouchTime = 2f;
    public float m_speedFactor = 4f;
    public float m_climbMinHeight = 1f;
    public float m_climbMaxHeight = 2f;
    public float m_climbMaxDistance = 1f;
    public float m_timeToClimb = 1;

    //Other
    public bool m_canJump = true;
    public float m_jumpHeight = 2.0f;
    public float m_gravity = 10.0f;
    #endregion

    #region hide in inspector public variables
    /// <summary>
    /// the controller script on the camera
    /// </summary>
    private CameraController m_cameraController;

    /// <summary>
    /// the original rotation as quaternion
    /// </summary>
    public Quaternion m_originalRotation;

    /// <summary>
    /// the different states of the player sorted by sound volumn
    /// </summary>
    public enum State { HoldingBreath, Idle, Crouching, Walking, Sprinting }

    /// <summary>
    /// the current state the player is in
    /// </summary>
    public State playerState;

    /// <summary>
    /// the gameObjects rigidbody
    /// </summary>
    public Rigidbody m_rigidbody;

    /// <summary>
    /// the current movement speed
    /// </summary>
    public float m_currentMovementSpeed;

    /// <summary>
    /// the current height of the players collider
    /// </summary>
    public float m_currentPlayerHeight = 1.7f;

    #endregion

    #region private variables
    /// <summary>
    /// the current amount of health
    /// </summary>
    private float m_currentHealth = 100;

    /// <summary>
    /// the current ammount of stamina
    /// </summary>
    private float m_currentStamina = 100;

    /// <summary>
    /// the current ammount of breath in seconds
    /// </summary>
    private float m_currentBreath = 100;

    /// <summary>
    /// the factor of height added to the players collider based on the height
    /// </summary>
    private float m_colliderfactor = 0.15f;

    /// <summary>
    /// the radius of the players collider
    /// </summary>
    private float m_colliderRadius = 0.5f;

    /// <summary>
    ///  is true when the player is crouching
    /// </summary>
    private bool m_isCrouching = false;

    /// <summary>
    ///  is true when the player is sprinting
    /// </summary>
    private bool m_isSprinting = false;

    private bool m_isClimbing = false;

    private Vector3 m_climbPosition;

    private Vector3 m_climbStartPosition;

    private float m_currentClimbTime = 0;


    /// <summary>
    /// the players capsul collider
    /// </summary>
    private CapsuleCollider m_playerCollider;

    /// <summary>
    /// the gameObject which is hit by the ground check
    /// </summary>
    private RaycastHit m_groundHit;
    #endregion

    #region unityFunctions
    private void OnValidate()
    {
        //clamping values to not be able to be set below a given value
        m_stamina = Mathf.Clamp(m_stamina, 0, float.MaxValue);
        m_health = Mathf.Clamp(m_health, 0, float.MaxValue);
        m_walkSpeed = Mathf.Clamp(m_walkSpeed, 0, float.MaxValue);
        m_sprintSpeed = Mathf.Clamp(m_sprintSpeed, 0, float.MaxValue);
        m_crouchSpeed = Mathf.Clamp(m_crouchSpeed, 0, float.MaxValue);
        m_velocityChange = Mathf.Clamp(m_velocityChange, 0, float.MaxValue);
        m_playerHeight = Mathf.Clamp(m_playerHeight, 0, float.MaxValue);
        m_crouchHeight = Mathf.Clamp(m_crouchHeight, 0, float.MaxValue);
        m_playerWidth = Mathf.Clamp(m_playerWidth, 0, float.MaxValue);
        m_playerMass = Mathf.Clamp(m_playerMass, 0, float.MaxValue);
        m_climbMinHeight = Mathf.Clamp(m_climbMinHeight, 0, float.MaxValue);
        m_climbMaxHeight = Mathf.Clamp(m_climbMaxHeight, m_climbMinHeight, float.MaxValue);
        m_climbMaxDistance = Mathf.Clamp(m_climbMaxDistance, 0, float.MaxValue);
    }

    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        m_rigidbody = GetComponent<Rigidbody>();
        m_playerCollider = GetComponent<CapsuleCollider>();
        m_cameraController = GameObject.Find("MainCamera").GetComponent<CameraController>();


    m_colliderRadius = m_playerCollider.radius;

        //Add Collider and Rigidbody if not existing
        if (m_playerCollider == null)
        {
            gameObject.AddComponent(typeof(CapsuleCollider));
        }
        if (m_rigidbody == null)
        {
            gameObject.AddComponent(typeof(Rigidbody));
        }

        m_rigidbody.freezeRotation = true;
        m_rigidbody.useGravity = false;
        m_playerCollider.center = new Vector3(0, m_playerHeight * 0.5f, 0);
        m_playerCollider.radius = m_playerWidth;
        m_rigidbody.mass = m_playerMass;

        //Sets the players height
        ChangePlayerHeight(m_playerHeight, 0);

        m_originalRotation = transform.localRotation;
        m_currentMovementSpeed = m_walkSpeed;
    }

    void FixedUpdate()
    {
        if (grounded() && !m_freezeInputs)
        {
            // Calculates how fast the player moves
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= m_currentMovementSpeed;

            // Adds the grounds direction to the movement
            targetVelocity = Vector3.Cross(targetVelocity, m_groundHit.normal);
            targetVelocity = Vector3.Cross(-targetVelocity, m_groundHit.normal);

            // Applys a force that attempts to reach the target velocity
            Vector3 velocity = m_rigidbody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);

            velocityChange.x = Mathf.Clamp(velocityChange.x, -m_velocityChange, m_velocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -m_velocityChange, m_velocityChange);
            velocityChange.y = 0;

            m_rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            if (m_canJump && Input.GetButtonDown("Jump") && playerState != State.Crouching)
            {
                m_rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }

            if (m_debugView)
            {
                Debug.DrawRay(transform.position, targetVelocity * 0.5f, m_groundSlopeRaycastColor, 0.05f);
            }
        }

        // Manual gravity for more control
        m_rigidbody.AddForce(new Vector3(0, -m_gravity * m_rigidbody.mass, 0));
    }

    private void Update()
    {
        Walk();
        Sprinting();
        Crouching();
        HoldingBreath();
        Climping();

        m_currentPlayerHeight = m_playerCollider.height - (m_colliderfactor * m_playerCollider.height);

        if (m_debugView)
        {
            Debug.Log("Player State: " + playerState + " | Movement Speed: " + m_currentMovementSpeed + " | Stamina: " + m_currentStamina);
        }

        // Stamina
        if (m_currentStamina > 0)
        {
            if (playerState == State.Sprinting || playerState == State.HoldingBreath)
            {
                m_currentStamina -= Time.deltaTime * m_staminaFactor;
            }
        }

        if (m_currentStamina < m_stamina && playerState != State.Sprinting && playerState != State.HoldingBreath)
        {
            m_currentStamina += Time.deltaTime;
        }

        if (m_currentStamina < 0)
        {
            m_currentStamina = 0;
            m_isSprinting = false;
        }

        if (m_currentStamina > m_stamina)
        {
            m_currentStamina = m_stamina;
        }

        // Breath
        if (m_currentBreath < m_breath && playerState != State.HoldingBreath)
        {
            m_currentBreath += Time.deltaTime;
        }

        if (m_breath < 0)
        {
            m_breath = 0;
        }
        if (m_currentBreath > m_breath)
        {
            m_currentBreath = m_breath;
        }

        // Camera height seperated from Crouch function so its done every update without conditions
        if (m_isCrouching)
        {
            ChangePlayerHeight(m_crouchHeight, m_crouchTime);
        }
        else if (Camera.main.transform.position.y < m_playerHeight)
        {
            ChangePlayerHeight(m_playerHeight, m_crouchTime);
        }


        // Sprintig speed seperated from Sprinting function so its caluclated every update
        if (m_isSprinting)
        {
            // stamina / (100/Abstuffung) = 6,79 => (int)6,79 = 6 + 1 = 7  7/ABSTUFUNNGEN * Speed
            m_currentMovementSpeed = m_walkSpeed + (int)((m_currentStamina / (100 / m_speedFactor)) + 1) * ((m_sprintSpeed - m_walkSpeed) / m_speedFactor);
        }
        else if (!m_isCrouching)
        {
            m_currentMovementSpeed = m_walkSpeed;
        }

        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }

        if (transform.position.y <= -20)
        {
            transform.position = new Vector3(0, 0, 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        m_playerCollider = GetComponent<CapsuleCollider>();

        if (m_debugView)
        {
            // Crouch Collider           
            Gizmos.color = m_crouchColliderColor;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + m_crouchHeight * 0.5f, transform.position.z), 0.5f);

            // Crouch SphereCast
            Gizmos.color = m_crouchSpherecastColor;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + m_playerHeight - m_colliderRadius, transform.position.z), m_colliderRadius);

            //Ground Check
            Gizmos.color = m_groundCheckRaycastColor;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + m_colliderRadius - 0.1f, transform.position.z), m_colliderRadius - 0.025f);
        }
    }
    #endregion

    #region ownFunctions

    /// <summary>
    /// shoots a raycast down to check if the player touches the ground
    /// </summary>
    /// <returns></returns>
    public bool grounded()
    {
        if (Physics.SphereCast(new Vector3(transform.position.x, transform.position.y + m_colliderRadius * 2, transform.position.z), m_colliderRadius - 0.025f, -transform.up, out m_groundHit, 0.1f + m_colliderRadius))
        {
            if (m_groundHit.transform != transform.parent)
            {
                transform.parent = m_groundHit.transform;
            }

            if (m_debugView)
            {
                //Ground Normal
                Debug.DrawRay(m_groundHit.point, m_groundHit.normal, m_groundNormalRaycastColor, 0.05f);
            }

            if (Vector3.Angle(m_groundHit.normal, Vector3.up) != 0)
            {
                m_gravity = 20;
            }
            else
            {
                m_gravity = 10;
            }
            return true;
        }
        else
        {
            transform.parent = null;
            return false;
        }
    }

    /// <summary>
    /// return true if the player inputs Horizontal or Vertical axis
    /// </summary>
    /// <returns></returns>
    public void Walk()
    {
        if (playerState != State.Sprinting && playerState != State.Crouching && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            playerState = State.Walking;
        }
        if (playerState != State.Sprinting && playerState != State.Crouching && (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0))
        {
            playerState = State.Idle;
        }
    }

    /// <summary>
    /// Return true if button pressed, not holding breath and stamina > 0
    /// </summary>
    /// <returns></returns>
    public void Sprinting()
    {
        if (playerState != State.HoldingBreath && playerState != State.Crouching && m_currentStamina > 0 && Input.GetButtonDown("Sprint"))
        {
            m_isSprinting = true;
            playerState = State.Sprinting;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            m_isSprinting = false;
            playerState = State.Idle;
        }
    }

    /// <summary>
    /// Return true if button pressed, not sprinting and stamina > 0
    /// </summary>
    /// <returns></returns>
    public void HoldingBreath()
    {
        if (playerState != State.Sprinting && m_currentStamina > 0 && m_breath != 0 && Input.GetButtonDown("Hold Breath"))
        {
            m_currentBreath -= Time.deltaTime;
            playerState = State.HoldingBreath;
        }
        if (Input.GetButtonUp("Hold Breath"))
        {
            playerState = State.Idle;
        }
    }

    /// <summary>
    /// Returns true if botton pressed, not sprinting and stamina > 0
    /// </summary>
    /// <returns></returns>
    public void Crouching()
    {
        if (playerState != State.Sprinting && Input.GetButtonDown("Crouch"))
        {
            m_isCrouching = true;
            m_currentMovementSpeed = m_crouchSpeed;
            playerState = State.Crouching;
        }

        if (playerState != State.Sprinting && !Input.GetButton("Crouch"))
        {
            if (!Physics.SphereCast(new Vector3(transform.position.x, transform.position.y + m_colliderRadius, transform.position.z), m_colliderRadius, transform.up, out m_groundHit, transform.position.y + m_playerHeight - m_colliderRadius - (m_playerHeight * m_colliderfactor)))
            {
                m_isCrouching = false;
                m_currentMovementSpeed = m_walkSpeed;
                playerState = State.Idle;
            }
        }
    }

    public void Climping()
    {
        if (Input.GetButtonDown("Jump") && !m_isClimbing)
        {
            RaycastHit frontHit = new RaycastHit();
            //Checks if something is in front of the player
            Vector3 rayPosFront = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            if (Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + (m_climbMaxHeight - m_climbMinHeight) * 0.5f, transform.position.z), new Vector3(0.01f, (m_climbMaxHeight - m_climbMinHeight) * 0.5f, 0.01f), transform.forward, out frontHit, transform.rotation, m_climbMaxDistance))
            {
                RaycastHit topHit = new RaycastHit();
                Vector3 offset = (frontHit.point - rayPosFront).normalized * 0.1f;
                Vector3 rayPosTop = new Vector3(frontHit.point.x, rayPosFront.y + m_climbMaxHeight, frontHit.point.z) + offset;

                if (Physics.Raycast(rayPosTop, -transform.up, out topHit, m_climbMaxHeight - m_climbMinHeight))
                {
                    if (!Physics.CapsuleCast(new Vector3(frontHit.point.x, topHit.point.y + m_playerWidth, frontHit.point.z),
                                            new Vector3(frontHit.point.x, topHit.point.y + m_playerHeight - m_playerWidth, frontHit.point.z), m_playerWidth, transform.forward,
                                            Vector3.Distance(new Vector3(frontHit.point.x, topHit.point.y + m_playerWidth, frontHit.point.z), topHit.point)))
                    {

                        m_climbPosition = topHit.point;
                        m_isClimbing = true;
                        m_climbStartPosition = transform.position;
                    }
                }
            }
        }

        if (m_isClimbing)
        {
            m_currentClimbTime += Time.deltaTime * m_timeToClimb;

            m_freezeInputs = true;
            m_cameraController.m_freezeCamera = true;
            transform.position = Vector3.Lerp(m_climbStartPosition, m_climbPosition, m_currentClimbTime);
            if (transform.position == m_climbPosition)
            {
                m_isClimbing = false;
                m_freezeInputs = false;
                m_cameraController.m_freezeCamera = false;
                m_currentClimbTime = 0;
            }
        }
    }

    /// <summary>
    /// Calculates the Vertical speed squared to gravity
    /// </summary>
    /// <returns></returns>
    private float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt(2 * m_jumpHeight * m_gravity);
    }

    /// <summary>
    /// Changes the cameras Y position to the new given height in the time given
    /// </summary>
    /// <param name="_height"></param>
    /// <param name="_time"></param>
    void ChangePlayerHeight(float _height, float _time)
    {
        m_playerCollider.height = _height + (m_colliderfactor * _height);
        m_playerCollider.center = (new Vector3(0, m_playerCollider.height * 0.5f, 0));
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(transform.position.x, _height, transform.position.z), Time.deltaTime * _time);
    }
    #endregion
}
