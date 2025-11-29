using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ShellPickupOnUse : MonoBehaviour
{
    [Header("Player anim")]
    public Animator playerAnimator;
    public string pickupTrigger = "Pickup";

    [Header("Dialogue System variable")]
    public string variableName = "ShellsCollected";
    public int incrementBy = 1;

    [Header("Despawn timing")]
    public float vanishDelay = 0.75f;   // set to match your pickup clip
    public bool hideVisualImmediately = false;

    Collider[] _cols;
    Renderer[] _renders;
    bool _used;

    void Awake()
    {
        _cols = GetComponentsInChildren<Collider>(true);
        _renders = GetComponentsInChildren<Renderer>(true);
    }

    public void OnUse()
    {
        if (_used) return;
        _used = true;

        if (playerAnimator && !string.IsNullOrEmpty(pickupTrigger))
            playerAnimator.SetTrigger(pickupTrigger);

        // increment variable
        int n = DialogueLua.GetVariable(variableName).asInt;
        DialogueLua.SetVariable(variableName, n + incrementBy);

        // block re-use right away
        foreach (var c in _cols) c.enabled = false;

        // optionally hide mesh immediately (or leave visible until delay)
        if (hideVisualImmediately)
            foreach (var r in _renders) r.enabled = false;

        StartCoroutine(DespawnAfterDelay());
    }

    System.Collections.IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(vanishDelay);
        Destroy(gameObject);
    }
}
