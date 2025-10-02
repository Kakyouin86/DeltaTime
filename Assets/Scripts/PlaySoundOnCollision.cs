using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    public AudioSource soundToPlay;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
        {
            soundToPlay.Stop();
            soundToPlay.pitch = Random.Range(minPitch, maxPitch);
            soundToPlay.Play();
        }
    }
}