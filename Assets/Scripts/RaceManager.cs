using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;
    public Checkpoint[] allCheckpoints;
    public Checkpoint[] allCheckpointsAI;
    public int totalLaps = 3;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].checkpointNumber = i;
        }

        for (int i = 0; i < allCheckpointsAI.Length; i++)
        {
            allCheckpointsAI[i].checkpointNumberAI = i;
        }
    }

    void Update()
    {

    }

    public void FinishRace()
    {
  
    }

    public void ExitRace()
    {

    }
}
