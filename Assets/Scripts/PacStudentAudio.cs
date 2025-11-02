using UnityEngine;

public class PacStudentAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource walkSource;
    public AudioSource wallHitSource;
    public AudioSource pelletSource;

    [Header("Clips")]
    public AudioClip walkClip;    
    public AudioClip wallHitClip;   
    public AudioClip pelletClip;    

    public void HandleWalking(bool walking)
    {
        if (walking)
        {
            if (!walkSource.isPlaying)
            {
                walkSource.clip = walkClip;
                walkSource.loop = true;
                walkSource.Play();
            }
        }
        else
        {
            walkSource.Stop();
        }
    }

    public void PlayWallHit()
    {
        if (!wallHitSource.isPlaying)
            wallHitSource.PlayOneShot(wallHitClip);
    }

    public void PlayPelletEat()
    {
        pelletSource.PlayOneShot(pelletClip);
    }
}
