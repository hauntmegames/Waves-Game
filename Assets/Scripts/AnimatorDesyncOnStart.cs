// AnimatorDesyncOnStart.cs
using UnityEngine;

public class AnimatorDesyncOnStart : MonoBehaviour
{
    [Tooltip("Characters' Animators to desync.")]
    public Animator[] animators;

    [Tooltip("Exact state name to desync (e.g., \"HoldingIdle\").")]
    public string stateName = "HoldingIdle";   // the looping default/idle state
    public int layer = 0;

    [Tooltip("Random start point within the loop (0..1).")]
    public Vector2 startOffsetRange = new Vector2(0f, 1f);

    [Tooltip("Small per-actor speed variance, e.g., 0.95–1.05.")]
    public Vector2 speedRange = new Vector2(0.98f, 1.02f);

    void Start()
    {
        if (animators == null) return;

        int stateHash = Animator.StringToHash(stateName);

        foreach (var a in animators)
        {
            if (!a) continue;

            // Randomize where in the loop they start
            float t = Random.Range(startOffsetRange.x, startOffsetRange.y);
            a.Play(stateHash, layer, t);

            // Tiny speed variance so they drift out of sync over time
            a.speed = Random.Range(speedRange.x, speedRange.y);
        }
    }
}
