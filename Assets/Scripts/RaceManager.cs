using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;
    public Checkpoint[] allCheckpoints;
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
