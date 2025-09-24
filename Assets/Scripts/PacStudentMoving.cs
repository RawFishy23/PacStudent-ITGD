using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Vector3[] path;
    public Animator animator;
    public AudioSource moveAudio;
    private int currentTarget = 0;
    private Vector3 startPos;
    private Tweener tweener;

    void Start()
    {
        if (path.Length == 0) return;
        startPos = transform.position;
        tweener = GetComponent<Tweener>();
        MoveToNext();
        if (moveAudio != null) moveAudio.Play();
    }

    void MoveToNext()
    {
        Vector3 endPos = path[currentTarget];
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / moveSpeed;
        tweener.AddTween(new Tween(transform, startPos, endPos, duration));
        UpdateAnimatorDirection(endPos - startPos);
        currentTarget = (currentTarget + 1) % path.Length;
        startPos = endPos;
        Invoke(nameof(MoveToNext), duration);
    }

    void UpdateAnimatorDirection(Vector3 direction)
    {
        if (animator == null) return;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0) animator.Play("Right");
            else animator.Play("Left");
        }
        else
        {
            if (direction.y > 0) animator.Play("Up");
            else animator.Play("Down");
        }
    }
}
