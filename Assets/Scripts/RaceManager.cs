using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("Checkpoint References")]
    public static RaceManager instance;
    public Checkpoint[] allCheckpoints;
    public Checkpoint[] allCheckpointsAI;
    public int totalLaps = 3;

    [Header("Position References")]
    public CarController playerCar;
    public CarControllerV2 playerCarV2;
    public List<CarController> allAICars = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPosCheck = 0.2f;
    public float posCheckCounter;

    [Header("Rubberband References")]
    public float aiDefaultSpeed = 30f;
    public float playerDefaultSpeed = 30f;
    public float rubberBandSpeedMod = 3.5f;
    public float rubBandAccel = 0.5f;

    [Header("Starting References")]
    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    public float startCounter;
    public int countdownCurrent = 3;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        playerCarV2 = FindFirstObjectByType<CarControllerV2>();

        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].checkpointNumber = i;
        }

        for (int i = 0; i < allCheckpointsAI.Length; i++)
        {
            allCheckpointsAI[i].checkpointNumberAI = i;
        }

        isStarting = true;
        startCounter = timeBetweenStartCount;
        UIManager.instance.countDownText.text = countdownCurrent + "";
    }

    void Update()
    {
        if (isStarting)
        {
            startCounter -= Time.deltaTime;
            if (startCounter <= 0)
            {
                countdownCurrent--;
                startCounter = timeBetweenStartCount;

                UIManager.instance.countDownText.text = countdownCurrent + "";

                if (countdownCurrent == 0)
                {
                    isStarting = false;

                    UIManager.instance.countDownText.gameObject.SetActive(false);
                    UIManager.instance.goText.gameObject.SetActive(true);
                }
            }
        }

        else
        {
            posCheckCounter -= Time.deltaTime;

            if (posCheckCounter <= 0)
            {
                playerPosition = 1;

                foreach (CarController aiCar in allAICars)
                {
                    if (aiCar.currentLap > (playerCar.currentLap))
                    {
                        playerPosition++;
                    }

                    else if (aiCar.currentLap == playerCar.currentLap)
                    {
                        if (aiCar.nextCheckpoint > playerCar.nextCheckpoint)
                        {
                            playerPosition++;
                        }

                        else if (aiCar.nextCheckpoint == playerCar.nextCheckpoint)
                        {
                            if (Vector3.Distance(aiCar.transform.position, allCheckpoints[aiCar.nextCheckpoint].transform.position) < Vector3.Distance(playerCar.transform.position, allCheckpoints[aiCar.nextCheckpoint].transform.position))
                            {
                                playerPosition++;
                            }
                        }
                    }
                }

                posCheckCounter = timeBetweenPosCheck;

                UIManager.instance.positionText.text = playerPosition + "/" + (allAICars.Count + 1);
            }

            //manage rubber banding
            if (playerPosition == 1)
            {
                foreach (CarController aiCar in allAICars)
                {
                    aiCar.maxSpeed = Mathf.MoveTowards(aiCar.maxSpeed, aiDefaultSpeed + rubberBandSpeedMod, rubBandAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed - rubberBandSpeedMod, rubBandAccel * Time.deltaTime);
            }

            else
            {
                foreach (CarController aiCar in allAICars)
                {
                    aiCar.maxSpeed = Mathf.MoveTowards(aiCar.maxSpeed, aiDefaultSpeed - (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))), rubBandAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed + (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))), rubBandAccel * Time.deltaTime);
            }
        }
    }
 
    public void FinishRace()
    {
  
    }

    public void ExitRace()
    {

    }
}
