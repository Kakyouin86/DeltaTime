using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Text References")]
    public TMP_Text lapCounterText;
    public TMP_Text currentLapTimeText;
    public TMP_Text bestLapTimeText;
    public TMP_Text positionText;
    public TMP_Text countDownText;
    public TMP_Text goText;
    public TMP_Text raceResultText;

    [Header("UI Game Object References")]
    public GameObject resultsScreen, pauseScreen, trackUnlockedMessage;

    [Header("Game Paused?")]
    public bool isPaused;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }
    public void PauseUnpause()
    {
        isPaused = !isPaused;

        pauseScreen.SetActive(isPaused);

        if(isPaused)
        {
            Time.timeScale = 0f;
        } else
        {
            Time.timeScale = 1f;
        }
    }

    public void ExitRace()
    {
        Time.timeScale = 1f;
        RaceManager.instance.ExitRace();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}