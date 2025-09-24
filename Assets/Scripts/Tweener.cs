using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private List<Tween> tweens = new List<Tween>();

    void Update()
    {
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            if (tweens[i].UpdateTween(Time.deltaTime))
                tweens.RemoveAt(i);
        }
    }

    public void AddTween(Tween tween)
    {
        tweens.Add(tween);
    }
}
