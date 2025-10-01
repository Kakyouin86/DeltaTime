using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CarController theCarController;
    public Vector3 offsetDirection;

    void Start()
    {
        theCarController = FindFirstObjectByType<CarController>();

        if (theCarController == null)
        {
            Debug.LogError("No CarController found in the scene.");
            return;
        }

        offsetDirection = transform.position - theCarController.transform.position;

    }

    void Update()
    {
        
    }
}
