using System.Collections;
using UnityEngine;

[AddComponentMenu("Utils/Simple Street Lamp Flicker")]
public class SimpleStreetLampFlicker : MonoBehaviour
{
    public enum Mode { Toggle, Intensity }

    [Header("Basic")]
    public Light lamp;                     // assign or it will try GetComponent<Light>()
    public Mode mode = Mode.Intensity;

    [Header("Timing")]
    public float minInterval = 0.05f;      // shortest pause between flicker events
    public float maxInterval = 0.35f;      // longest pause between flicker events

    [Header("Toggle mode")]
    [Range(0f, 1f)] public float onChance = 0.7f; // chance the light stays on when sampled

    [Header("Intensity mode")]
    public float minIntensity = 0.2f;      // dimmest
    public float maxIntensity = 1.2f;      // brightest
    public float fadeDuration = 0.08f;     // time to lerp to new intensity

    void Reset()
    {
        minInterval = 0.05f;
        maxInterval = 0.35f;
        onChance = 0.7f;
        minIntensity = 0.2f;
        maxIntensity = 1.2f;
        fadeDuration = 0.08f;
        mode = Mode.Intensity;
    }

    void OnEnable()
    {
        if (lamp == null) lamp = GetComponent<Light>();
        if (lamp == null)
        {
            Debug.LogWarning("[SimpleStreetLampFlicker] No Light assigned or found. Disabling script.");
            enabled = false;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(FlickerRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator FlickerRoutine()
    {
        if (mode == Mode.Toggle)
        {
            while (true)
            {
                float wait = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(wait);

                lamp.enabled = Random.value < onChance;
            }
        }
        else // Intensity
        {
            float current = lamp.intensity;
            while (true)
            {
                float wait = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(wait);

                float target = Random.Range(minIntensity, maxIntensity);
                // smooth lerp to the new intensity (very cheap)
                float t = 0f;
                float start = current;
                if (fadeDuration <= 0f)
                {
                    current = target;
                    lamp.intensity = current;
                    continue;
                }
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    current = Mathf.Lerp(start, target, t / fadeDuration);
                    lamp.intensity = current;
                    yield return null;
                }
                current = target;
                lamp.intensity = current;
            }
        }
    }
}
