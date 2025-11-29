using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("Target To Shake")]
    [Tooltip("Leave empty to auto-use Camera.main (your Main Camera).")]
    public Transform target;

    [Header("Shake Tuning")]
    [Tooltip("How fast the shake fades per second.")]
    public float traumaDecay = 2.5f;
    [Tooltip("Noise frequency (wiggle speed).")]
    public float frequency = 28f;
    [Tooltip("Max positional offset at full shake (meters).")]
    public float maxPosAmplitude = 0.05f;   // ~5 cm
    [Tooltip("Max rotational offset at full shake (degrees).")]
    public float maxRotAmplitude = 0.8f;    // small tilt

    float trauma;                            // 0..1
    Vector3    baseLocalPos;
    Quaternion baseLocalRot;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!target)
        {
            var cam = Camera.main;
            target = cam ? cam.transform : transform;
        }

        baseLocalPos = target.localPosition;
        baseLocalRot = target.localRotation;
    }

    void OnDisable()
    {
        if (!target) return;
        target.localPosition = baseLocalPos;
        target.localRotation = baseLocalRot;
        trauma = 0f;
    }

    // Run AFTER other camera movers (e.g., your Boom/Zoom on PlayerCameraRoot)
    void LateUpdate()
    {
        if (!target) return;

        // Always start from whatever other scripts set this frame:
        baseLocalPos = target.localPosition;
        baseLocalRot = target.localRotation;

        if (trauma <= 0f) return;

        float t = Time.time * frequency;
        float s = trauma * trauma; // nicer falloff curve

        float nx = Mathf.PerlinNoise(t, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(t, t)   * 2f - 1f;

        Vector3 posOff = new Vector3(nx, ny, 0f) * (maxPosAmplitude * s);
        Vector3 rotOff = new Vector3(ny, nx, nz) * (maxRotAmplitude * s);

        target.localPosition = baseLocalPos + posOff;
        target.localRotation = baseLocalRot * Quaternion.Euler(rotOff);

        trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);
    }

    /// <summary>Kick the shaker (0..1; try 0.35f for hits).</summary>
    public void Shake(float intensity)
    {
        trauma = Mathf.Clamp01(trauma + intensity);
    }
}
