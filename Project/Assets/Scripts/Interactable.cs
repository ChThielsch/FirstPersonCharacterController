using System;
using System.Collections;
using UnityEngine;

//Script by Christian Thielsch 2019
public class Interactable : MonoBehaviour
{
    [Range(0, 200)]
    [Tooltip("Strength of the spring")]
    public float m_spring = 50.0f;

    [Range(0, 50)]
    [Tooltip("Strength of the spring")]
    public float m_damper = 5.0f;

    [Range(0, 10)]
    [Tooltip("Amount that the spring is reduced when active")]
    public float m_drag = 10.0f;

    [Range(0, 10)]
    [Tooltip("The angular Drag of the picked up gameObject")]
    public float m_angularDrag = 5.0f;

    [Range(0, 1)]
    [Tooltip("Upper limit of the distance range over which the spring will not apply any force")]
    public float m_maxDistance = 0.2f;

    [Range(0, 10)]
    [Tooltip("The distance in which objects can be picked up in")]
    public float m_reach = 10;

    [Space]
    [Tooltip("The ammount of force added to the objects Y force to be throwen (set to 1 for none)")]
    [Range(1, 10)]
    public float m_throwForceUpMultiplier = 10;

    [Tooltip("The ammount of force added to the object to be throwen")]
    public float m_throwForce = 20;

    private bool m_attachToCenterOfMass = false;
    private SpringJoint m_SpringJoint;

    private void OnValidate()
    {
        m_throwForce = Mathf.Clamp(m_throwForce, 0, float.MaxValue);
    }

    private void Update()
    {
        if (!Input.GetButtonDown("Fire1"))
        {
            return;
        }
        var mainCamera = FindCamera();

        RaycastHit hit = new RaycastHit();
        if (
            !Physics.Raycast(mainCamera.transform.position,
                             mainCamera.transform.forward, out hit, m_reach,
                             Physics.DefaultRaycastLayers))
        {
            return;
        }

        if (!hit.rigidbody || hit.rigidbody.isKinematic)
        {
            return;
        }

        if (!m_SpringJoint)
        {
            GameObject go = new GameObject("Rigidbody_Dragger");
            Rigidbody body = go.AddComponent<Rigidbody>();
            m_SpringJoint = go.AddComponent<SpringJoint>();
            body.isKinematic = true;

        }

        m_SpringJoint.transform.position = hit.point;
        m_SpringJoint.anchor = Vector3.zero;

        m_SpringJoint.spring = m_spring;
        m_SpringJoint.damper = m_damper;
        m_SpringJoint.maxDistance = m_maxDistance;
        m_SpringJoint.connectedBody = hit.rigidbody;

        StartCoroutine("DragObject", hit.distance);
    }


    private IEnumerator DragObject(float _distance)
    {
        float oldDrag = m_SpringJoint.connectedBody.drag;
        float oldAngularDrag = m_SpringJoint.connectedBody.angularDrag;

        m_SpringJoint.connectedBody.drag = m_drag;
        m_SpringJoint.connectedBody.angularDrag = m_angularDrag;
        Camera mainCamera = FindCamera();
        while (Input.GetButton("Fire1"))
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            m_SpringJoint.transform.position = ray.GetPoint(_distance);


            if (Input.GetButtonDown("Fire2"))
            {
                m_SpringJoint.connectedBody.drag = oldDrag;
                m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
                m_SpringJoint.connectedBody.AddForce(new Vector3(
                    mainCamera.transform.forward.x,
                    mainCamera.transform.forward.y * m_throwForceUpMultiplier,
                    mainCamera.transform.forward.z) * m_throwForce, ForceMode.Impulse);

                m_SpringJoint.connectedBody = null;
            }
            yield return null;
        }
        if (m_SpringJoint.connectedBody)
        {
            m_SpringJoint.connectedBody.drag = oldDrag;
            m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
            m_SpringJoint.connectedBody = null;
        }
    }

    /// <summary>
    /// Finds the Scenes main camera and returns it
    /// </summary>
    /// <returns>Camera.main</returns>
    private Camera FindCamera()
    {
        if (GetComponent<Camera>())
        {
            return GetComponent<Camera>();
        }

        return Camera.main;
    }
}
