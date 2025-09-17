using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip[] audioPlayerOrder;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.clip = audioPlayerOrder[0];                
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = audioPlayerOrder[1];
            audioSource.Play();
            audioSource.loop = true;
        }
    }
}
