using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentMoving : MonoBehaviour
{

    public float moveSpeed = 2f; 
    public Vector3[] path;
    public Animator animator; 
    private int currentTarget = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveAlongPath());
    }

    IEnumerator MoveAlongPath()
    {
        while (true)
        {
            Vector3 start = transform.position;
            Vector3 end = path[currentTarget];
            float distance = Vector3.Distance(start, end);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;

            currentTarget = (currentTarget + 1) % path.Length;
        }
    }
}
