using UnityEngine;
using Rewired;

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
    public float lowValueDrag = 0.1f;
    public float gravityMod = 10f;

    [Header("Wheels")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public float maxWheelTurn = 25f;

    [Header("Dust Trail")]
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f;
    public float emissionFadeSpeed = 50f;
    public float emissionRate;

    [Header("Checkpoint & Laps")]
    public int nextCheckpoint;
    public int currentLap;
    public float lapTime;
    public float bestLapTime;

    [Header("Audio")]
    public AudioSource engineSFX;
    public AudioSource skidSoundSFX;
    public float skidSFXFadeSpeed;

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

        emissionRate = dustTrail[0].emission.rateOverTime.constant;

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }

    void Update()
    {
        lapTime += Time.deltaTime;

        var ts = System.TimeSpan.FromSeconds(lapTime);
        UIManager.instance.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

        speedInput = 0;
        if (player.GetAxis("Vertical") > 0)
        {
            speedInput = player.GetAxis("Vertical") * forwardAcceleration;
        }

        else if (player.GetAxis("Vertical") < 0)
        {
            speedInput = player.GetAxis("Vertical") * reverseAcceleration;
        }

        /*else
        { 
            theRB.transform.localPosition = Vector3.zero;
            theRB.linearVelocity = Vector3.zero;
        }*/

        turnInput = player.GetAxis("Horizontal");

        if (player.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.linearVelocity.magnitude / maxSpeed), 0f));
        }

        frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, frontLeftWheel.localRotation.eulerAngles.z);
        //frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, (maxWheelTurn * turnInput) - 180, frontLeftWheel.localRotation.eulerAngles.z);

        frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), frontRightWheel.localRotation.eulerAngles.z);
        //frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, (maxWheelTurn * turnInput), frontRightWheel.localRotation.eulerAngles.z);

        //transform.position = theRB.position;

        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.linearVelocity.magnitude < maxSpeed * .5f && theRB.linearVelocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }

        if (theRB.linearVelocity.magnitude  < 0.5f)
        {
            emissionRate = 0f;
        }

        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }

        if (engineSFX != null)
        {
            engineSFX.pitch = 1f + ((theRB.linearVelocity.magnitude / maxSpeed) * 1.5f);
        }

        if (skidSoundSFX != null)
        {
            if (grounded && Mathf.Abs(turnInput) > 0.5f && theRB.linearVelocity.magnitude >= .5f)
            {
                if (!skidSoundSFX.isPlaying)
                {
                    skidSoundSFX.Play();
                }
                skidSoundSFX.volume = 1f;
            }

            else
            {
                skidSoundSFX.volume = Mathf.MoveTowards(
                    skidSoundSFX.volume,
                    0f,
                    skidSFXFadeSpeed * Time.deltaTime
                );

                /*if (skidSoundSFX.volume <= 0.01f)
                {
                    skidSoundSFX.Stop();
                }*/
            }
        }
    }

    public void LateUpdate()
    {
        transform.position = theRB.position;
    }

    public void FixedUpdate()
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            grounded = true;

            normalTarget = hit.normal;
        }

        if (Physics.Raycast(angleRayPoint.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        //when on ground rotate to match the normal
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        //accelerates the car
        if (grounded)
        {
            theRB.linearDamping = dragOnGround;

            theRB.AddForce(transform.forward * speedInput * 1000f);
            //theRB.AddForce(transform.forward * speedInput * 1f);
        }

        else
        {
            theRB.linearDamping = lowValueDrag;

            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }

        if (theRB.linearVelocity.magnitude > maxSpeed)
        {
            theRB.linearVelocity = theRB.linearVelocity.normalized * maxSpeed;
        }

        /*if (grounded && speedInput != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.linearVelocity.magnitude / maxSpeed), 0f));
        }

        transform.position = theRB.position;*/

        //Debug.Log("Magnitude: " + theRB.linearVelocity.magnitude);
        //Debug.Log("Linear Velocity: " + theRB.linearVelocity);
        //Debug.Log("Speed Input: " + speedInput);
    }

    void OnDrawGizmos()
    {
        if (groundRayPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundRayPoint.position, groundRayPoint.position - transform.up * groundRayLength);
        }
    }

    public void CheckpointHit(int cpNumber)
    {
        if (cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            if (nextCheckpoint == RaceManager.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }
    }

    public void LapCompleted()
    {
        currentLap++;

        if (lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }

        lapTime = 0f;

        var ts = System.TimeSpan.FromSeconds(bestLapTime);
        UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);
        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }
}
