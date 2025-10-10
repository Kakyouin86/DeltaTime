using UnityEngine;

public class DisableOverTime : MonoBehaviour
{
    public float timeToDisable = 1.5f;

    void Update()
    {
        timeToDisable -= Time.deltaTime;
        if(timeToDisable<= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
