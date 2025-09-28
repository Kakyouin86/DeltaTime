using UnityEngine;
using Rewired;

public class HoverCar : MonoBehaviour
{
    [Header("Hover Settings")]
    public Transform[] hoverPoints;
    public float hoverHeight = 2f;
    public float hoverForce = 500f;
    public float damping = 0.5f;
    public LayerMask groundLayer;

    public enum HoverMode
    {
        Constant,
        RandomRange
    }

    public HoverMode hoverMode = HoverMode.Constant;
    public float minHoverForce = 400f;
    public float maxHoverForce = 600f;
    public float hoverForceVariationTime = 2f;
    private float hoverForceTimer;

    [Header("Movement Settings")]
    public float forwardForce = 1000f;
    public float maxForwardSpeed = 50f;
    public float turnTorque = 3000f;
    public float maxTurnSpeed = 1000f;
    public float brakeStrength = 25f;
    public float stopThreshold = 5f;

    [Header("Hover Noise")]
    public float noiseAmplitude = 0.2f;
    public float noiseFrequency = 1f;
    public bool noiseX = false;
    public bool noiseY = false;
    public bool noiseZ = false;

    [Header("Rotation Clamps")]
    public bool clampX = true, clampY = true, clampZ = true;
    public float minX = -5f;
    public float maxX = 5f;
    public float minY = -25f;
    public float maxY = 25f;
    public float minZ = -5f;
    public float maxZ = 5f;

    public enum AccelerateBrakeMode
    {
        PhysicsCombine,   // Ambas fuerzas se suman normalmente
        BrakePriority     // El freno tiene prioridad, acel no aplica
    }

    [Header("Movement Mode")]
    public AccelerateBrakeMode accelerateBrakeMode = AccelerateBrakeMode.BrakePriority;

    [Header("Debug")]
    public bool isGrounded;
    public bool wasGrounded = false;
    public Rigidbody rb;
    public Player player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(0);
    }

    void FixedUpdate()
    {
        Debug.Log($"🏎️ Velocidad actual: {rb.linearVelocity.magnitude:F2} m/s");

        // Hover y grounded
        isGrounded = HandleHover();
        ApplyHoverNoise();

        // Debug solo cuando cambia
        if (isGrounded != wasGrounded)
        {
            wasGrounded = isGrounded;
            Debug.Log(isGrounded ? "🟢 La nave está en el suelo!" : "🔴 La nave está en el aire!");
        }

        // Movimiento
        HandleMovement();

        // Clamp de rotación del parent
        ClampRotation();
    }

    bool HandleHover()
    {
        bool grounded = false;

        foreach (Transform point in hoverPoints)
        {
            RaycastHit hit;
            Vector3 down = -transform.up;

            Debug.DrawRay(point.position, down * hoverHeight, Color.red);

            if (Physics.Raycast(point.position, down, out hit, hoverHeight, groundLayer))
            {
                grounded = true;

                // Hover mode
                float appliedHoverForce = hoverForce;
                if (hoverMode == HoverMode.RandomRange)
                {
                    hoverForceTimer += Time.fixedDeltaTime;
                    if (hoverForceTimer >= hoverForceVariationTime)
                    {
                        appliedHoverForce = Random.Range(minHoverForce, maxHoverForce);
                        hoverForceTimer = 0f;
                    }
                }

                float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
                Vector3 appliedForce = transform.up * appliedHoverForce * proportionalHeight;

                appliedForce -= rb.linearVelocity * damping;

                rb.AddForceAtPosition(appliedForce, point.position);
            }
        }

        return grounded;
    }

    void ApplyHoverNoise()
    {
        Vector3 noise = Vector3.zero;
        float n = (Mathf.PerlinNoise(Time.time * noiseFrequency, 0f) - 0.5f) * 2f * noiseAmplitude;

        if (noiseX) noise.x = n;
        if (noiseY) noise.y = n;
        if (noiseZ) noise.z = n;

        rb.AddForce(noise, ForceMode.Acceleration);
    }

    void HandleMovement()
    {
        bool accelerate = player.GetButton("Accelerate");
        bool brake = player.GetButton("Break");
        float turnInput = player.GetAxis("Horizontal");

        Debug.Log($"🚀 Accelerate: {(accelerate ? "ON" : "OFF")} | 🛑 Break: {(brake ? "ON" : "OFF")} | ↪ TurnInput: {turnInput}");

        switch (accelerateBrakeMode)
        {
            case AccelerateBrakeMode.PhysicsCombine:
                if (accelerate)
                    ApplyAcceleration();
                if (brake)
                    ApplyBrake();
                break;

            case AccelerateBrakeMode.BrakePriority:
                if (brake)
                    ApplyBrake();
                else if (accelerate)
                    ApplyAcceleration();
                break;
        }

        // Giro siempre
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float appliedTorque = Mathf.Clamp(turnInput * turnTorque * Time.fixedDeltaTime, -maxTurnSpeed, maxTurnSpeed);
            rb.AddTorque(transform.up * appliedTorque);
        }
    }

    void ApplyAcceleration()
    {
        Vector3 vel = rb.linearVelocity;
        vel += transform.forward * forwardForce * Time.fixedDeltaTime;
        vel = Vector3.ClampMagnitude(vel, maxForwardSpeed);
        rb.linearVelocity = vel;
    }

    void ApplyBrake()
    {
        Vector3 brakeForce = -rb.linearVelocity * brakeStrength * Time.fixedDeltaTime;
        rb.AddForce(brakeForce, ForceMode.Acceleration);

        // Detener completamente si está muy lento
        if (rb.linearVelocity.magnitude < stopThreshold)
            rb.linearVelocity = Vector3.zero;
    }

    void ClampRotation()
    {
        Vector3 euler = transform.eulerAngles;

        if (clampX)
            euler.x = Mathf.Clamp(NormalizeAngle(euler.x), minX, maxX);
        if (clampY)
            euler.y = Mathf.Clamp(NormalizeAngle(euler.y), minY, maxY);
        if (clampZ)
            euler.z = Mathf.Clamp(NormalizeAngle(euler.z), minZ, maxZ);

        transform.eulerAngles = euler;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
