using UnityEngine;

public class CheckpointChecker : MonoBehaviour
{
    public CarController theCarController;
    public CarControllerV2 theCarControllerV2;

    void Start()
    {
        theCarController = FindFirstObjectByType<CarController>();
        theCarControllerV2 = FindFirstObjectByType<CarControllerV2>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Checkpoint" && theCarController != null)
        {
            //Debug.Log("Hit checkpoint " + other.GetComponent<Checkpoint>().checkpointNumber); 

            theCarController.CheckpointHit(other.GetComponent<Checkpoint>().checkpointNumber);
        }

        if (other.tag == "Checkpoint" && theCarControllerV2 != null)
        {
            //Debug.Log("Hit checkpoint " + other.GetComponent<Checkpoint>().checkpointNumber);

            theCarControllerV2.CheckpointHit(other.GetComponent<Checkpoint>().checkpointNumber);
        }
    }
}