using UnityEngine;

public class Tween
{
    public Transform target;
    public Vector3 startPos;
    public Vector3 endPos;
    public float duration;
    public float elapsed;

    public Tween(Transform target, Vector3 startPos, Vector3 endPos, float duration)
    {
        this.target = target;
        this.startPos = startPos;
        this.endPos = endPos;
        this.duration = duration;
        elapsed = 0f;
    }

    public bool UpdateTween(float deltaTime)
    {
        elapsed += deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        target.position = Vector3.Lerp(startPos, endPos, t);
        return t >= 1f;
    }
}
