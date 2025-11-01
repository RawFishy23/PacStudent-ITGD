using System.Collections;
using UnityEngine;

public enum BGMState { Intro, Normal, Scared, Dead }

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip introClip;
    public AudioClip normalBGM;
    public AudioClip scaredBGM;
    public AudioClip deadBGM;

    public static AudioPlayer Instance;
    private AudioSource audioSource;
    private BGMState currentState = BGMState.Intro;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        PlayClip(introClip);
        StartCoroutine(CheckIntroFinished());
    }

    private void PlayClip(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    private IEnumerator CheckIntroFinished()
    {
        yield return new WaitForSeconds(introClip.length);
        SwitchState(BGMState.Normal);
    }

    public void SwitchState(BGMState newState)
    {
        if (currentState == newState) return; 

        currentState = newState;
        switch (newState)
        {
            case BGMState.Normal:
                PlayClip(normalBGM, true);
                break;
            case BGMState.Scared:
                PlayClip(scaredBGM, true);
                break;
            case BGMState.Dead:
                PlayClip(deadBGM, true);
                break;
        }
    }
}
