using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if USE_DIALOGUE_SYSTEM
using PixelCrushers;
#endif

public class RaftCondition : MonoBehaviour
{
    public static RaftCondition Instance { get; private set; }

    [Header("Durability")]
    public float max = 100f;
    public float start = 100f;
    public float damagePerHit = 12f;   // how much to drain per obstacle
    public float regenPerSecond = 0f;  // 0 = no regen

    [Header("UI (optional)")]
    public Slider slider;              // assign a UI Slider (0..1 normalized)
    public Image  fillImage;           // OR assign a Filled Image (type Filled)
    public float  smoothUiSpeed = 6f;  // how fast the UI lerps

    [Header("Events")]
    public UnityEvent onDepleted;      // hook “restart level”, etc.

    float _current;
    float _uiValue;                    // smoothed 0..1

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _current = Mathf.Clamp(start, 0f, max);
        _uiValue = _current / Mathf.Max(1f, max);
        UpdateImmediateUI();
        PushToDialogue();
    }

    void Update()
    {
        // optional regen
        if (regenPerSecond > 0f && _current > 0f && _current < max)
        {
            _current = Mathf.Min(max, _current + regenPerSecond * Time.deltaTime);
        }

        // smooth UI toward target
        float target = _current / Mathf.Max(1f, max);
        _uiValue = Mathf.MoveTowards(_uiValue, target, smoothUiSpeed * Time.deltaTime);
        ApplyUI(_uiValue);
    }

    public void Damage() => Damage(damagePerHit);

    public void Damage(float amount)
    {
        if (_current <= 0f) return;
        _current = Mathf.Max(0f, _current - Mathf.Abs(amount));
        PushToDialogue();

        if (_current <= 0f)
        {
            onDepleted?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (_current <= 0f) return;
        _current = Mathf.Min(max, _current + Mathf.Abs(amount));
        PushToDialogue();
    }

    public float Current => _current;
    public float Normalized => _current / Mathf.Max(1f, max);

    void ApplyUI(float normalized)
    {
        if (slider) slider.value = normalized;
        if (fillImage) fillImage.fillAmount = normalized;
    }

    void UpdateImmediateUI()
    {
        float n = _current / Mathf.Max(1f, max);
        if (slider)
        {
            slider.minValue = 0f; slider.maxValue = 1f;
            slider.value = n;
        }
        if (fillImage) fillImage.fillAmount = n;
    }

    void PushToDialogue()
    {
        #if USE_DIALOGUE_SYSTEM
        DialogueLua.SetVariable("RaftCondition", Mathf.RoundToInt(Normalized * 100f));
        DialogueLua.SetVariable("RaftConditionFloat", Normalized);
        #endif
    }
}
