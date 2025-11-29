using UnityEngine;

public class StoplightSimpleFlash : MonoBehaviour
{
    [Header("Assign your 3 point lights")]
    public Light redLight;
    public Light yellowLight;
    public Light greenLight;

    [Header("Timing")]
    [Tooltip("Time in seconds between state changes")]
    public float interval = 1f;

    [Header("Behavior chances (0 to 1)")]
    [Tooltip("Chance to have exactly 2 lights on")]
    public float chanceTwoOn = 0.35f;
    [Tooltip("Chance to have all 3 lights on")]
    public float chanceAllOn = 0.10f;
    [Tooltip("Chance to have none on (dark)")]
    public float chanceAllOff = 0.02f;

    private float timer;

    void Reset()
    {
        // sensible defaults
        interval = 1f;
        chanceTwoOn = 0.35f;
        chanceAllOn = 0.10f;
        chanceAllOff = 0.02f;
    }

    void Start()
    {
        // do nothing if lights not assigned — won't touch scene
        if (redLight == null || yellowLight == null || greenLight == null)
        {
            Debug.LogWarning("[StoplightSimpleFlash] One or more lights not assigned. Script will remain idle.");
            enabled = false;
            return;
        }

        // initialize timer so it changes immediately if desired
        timer = interval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = interval;

        // Decide new state
        float roll = Random.value;

        bool[] state;

        if (roll < chanceAllOff)
        {
            state = new bool[] { false, false, false };
        }
        else if (roll < chanceAllOff + chanceAllOn)
        {
            state = new bool[] { true, true, true };
        }
        else if (Random.value < chanceTwoOn)
        {
            int pair = Random.Range(0, 3);
            switch (pair)
            {
                case 0: state = new bool[] { true, true, false }; break; // red+yellow
                case 1: state = new bool[] { true, false, true }; break; // red+green
                default: state = new bool[] { false, true, true }; break; // yellow+green
            }
        }
        else
        {
            int single = Random.Range(0, 3);
            state = new bool[] { single == 0, single == 1, single == 2 };
        }

        // Apply instantly (very cheap)
        redLight.enabled = state[0];
        yellowLight.enabled = state[1];
        greenLight.enabled = state[2];
    }
}
