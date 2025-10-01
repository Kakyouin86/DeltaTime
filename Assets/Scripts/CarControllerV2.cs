using Rewired;
using UnityEngine;
using static CarControllerV2;

public class CarControllerV2 : MonoBehaviour
{
    /// <summary>
    /// Determines how the car applies steering input.
    /// </summary>
    public enum SteeringMode
    {
        /// <summary>
        /// Standard: car only turns when accelerating forward.
        /// </summary>
        Simple,

        /// <summary>
        /// Allows turning even when coasting or moving backward.
        /// </summary>
        WithInertia,

        /// <summary>
        /// Turn strength scales proportionally with current speed.
        /// </summary>
        SpeedScaled
    }

    /// <summary>
    /// Determines lateral friction / side-sliding behavior of the car.
    /// </summary>
    public enum LateralFrictionMode
    {
        /// <summary>No extra lateral friction applied (slippery).</summary>
        None,

        /// <summary>Limit lateral sliding to 50% of default (semi-grippy).</summary>
        Clamp50Percent,

        /// <summary>Limit lateral sliding to 25% of default (very grippy).</summary>
        Clamp25Percent
    }

    /// <summary>
    /// Determines which braking method to use.
    /// </summary>
    public enum BrakeMode
    {
        /// <summary>Normal brake using the existing brakeForce.</summary>
        Normal,

        /// <summary>Soft brake: applies a reduced factor (like 0.9) to slow down more gently.</summary>
        Soft
    }

    /// <summary>
    /// Determines which wheels apply force.
    /// </summary>
    public enum DriveType
    {
        /// <summary>Force applied at the center (neutral behavior).</summary>
        Center,

        /// <summary>Front-wheel drive: car pushes from front, rear rotates more smoothly.</summary>
        FrontWheel,

        /// <summary>Rear-wheel drive: car tends to lift front slightly and rear can slide more easily.</summary>
        RearWheel
    }

    /// <summary>
    /// Determines if we want to use Hover Effect or not.
    /// </summary>
    public enum HoverEffectMode
    {
        /// <summary>Hover Off.</summary>
        Off,
        /// <summary>>Hover On.</summary>
        On
    }

    /// <summary>
    /// Debug mode toggle.
    /// </summary>
    public enum DebugMode
    {
        /// <summary>Debugging disabled.</summary>
        Off,

        /// <summary>Debugging enabled.</summary>
        On
    }


    [Header("Debug Settings")]
    public DebugMode debugMode = DebugMode.Off;

    [Header("Rewired")]
    public int playerId = 0;
    public Player player;

    [Header("Speed & Torque")]
    public float acceleration = 5000f;
    public float maxSpeed = 50f;
    public float turnSpeed = 100f;
    public float brakeForce = 3000f;
    public float currentSpeed;
    public float inertiaFactor = 10f;
    private Vector3 lastPosition;
    private float lastMovedDistance;

    [Header("Brake Settings")]
    public BrakeMode brakeMode = BrakeMode.Normal;
    public float softBrakeFactor = 0.9f;

    [Header("Steering / Friction Options")]
    public SteeringMode steeringMode = SteeringMode.WithInertia;
    public LateralFrictionMode lateralFrictionMode = LateralFrictionMode.Clamp50Percent;

    [Header("Ground Check")]
    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundRayPoint;
    public float groundCheckDistance = 1.5f;

    [Header("Air Control")]
    public float airGravity = 20f; // Extra downward force when airborne
    public float airDrag = 0.1f; // Reduce drag in air for smoother falling

    [Header("Mesh / Visual Rotation")]
    public Transform angleRayPoint; // Point to cast ray from
    public Transform meshTransform; // Child mesh to rotate
    public float meshRayLength = 2f; // How far down to check terrain
    public float meshRotationSpeed = 5f; // Smooth interpolation

    [Header("Step-Up / Ramp Grace")]
    public float maxStepHeight = 0.7f; // Max step car can climb
    public float stepOriginHeight = 0.1f; // How much above base to cast ray from
    public float forwardCheckDistance = 0.5f; // Look-ahead distance

    [Header("Wheels")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public float maxWheelTurn = 25f; // Maximum wheel rotation in degrees

    [Header("Dust Trail")]
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f;
    public float emissionFadeSpeed = 50f;
    public float emissionRate;

    [Header("Drive Type")]
    public DriveType driveType = DriveType.Center;
    public Transform frontForcePoint;
    public Transform rearForcePoint;

    [Header("Hover Effect (Visual Only)")]
    public HoverEffectMode hoverEffect = HoverEffectMode.Off;
    public float hoverAmplitude = 0.25f;
    public float hoverFrequency = 1f;
    public float tiltAmount = 0.35f;

    [Header("Components")]
    public Rigidbody rb;
    public float originalMass;
    public float originalLinearDamping;
    public float originalAngularDamping;
    public Vector3 meshBaseLocalPos;

    void Awake()
    {
        player = ReInput.players.GetPlayer(playerId);
        rb = GetComponent<Rigidbody>();
        originalMass = rb.mass;
        originalLinearDamping = rb.linearDamping;
        originalAngularDamping = rb.angularDamping;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (meshTransform != null)
            meshBaseLocalPos = meshTransform.localPosition;
    }

    void FixedUpdate()
    {
        currentSpeed = rb.linearVelocity.magnitude;

        GroundCheck();

        if (isGrounded)
        {
            rb.linearDamping = originalLinearDamping;
        }
        else
        {
            // --- Air control ---
            rb.linearDamping = airDrag;
            rb.AddForce(Vector3.down * airGravity * rb.mass, ForceMode.Force);
        }

        if (!isGrounded) return;

        float moveInput = player.GetAxis("Vertical"); // W/S
        float turnInputLocal = player.GetAxis("Horizontal"); // A/D
        bool brakeInput = player.GetButton("Brake");

        // --- Calculate forward force ---
        Vector3 forwardForce = transform.forward * moveInput * acceleration * Time.fixedDeltaTime;

        // --- Torque Debug ---
        Vector3 forcePoint = rb.position; // default center
        if (driveType == DriveType.FrontWheel && frontForcePoint != null) forcePoint = frontForcePoint.position;
        if (driveType == DriveType.RearWheel && rearForcePoint != null) forcePoint = rearForcePoint.position;

        Vector3 torque = Vector3.Cross(forcePoint - rb.worldCenterOfMass, forwardForce);

        if (debugMode == DebugMode.On)
        {
            Debug.DrawRay(forcePoint, transform.forward * 2f, Color.red);
            Debug.Log("DriveType: " + driveType + " | Torque magnitude: " + torque.magnitude);
        }

        // --- Apply force based on DriveType ---
        switch (driveType)
        {
            case DriveType.Center:
                rb.AddForce(forwardForce);
                break;
            case DriveType.FrontWheel:
                if (frontForcePoint != null)
                    rb.AddForceAtPosition(forwardForce, frontForcePoint.position);
                else
                    rb.AddForce(forwardForce); // fallback
                break;
            case DriveType.RearWheel:
                if (rearForcePoint != null)
                    rb.AddForceAtPosition(forwardForce, rearForcePoint.position);
                else
                    rb.AddForce(forwardForce); // fallback
                break;
        }

        // --- Regular Brake ---
        if (brakeInput && rb.linearVelocity.sqrMagnitude > 0.01f) // Solo si estamos moviéndonos
        {
            float factor = brakeMode == BrakeMode.Normal ? 1f : softBrakeFactor;
            Vector3 brake = -rb.linearVelocity.normalized * brakeForce * factor * Time.fixedDeltaTime;
            rb.AddForce(brake);
        }

        // --- Handbrake (rear wheels only) ---
        bool handbrakeInput = player.GetButton("Handbrake");
        if (handbrakeInput && rearForcePoint != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 rearVel = rb.GetPointVelocity(rearForcePoint.position);
            if (rearVel.sqrMagnitude > 0.01f)
            {
                Vector3 rearBrakeForce = -rearVel.normalized * brakeForce * 1.2f * Time.fixedDeltaTime;
                rb.AddForceAtPosition(rearBrakeForce, rearForcePoint.position);
            }
        }

        // --- Step-Up / Ramp Grace ---
        Vector3 stepOrigin = transform.position + Vector3.up * stepOriginHeight;
        RaycastHit stepHit;
        Vector3 forwardOffset = transform.forward * forwardCheckDistance;

        if (Physics.Raycast(stepOrigin + forwardOffset, -transform.up, out stepHit, maxStepHeight + 0.1f, groundLayer))
        {
            float stepOffset = maxStepHeight - stepHit.distance;
            if (stepOffset > 0f)
            {
                rb.position += Vector3.up * stepOffset;
            }
        }

        // --- Limit speed ---
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // --- Steering ---
        float forwardVel = Vector3.Dot(rb.linearVelocity, transform.forward);
        float directionSign = Mathf.Sign(forwardVel);
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / inertiaFactor);

        switch (steeringMode)
        {
            case SteeringMode.Simple:
                if (rb.linearVelocity.magnitude > 1f && Mathf.Abs(moveInput) > 0f)
                {
                    float turn = turnInputLocal * turnSpeed * Time.fixedDeltaTime;
                    rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
                }
                break;

            case SteeringMode.WithInertia:
                if (rb.linearVelocity.sqrMagnitude > 0.01f)
                {
                    float turn = turnInputLocal * turnSpeed * Time.fixedDeltaTime * speedFactor * directionSign;
                    rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
                }
                break;

            case SteeringMode.SpeedScaled:
                if (rb.linearVelocity.sqrMagnitude > 0.01f)
                {
                    float turn = turnInputLocal * maxSpeed * Time.fixedDeltaTime * speedFactor * directionSign;
                    rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
                }
                break;
        }

        // --- Lateral friction ---
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        switch (lateralFrictionMode)
        {
            case LateralFrictionMode.Clamp50Percent:
                localVel.x *= 0.5f;
                break;
            case LateralFrictionMode.Clamp25Percent:
                localVel.x *= 0.25f;
                break;
            case LateralFrictionMode.None:
                break;
        }
        rb.linearVelocity = transform.TransformDirection(localVel);
    }

    void LateUpdate()
    {
        if (meshTransform == null || angleRayPoint == null) return;

        RaycastHit hit;
        if (Physics.Raycast(angleRayPoint.position, -transform.up, out hit, meshRayLength, groundLayer))
        {
            // Get the terrain normal
            Vector3 normal = hit.normal;

            // Compute target rotation: align mesh 'up' with normal, keep forward
            Quaternion targetRot = Quaternion.FromToRotation(meshTransform.up, normal) * meshTransform.rotation;

            // Smoothly interpolate to target rotation
            meshTransform.rotation = Quaternion.Slerp(meshTransform.rotation, targetRot, meshRotationSpeed * Time.deltaTime);
        }

        if (hoverEffect == HoverEffectMode.On && meshTransform != null)
        {
            float time = Time.time * hoverFrequency;
            float yOffset = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * hoverAmplitude;
            float pitch = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * tiltAmount;
            float roll = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * tiltAmount;
            meshTransform.localPosition = meshBaseLocalPos + Vector3.up * yOffset;
            Quaternion tiltRot = Quaternion.Euler(pitch, 0f, roll);
            meshTransform.localRotation = meshTransform.localRotation * tiltRot;
        }
    }

    void Update()
    {
        // --- Calculate movement from position ---
        Vector3 deltaPos = transform.position - lastPosition;
        float movedDistance = deltaPos.magnitude;
        bool isMoving = movedDistance > 0.01f;
        lastPosition = transform.position;

        float turnInput = player.GetAxis("Horizontal");
        bool slowingDown = isMoving && movedDistance < lastMovedDistance;
        lastMovedDistance = movedDistance;

        if (debugMode == DebugMode.On)
        {
            Debug.Log($"Grounded: {isGrounded} | TurnInput: {turnInput:F2} | Moved: {movedDistance:F3} | SlowingDown: {slowingDown}");
        }

        // --- DustTrail logic ---
        if (isGrounded && (Mathf.Abs(turnInput) > 0.5f || slowingDown) && isMoving)
            emissionRate = maxEmission;
        else
            emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }

        // --- Wheels rotation ---
        if (frontLeftWheel != null)
            frontLeftWheel.localRotation = Quaternion.Euler(
                frontLeftWheel.localRotation.eulerAngles.x,
                (turnInput * maxWheelTurn) - 180,
                frontLeftWheel.localRotation.eulerAngles.z
            );
        if (frontRightWheel != null)
            frontRightWheel.localRotation = Quaternion.Euler(
                frontRightWheel.localRotation.eulerAngles.x,
                turnInput * maxWheelTurn,
                frontRightWheel.localRotation.eulerAngles.z
            );
    }

    void GroundCheck()
    {
        isGrounded = false;
        RaycastHit hit;

        if (groundRayPoint != null && Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundCheckDistance, groundLayer))
            isGrounded = true;

        if (angleRayPoint != null && Physics.Raycast(angleRayPoint.position, -transform.up, out hit, groundCheckDistance, groundLayer))
            isGrounded = true;
    }

    void OnDrawGizmos()
    {
        // --- Ground check rays ---
        if (groundRayPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundRayPoint.position, groundRayPoint.position - transform.up * groundCheckDistance);
        }
        if (angleRayPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(angleRayPoint.position, angleRayPoint.position - transform.up * groundCheckDistance);
        }

        // --- Force points ---
        if (frontForcePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(frontForcePoint.position, 0.1f);
        }
        if (rearForcePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rearForcePoint.position, 0.1f);
        }

        // --- Center force ---
        if (rb != null && driveType == DriveType.Center)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rb.position, rb.position + transform.forward * 2f);
        }

        // --- Step-Up Gizmo ---
        Gizmos.color = Color.magenta;
        Vector3 stepOrigin = transform.position + Vector3.up * stepOriginHeight;
        Vector3 forwardOffset = transform.forward * forwardCheckDistance;
        Gizmos.DrawLine(stepOrigin + forwardOffset, stepOrigin + forwardOffset - transform.up * maxStepHeight);
    }
}