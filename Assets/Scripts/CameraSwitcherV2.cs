using Rewired;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcherV2 : MonoBehaviour
{
    public static CameraSwitcherV2 instance;
    [Header("Rewired")]
    public int playerId = 0;
    public Player player;

    [Header("Cameras")]
    public GameObject[] cameras;
    public int currentCam;
    public CameraController topDownCam;
    public CinemachineCamera cineCam;

    private void Awake()
    {
        instance = this;
        player = ReInput.players.GetPlayer(playerId);
    }

    void Start()
    {

    }

    void Update()
    {
        if (player.GetButtonDown("Camera Switcher"))
        {
            currentCam++;

            if (currentCam >= cameras.Length)
            {
                currentCam = 0;
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                if (i == currentCam)
                {
                    cameras[i].SetActive(true);
                }
                else
                {
                    cameras[i].SetActive(false);
                }
            }
        }
    }

    public void SeTarget(CarController playerCar)
    {
        if (topDownCam.theCarController != null)
        {
            topDownCam.theCarController = playerCar;
            cineCam.Follow = playerCar.transform;
            cineCam.LookAt = playerCar.transform;
        }
    }

    public void SeTargetV2(CarControllerV2 playerCar)
    {
        if (topDownCam.theCarControllerV2 != null)
        {
            topDownCam.theCarControllerV2 = playerCar;
            cineCam.Follow = playerCar.transform;
            cineCam.LookAt = playerCar.transform;
        }
    }
}