using UnityEngine;
using UnityEngine.UI;

public class SkyboxChanger : MonoBehaviour
{
    public Material[] skyboxes; // Array of Skybox materials
    public Button leftButton;   // Reference to the left button
    public Button rightButton;  // Reference to the right button

    private int currentSkyboxIndex = 0;

    void Start()
    {
        if (skyboxes.Length == 0)
        {
            Debug.LogError("No skyboxes assigned in the array.");
            return;
        }

        RenderSettings.skybox = skyboxes[currentSkyboxIndex];

        leftButton.onClick.AddListener(ChangeSkyboxLeft);
        rightButton.onClick.AddListener(ChangeSkyboxRight);
    }

    void ChangeSkyboxLeft()
    {
        currentSkyboxIndex--;
        if (currentSkyboxIndex < 0)
        {
            currentSkyboxIndex = skyboxes.Length - 1;
        }

        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
    }

    void ChangeSkyboxRight()
    {
        currentSkyboxIndex++;
        if (currentSkyboxIndex >= skyboxes.Length)
        {
            currentSkyboxIndex = 0;
        }

        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
    }
}
