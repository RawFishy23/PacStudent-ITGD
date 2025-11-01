using UnityEngine;

public class CherryMover : MonoBehaviour
{
    private Vector2 targetPos;
    private float moveSpeed;
    private CherryController controller;

    public void Initialize(Vector2 target, float speed, CherryController owner)
    {
        targetPos = target;
        moveSpeed = speed;
        controller = owner;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            controller.NotifyCherryDestroyed();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            controller.NotifyCherryDestroyed();
            GameManager.Instance.AddScore(100);
            Destroy(gameObject);
        }
    }
}
