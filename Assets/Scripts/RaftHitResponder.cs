// RaftHitResponder.cs
using UnityEngine;
#if USE_DIALOGUE_SYSTEM
using PixelCrushers;
#endif

public class RaftHitResponder : MonoBehaviour
{
    [Header("Anim Trigger")]
    [Tooltip("Assign the Animators of the characters in the raft that should play the collide animation.")]
    public Animator[] animators;                 // e.g., both characters in the raft
    public string collideTrigger = "Collide";    // Trigger parameter on their controllers

    [Header("Hit Logic")]
    public float invulnSeconds   = 0.40f;        // ignore rapid double hits
    public float shakeIntensity  = 0.35f;        // 0..1
    public AudioSource hitSfx;                   // optional SFX

    [Header("Speed Bump")]
    public bool  slowOnHit         = true;
    [Range(0.1f,1f)]
    public float slowFactor        = 0.70f;      // multiply speed (0.7 = 30% slow)
    public float minSpeedAfterHit  = 4f;         // don’t drop below this
    public float recoverSeconds    = 1.0f;       // ease back to pre-hit speed

    float cooldown;

    /// <summary>Call this from obstacles when the raft is hit.</summary>
    public void OnHit(Vector3 hitPos)
    {
        if (cooldown > 0f) return;
        cooldown = invulnSeconds;

        // Trigger all assigned animators
        if (!string.IsNullOrEmpty(collideTrigger) && animators != null)
        {
            for (int i = 0; i < animators.Length; i++)
                if (animators[i]) animators[i].SetTrigger(collideTrigger);
        }

        // Camera shake
        if (CameraShaker.Instance) CameraShaker.Instance.Shake(shakeIntensity);

        // SFX
        if (hitSfx) hitSfx.Play();

        // NEW: drain the raft condition bar (if present)
        if (RaftCondition.Instance) RaftCondition.Instance.Damage();

        // Optional slowdown via TrackMover API (no direct Speed writes)
        if (slowOnHit && TrackMover.Instance)
            TrackMover.Instance.ApplyHitSlowdown(slowFactor, minSpeedAfterHit, recoverSeconds);

        // Optional Dialogue System stat
        #if USE_DIALOGUE_SYSTEM
        DialogueLua.SetVariable("RaftHits",
            DialogueLua.GetVariable("RaftHits").AsInt + 1);
        #endif
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }
}
