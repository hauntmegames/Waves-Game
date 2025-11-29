using System.Collections;
using UnityEngine;

/// <summary>
/// StreetLampRandomToggle.cs
/// - Toggles a Light.enabled and a bulb GameObject (or Renderer) on/off at random intervals.
/// - Starts with a random initial delay so many lamps won't flash in perfect sync.
/// Reference image you posted: /mnt/data/01d02541-f808-493f-96e1-a43d25e4498e.png
/// </summary>
[AddComponentMenu("Utils/Street Lamp Random Toggle")]
public class StreetLampRandomToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Light component to toggle (Point/Spot/etc).")]
    public Light lampLight;
    [Tooltip("The bulb GameObject to enable/disable (mesh with emissive material).")]
    public GameObject bulbObject;

    [Header("Behavior")]
    [Tooltip("Minimum seconds between state changes")]
    public float minInterval = 0.8f;
    [Tooltip("Maximum seconds between state changes")]
    public float maxInterval = 2.0f;

    [Tooltip("Chance (0..1) that the lamp will be ON when sampled. 0 = always off, 1 = always on")]
    [Range(0f, 1f)]
    public float onChance = 0.7f;

    [Tooltip("If true, script will disable/enable the Renderer component on bulbObject instead of the whole GameObject")]
    public bool toggleRendererInstead = false;

    [Tooltip("If true, avoid having the exact same on/off state twice in a row")]
    public bool avoidRepeats = true;

    // internal
    private Renderer bulbRenderer;
    private bool lastState = false;
    private bool initialized = false;

    void Reset()
    {
        minInterval = 0.8f;
        maxInterval = 2.0f;
        onChance = 0.7f;
        toggleRendererInstead = false;
        avoidRepeats = true;
    }

    void Start()
    {
        // Safety: if nothing assigned, try to find reasonable defaults (cheap, local-only)
        if (lampLight == null) lampLight = GetComponent<Light>();

        if (bulbObject == null)
        {
            // try to find a child named "Bulb" or "bulb"
            Transform t = transform.Find("Bulb");
            if (t != null) bulbObject = t.gameObject;
        }

        if (bulbObject != null && toggleRendererInstead)
        {
            bulbRenderer = bulbObject.GetComponent<Renderer>();
            if (bulbRenderer == null)
            {
                // fallback: disable toggleRendererInstead if no renderer found
                toggleRendererInstead = false;
            }
        }

        // record initial state (if we have a light)
        if (lampLight != null)
            lastState = lampLight.enabled;
        else if (bulbObject != null)
            lastState = bulbObject.activeSelf;
        else
            lastState = false;

        // start coroutine with a small random initial delay so different instances desync
        float initial = Random.Range(0f, Mathf.Min(maxInterval, 1.2f)) + Random.value * 0.15f;
        StartCoroutine(RandomToggleRoutine(initial));

        initialized = true;
    }

    IEnumerator RandomToggleRoutine(float initialDelay)
    {
        // initial randomized wait to desync multiple lamps
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            bool newState = Random.value < onChance;

            // avoid repeating exact same state if desired
            if (avoidRepeats && newState == lastState)
            {
                // small chance to flip to a different state anyway to keep variety
                if (Random.value < 0.5f)
                    newState = !newState;
                // OR we could roll again: newState = Random.value < onChance; leaving it as above is fine
            }

            ApplyState(newState);
            lastState = newState;
        }
    }

    private void ApplyState(bool on)
    {
        // toggle the Light if present
        if (lampLight != null)
        {
            lampLight.enabled = on;
        }

        // toggle the bulb GameObject or its Renderer
        if (bulbObject != null)
        {
            if (toggleRendererInstead && bulbRenderer != null)
            {
                bulbRenderer.enabled = on;
            }
            else
            {
                // toggle active state of the whole GameObject (cheap and reliable)
                bulbObject.SetActive(on);
            }
        }
    }

    // Optional API: allow an external caller to force state
    public void ForceState(bool on)
    {
        if (!initialized) return;
        ApplyState(on);
        lastState = on;
    }
}
