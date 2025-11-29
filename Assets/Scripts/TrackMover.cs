// TrackMover.cs  — global forward speed controller + safe slowdown API
using UnityEngine;
using System.Collections;

public class TrackMover : MonoBehaviour
{
    public static TrackMover Instance { get; private set; }

    // Current global scroll speed (read-only outside)
    public static float Speed { get; private set; }

    [Header("Global Forward Speed")]
    public float startSpeed = 8f;
    public float maxCruiseSpeed = 18f;
    public float accelPerSecond = 0.5f;
    public bool running = true;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Speed = startSpeed;
    }

    void Update()
    {
        if (!running) return;
        // simple ramp toward cruise
        Speed = Mathf.MoveTowards(Speed, maxCruiseSpeed, accelPerSecond * Time.deltaTime);
    }

    // --- Public control helpers ---

    /// <summary>Instantly set current speed.</summary>
    public void ForceSpeed(float value) => Speed = Mathf.Max(0f, value);

    /// <summary>Apply a temporary slowdown, then ease back to the pre-hit speed.</summary>
    public void ApplyHitSlowdown(float factor, float minSpeed, float recoverSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(SlowAndRecoverRoutine(factor, minSpeed, recoverSeconds));
    }

    IEnumerator SlowAndRecoverRoutine(float factor, float minSpeed, float recoverSeconds)
    {
        float before = Speed;
        float after = Mathf.Max(before * Mathf.Clamp01(factor), minSpeed);
        Speed = after;

        if (recoverSeconds > 0.01f)
        {
            float t = 0f;
            while (t < recoverSeconds)
            {
                t += Time.deltaTime;
                Speed = Mathf.Lerp(after, before, t / recoverSeconds);
                yield return null;
            }
        }
        Speed = before;
    }
}
