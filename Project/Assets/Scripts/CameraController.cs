using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script by Christian Thielsch 2019
public class CameraController : MonoBehaviour
{
    #region public variables
    public bool m_freezeCamera = false;


    public bool m_invertMouseX;
    public bool m_invertMouseY;

    public float m_sensitivityX = 15f;
    public float m_sensitivityY = 15f;
    public float m_mouseDelay = 10f;

    public float m_minimumX = -180f;
    public float m_maximumX = 180f;

    public bool m_clampY = false;

    public float m_minimumY = -60f;
    public float m_maximumY = 60f;


    [Space]
    public bool m_headBob;
    public float m_headBobRage = 2.5f;
    public float m_headBobSpeed = 5;
    public float m_headBobTransitionSpeed = 20f;

    #endregion

    #region private variables
    /// <summary>
    /// the value added to the mouseX input to invert it
    /// </summary>
    private int m_invertX = 1;

    /// <summary>
    /// the value added to the mouseY input to invert it
    /// </summary>
    private int m_invertY = 1;

    /// <summary>
    /// the current X value of the cameras rotation
    /// </summary>
    private float m_rotationX = 0f;

    /// <summary>
    /// the current Y value of the cameras rotation
    /// </summary>
    private float m_rotationY = 0f;

    /// <summary>
    /// the average X value of the cameras rotation
    /// </summary>
    private float m_rotAverageX = 0f;

    /// <summary>
    /// the average Y value of the cameras rotation
    /// </summary>
    private float m_rotAverageY = 0f;

    /// <summary>
    /// a list of all X rotation inputs 
    /// </summary>
    private List<float> m_rotArrayX = new List<float>();

    /// <summary>
    /// a list of all Y rotation inputs 
    /// </summary>
    private List<float> m_rotArrayY = new List<float>();

    /// <summary>
    /// the playerController script on the player
    /// </summary>
    private PlayerController m_playerController;

    /// <summary>
    /// the timer needed for head bob
    /// </summary>
    private float m_headBobTimer = Mathf.PI / 2;

    /// <summary>
    /// the default position of the camera
    /// </summary>
    private Vector3 m_restPosition;
    #endregion

    private void OnValidate()
    {
        m_headBobSpeed = Mathf.Clamp(m_headBobSpeed, 0, float.MaxValue);
        m_headBobTransitionSpeed = Mathf.Clamp(m_headBobTransitionSpeed, 0, float.MaxValue);
    }

    void Start()
    {
        m_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (m_invertMouseX)
        {
            m_invertX = 1;
        }
        else
        {
            m_invertX = -1;
        }

        if (m_invertMouseY)
        {
            m_invertY = -1;
        }
        else
        {
            m_invertY = 1;
        }


        if (!m_freezeCamera)
        {
            // Resets the average rotation
            m_rotAverageY = 0f;
            m_rotAverageX = 0f;

            // Gets rotational input from the mouse
            m_rotationY = Input.GetAxis("Mouse X") * m_sensitivityY * Time.deltaTime * 10 * m_invertY;
            m_rotationX = Input.GetAxis("Mouse Y") * m_sensitivityX * Time.deltaTime * 10 * m_invertX;

            // Adds the rotation values to their relative array
            m_rotArrayY.Add(m_rotationY);
            m_rotArrayX.Add(m_rotationX);

            // If the arrays length is bigger or equal to the value of frameCounter remove the first value in the array
            if (m_rotArrayY.Count >= m_mouseDelay)
            {
                m_rotArrayY.RemoveAt(0);
            }
            if (m_rotArrayX.Count >= m_mouseDelay)
            {
                m_rotArrayX.RemoveAt(0);
            }

            // Adding up all the rotational input values from each array
            for (int j = 0; j < m_rotArrayY.Count; j++)
            {
                m_rotAverageY += m_rotArrayY[j];
            }
            for (int j = 0; j < m_rotArrayX.Count; j++)
            {
                m_rotAverageX += m_rotArrayX[j];
            }

            // Standard maths to find the average
            m_rotAverageY /= m_rotArrayY.Count;
            m_rotAverageX /= m_rotArrayX.Count;

            // Setting rotations to current rotations
            Vector3 cameraRotation = transform.eulerAngles;
            Vector3 playerRotation = m_playerController.transform.eulerAngles;

            // Calcuation for unitys weird rotations
            cameraRotation.x = (cameraRotation.x + 180) % 360;
            playerRotation.y = (playerRotation.y + 180) % 360;

            // Adding rotationn average
            playerRotation.y += m_rotAverageY;
            cameraRotation.x += m_rotAverageX;

            // Clamping values within given range
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, 180 + m_minimumX, 180 + m_maximumX);
            if (m_clampY)
            {
                playerRotation.y = Mathf.Clamp(playerRotation.y, 180 + m_minimumY, 180 + m_maximumY);
            }

            // Calcuation for unitys weird rotations
            cameraRotation.x -= 180;
            playerRotation.y -= 180;

            // Setting rotation to new calculated rotation
            transform.eulerAngles = cameraRotation;
            m_playerController.transform.eulerAngles = playerRotation;
            m_playerController.transform.forward = new Vector3(m_playerController.transform.forward.x, 0, m_playerController.transform.forward.z);

            if (m_headBob && m_playerController.grounded())
            {
                m_restPosition = new Vector3(transform.parent.position.x, transform.parent.position.y + m_playerController.m_currentPlayerHeight, transform.parent.position.z);

                if(m_playerController.m_rigidbody.velocity.magnitude > 0.01f) //true when moving
                {
                    m_headBobTimer += (m_headBobSpeed + m_playerController.m_currentMovementSpeed) * 0.5f * Time.deltaTime;

                    //use the timer value to set the position
                    Vector3 newPosition = new Vector3(m_restPosition.x, m_restPosition.y
                                        + Mathf.Abs((Mathf.Sin(m_headBobTimer) * m_headBobRage)), m_restPosition.z); //abs val of y for range between 0 and 1

                    transform.position = newPosition;
                }
                else
                {
                    m_headBobTimer = Mathf.PI / 2; //reinitialize

                    //transition smoothly from walking to stopping
                    Vector3 newPosition = new Vector3(Mathf.Lerp(transform.position.x, m_restPosition.x, m_headBobTransitionSpeed * Time.deltaTime),
                                            Mathf.Lerp(transform.position.y, m_restPosition.y, m_headBobTransitionSpeed * Time.deltaTime),
                                            Mathf.Lerp(transform.position.z, m_restPosition.z, m_headBobTransitionSpeed * Time.deltaTime));

                    transform.position = newPosition;
                }

                //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values
                if (m_headBobTimer > Mathf.PI * 2)
                {
                    m_headBobTimer = 0;
                }
            }
        }
    }
}
