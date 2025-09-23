using UnityEngine;

public class PacStudentMoving : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Vector3[] path;
    public Animator animator;
    public AudioSource moveAudio;

    private int currentTarget = 0;
    private Vector3 startPos;
    private float t = 0f;

    void Start()
    {
        if (path.Length == 0) return;

        startPos = transform.position;          
        if (moveAudio != null) moveAudio.Play();
    }

    void Update()
    {
        if (path.Length == 0) return;

        Vector3 endPos = path[currentTarget];
        float segmentLength = Vector3.Distance(startPos, endPos);

        t += moveSpeed * Time.deltaTime / segmentLength;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPos, endPos, t);

        Vector3 direction = (endPos - transform.position).normalized;
        if (animator != null)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))   
            {
                if (direction.x > 0)
                    animator.Play("Right");
                else if (direction.x < 0)
                    animator.Play("Left");
            }
            else
            {
                if (direction.y > 0)
                    animator.Play("Up");
                else if (direction.y < 0)
                    animator.Play("Down");
            }
        }

        if (t >= 1f)
        {
            startPos = endPos;                        
            currentTarget = (currentTarget + 1) % path.Length;
            t = 0f;                                   
        }
    }
}
