using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using Rewired;

public class OsakaStudios : MonoBehaviour
{
    public string nextScene;
    public bool anyButtonSwitchesScene = false;
    public Image fadeScreen;
    public float fadeSpeed = 0.25f;
    public float waitTimeBeforeTransition = 11f;
    //public Player player;

    private void Awake()
    {
        //player = ReInput.players.GetPlayer(0);
    }

    void Start()
    {
        // Start fading in from black at the beginning
        FadeFromBlack();
        StartCoroutine(EndOfTheClip());
    }

    void Update()
    {
        // Handle input for scene transition
        HandleInput();
    }

    private void HandleInput()
    {
        //if (player.GetAnyButtonDown() && anyButtonSwitchesScene)
        if (Input.anyKeyDown && anyButtonSwitchesScene)
        {
            GoToNextScreen();
        }
    }

    public void FadeFromBlack()
    {
        // Start with a full black screen
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 1f);
        StartCoroutine(FadeToClear());
    }

    private IEnumerator FadeToClear()
    {
        // Fade out from black to clear
        while (fadeScreen.color.a > 0f)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, fadeScreen.color.a - fadeSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the screen is fully clear
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0f);
    }

    public void GoToNextScreen()
    {
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator EndOfTheClip()
    {
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        // Start fading to black before transitioning
        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        // Fade from clear to black
        while (fadeScreen.color.a < 1f)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, fadeScreen.color.a + fadeSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the screen is fully black
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 1f);
        GoToNextScreen(); // Load the next scene after fading to black
    }
}