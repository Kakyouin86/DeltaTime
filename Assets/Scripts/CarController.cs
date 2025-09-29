using UnityEngine;
using Rewired;
using Unity.VisualScripting;

public class CarController : MonoBehaviour
{
    [Header("Speed & Torque")]
    public float maxSpeed = 30f;
    public float speedInput;
    public float forwardAcceleration = 8f;
    public float reverseAcceleration = 4f;
    public float turnInput;
    public float turnStrength = 180f;

    [Header("Ground Check")]
    public bool grounded;
    public LayerMask groundLayer;
    public Transform groundRayPoint;
    public Transform angleRayPoint;
    public float groundRayLength = 0.75f;

    [Header("Air Control")]
    public float dragOnGround;
    public float gravityMod = 10f;

    [Header("Wheels")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public float maxWheelTurn = 25f;

    [Header("Components")]
    public Rigidbody theRB;
    public Player player;

    void Start()
    {
        theRB = GetComponentInChildren<Rigidbody>();
        if (theRB == null)
        {
            theRB.transform.parent = null;
            dragOnGround = theRB.linearDamping;
        }
        player = ReInput.players.GetPlayer(0);
    }


    void Update()
    {
        speedInput = 0;
        if (player.GetAxis("Vertical") > 0)
        {
            speedInput = player.GetAxis("Vertical") * forwardAcceleration;
        }

        else if (player.GetAxis("Vertical") < 0)
        {
            speedInput = player.GetAxis("Vertical") * reverseAcceleration;
        }

        turnInput = player.GetAxis("Horizontal");

        if (player.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.linearVelocity.magnitude / maxSpeed), 0));
        }

        frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, (maxWheelTurn * turnInput) - 180, frontLeftWheel.localRotation.eulerAngles.z);
        frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, (maxWheelTurn * turnInput), frontRightWheel.localRotation.eulerAngles.z);
        transform.position = theRB.transform.position;
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        grounded = Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, groundLayer);
        Vector3 normalTarget = Vector3.zero;

        if (Physics.Raycast(angleRayPoint.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal) / 2;
        }

        if (grounded)
        {
            normalTarget = hit.normal;
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
            if (theRB != null)
            {
                theRB.linearDamping = dragOnGround;
                theRB.AddForce(transform.forward * speedInput * 1000);
                //Debug.Log(theRB.linearVelocity.magnitude);
            }
        }
        
        else
        {
            if (theRB != null)
            {
                theRB.linearDamping = 0.01f;
                theRB.AddForce(-Vector3.up * gravityMod * 1000f);
            }
        }

        if (theRB != null && theRB.linearVelocity.magnitude > maxSpeed)
        {
            theRB.linearVelocity = theRB.linearVelocity.normalized * maxSpeed;
        }
    }

    void OnDrawGizmos()
    {
        if (groundRayPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundRayPoint.position, groundRayPoint.position - transform.up * groundRayLength);
        }
    }
}
