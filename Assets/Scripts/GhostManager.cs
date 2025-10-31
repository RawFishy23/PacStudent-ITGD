using UnityEngine;
using System.Collections.Generic;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance;

    public List<Animator> ghostAnimators;
    public AudioSource backgroundMusic;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip deadMusic;

    private float scaredTimer = 0f;
    private bool isScared = false;

    void Awake() => Instance = this;

    void Update()
    {
        if (isScared)
        {
            float remaining = GameManager.Instance.GetRemainingGhostTime();

            if (remaining <= 3f && !ghostAnimators[0].GetBool("Recovering"))
            {
                foreach (var g in ghostAnimators)
                    g.SetBool("Recovering", true);
            }

            if (remaining <= 0f)
            {
                foreach (var g in ghostAnimators)
                {
                    if (!g.GetBool("Dead"))
                    {
                        g.SetBool("Scared", false);
                        g.SetBool("Recovering", false);
                    }
                }

                backgroundMusic.clip = normalMusic;
                backgroundMusic.Play();
                isScared = false;
            }
        }
    }

    public void EnterScaredMode()
    {
        foreach (var g in ghostAnimators)
        {
            if (!g.GetBool("Dead"))
            {
                g.SetBool("Scared", true);
                g.SetBool("Recovering", false);
            }
        }

        backgroundMusic.clip = scaredMusic;
        backgroundMusic.Play();

        scaredTimer = 10f;
        isScared = true;
    }

    public void GhostEaten(Animator ghostAnimator)
    {
        ghostAnimator.SetBool("Dead", true);
        ghostAnimator.SetBool("Scared", false);
        ghostAnimator.SetBool("Recovering", false);

        GameManager.Instance.AddScore(300); 
        backgroundMusic.clip = deadMusic;
        backgroundMusic.Play();

        StartCoroutine(ReviveGhostAfterDelay(ghostAnimator, 3f));
    }

    private System.Collections.IEnumerator ReviveGhostAfterDelay(Animator ghostAnimator, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (scaredTimer <= 0f)
        {
            ghostAnimator.SetBool("Dead", false);
        }
        else
        {
            ghostAnimator.SetBool("Dead", false);
            if (scaredTimer <= 3f)
                ghostAnimator.SetBool("Recovering", true);
            else
                ghostAnimator.SetBool("Scared", true);
        }
    }
}
