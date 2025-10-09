using UnityEngine;
using Rewired;

public class CarController : MonoBehaviour
{
    [Header("AI")]
    public GameObject theAI;
    public bool isAI = false;
    public int currentTarget;
    public Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f;
    public float aiTurnSpeed = .8f;
    public float aiReachPointRange = 5f;
    public float aiPointVariance = 3f;
    public float aiMaxTurn = 30f;
    public float aiSpeedInput;
    public float aiSpeedMod;
    public float forwardAccel = 8f;
    public float reverseAccel = 4f;
    public float resetCooldown = 2f;
    public float resetCounter;

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
    public int currentLap = 1;
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

        if (isAI)
        {
            targetPoint = RaceManager.instance.allCheckpointsAI[currentTarget].transform.position;
            RandomiseAITarget();

            aiSpeedMod = Random.Range(.8f, 1.1f);
        }

        //UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        UIManager.instance.lapCounterText.text = currentLap.ToString();

        resetCounter = resetCooldown;
    }

    void Update()
    {
        if (player.GetButton("Enable AI") && theAI != null)
        {
            theAI.SetActive(true);
        }

        lapTime += Time.deltaTime;

        if (!isAI)
        {
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

            /*if (player.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.linearVelocity.magnitude / maxSpeed), 0f));
            }*/

            if (resetCounter > 0)
            {
                resetCounter -= Time.deltaTime;
            }

            if (player.GetButton("Reset") && resetCounter <= 0)
            {
                ResetToTrack();
            }
        }

        else
        {
            targetPoint.y = transform.position.y;

            if (Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
            {
                SetNextAITarget();
            }

            Vector3 targetDir = targetPoint - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);

            Vector3 localPos = transform.InverseTransformPoint(targetPoint);

            if (localPos.x < 0f)
            {
                angle = -angle;
            }

            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);

            if (Mathf.Abs(angle) < aiMaxTurn)
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
            }

            else
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
            }

            speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
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

        if (grounded && speedInput != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.linearVelocity.magnitude / maxSpeed), 0f));
        }

        //transform.position = theRB.position;

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

        if (isAI)
        {
            if (cpNumber == currentTarget)
            {
                SetNextAITarget();
            }
        }
    }
    public void SetNextAITarget()
    {
        currentTarget++;
        if (currentTarget >= RaceManager.instance.allCheckpointsAI.Length)
        {
            currentTarget = 0;
        }

        targetPoint = RaceManager.instance.allCheckpointsAI[currentTarget].transform.position;
        RandomiseAITarget();
    }

    public void LapCompleted()
    {
        currentLap++;

        if (lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }

        if (currentLap <= RaceManager.instance.totalLaps)
        {
            lapTime = 0f;

            if (!isAI)
            {
                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                //UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
                UIManager.instance.lapCounterText.text = currentLap.ToString();
            }
        }

        else
        {
            if (!isAI)
            {
                isAI = true;
                aiSpeedMod = 1f;

                targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                RandomiseAITarget();

                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                RaceManager.instance.FinishRace();
            }
        }
    }

    public void RandomiseAITarget()
    {
        targetPoint += new Vector3(Random.Range(-aiPointVariance, aiPointVariance), 0f, Random.Range(-aiPointVariance, aiPointVariance));
    }

    public void ResetToTrack()
    {
        int pointToGoTo = nextCheckpoint - 1;
        if (pointToGoTo < 0)
        {
            pointToGoTo = RaceManager.instance.allCheckpoints.Length - 1;
        }

        transform.position = RaceManager.instance.allCheckpoints[pointToGoTo].transform.position;
        theRB.transform.position = transform.position;
        theRB.linearVelocity = Vector3.zero;

        speedInput = 0f;
        turnInput = 0f;

        resetCounter = resetCooldown;
    }

    public void SwitchToAI()
    {
        aiSpeedMod = 1f;
        targetPoint = RaceManager.instance.allCheckpointsAI[currentTarget].transform.position;
        RandomiseAITarget();
    }
}
