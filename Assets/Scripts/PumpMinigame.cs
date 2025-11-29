// PumpMinigame_Debug.cs  — instrumented version of your PumpMinigame with safety cleanup & logs.
using UnityEngine;
using UnityEngine.Events;
#if TMP_PRESENT || TEXTMESHPRO
using TMPro;
#endif
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UI;
#endif
using PixelCrushers.DialogueSystem;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // new input
#endif
using UnityEngine.EventSystems;

public class PumpMinigame_Debug : MonoBehaviour
{
    [Header("Refs")]
    public Transform handle;
    public Transform cameraAnchor;
    public CameraDock cameraDock;
    public CameraZoomBoom zoomBoom;
    public Collider interactCollider;

    [Header("Handle motion (local Y)")]
    public float handleUpY = 0.25f;
    public float handleDownY = -0.20f;
    public float moveSpeed = 10f;          // how fast the handle travels to target Y

    [Header("Input (alternate 1 ↔ 2)")]
#if ENABLE_INPUT_SYSTEM
    public Key keyDown = Key.Digit1;       // press to push handle DOWN
    public Key keyUp   = Key.Digit2;       // press to pull handle UP
#else
    public KeyCode keyDown = KeyCode.Alpha1;
    public KeyCode keyUp   = KeyCode.Alpha2;
#endif

    [Header("Progress")]
    public float progressPerPump = 0.12f;  // added when a full down→up cycle completes
    public float decayPerSecond  = 0f;
    public string dialogueVarName = "RaftInflated";

    [Header("UI (optional)")]
    public GameObject uiRoot;
#if TMP_PRESENT || TEXTMESHPRO
    public TMP_Text promptText;
    public TMP_Text progressText;
#endif
#if UNITY_2019_1_OR_NEWER
    public Slider progressBar;
#endif

    [Header("Events")]
    public UnityEvent onStarted;
    public UnityEvent<float> onProgress;
    public UnityEvent onCompleted;
    public UnityEvent onCanceled;

    // ---- internals ----
    bool _active;
    float _progress;
    float _targetY;
    bool _lastStrokeWasDown = false;   // becomes true after a valid DOWN press

    float _savedDistance = -1f;        // for restoring 3rd-person zoom after

    // Usable calls this
    public void OnUse() => Begin();

    [ContextMenu("TEST Begin (Editor)")] void _TestBegin() => Begin();

    public void Begin()
    {
        if (_active || !handle || !cameraDock || !cameraAnchor)
        {
            Debug.Log($"[PumpMinigame_Debug] Begin aborted. active:{_active} handle:{handle!=null} cameraDock:{cameraDock!=null} cameraAnchor:{cameraAnchor!=null}");
            return;
        }

        Debug.Log("[PumpMinigame_Debug] Begin()");
        _active = true;
        _targetY = handle.localPosition.y;
        _lastStrokeWasDown = false;

        // snap zoom to FP & remember previous
        if (zoomBoom)
        {
            _savedDistance = zoomBoom.CurrentDistance;
            zoomBoom.SetTargetDistance(zoomBoom.minDistance, snap: true);
            Debug.Log($"[PumpMinigame_Debug] zoomBoom snap to minDistance. savedDistance={_savedDistance}");
        }

        if (interactCollider) {
            interactCollider.enabled = false;
            Debug.Log($"[PumpMinigame_Debug] interactCollider disabled: {interactCollider.name}");
        }

        if (uiRoot) {
            uiRoot.SetActive(true);
            Debug.Log($"[PumpMinigame_Debug] uiRoot set active: {uiRoot.name}");
        }
#if TMP_PRESENT || TEXTMESHPRO
        if (promptText) promptText.text = "Pump! 1 = down, 2 = up";
#endif
        cameraDock.Begin(cameraAnchor);
        onStarted?.Invoke();
    }

    public void Cancel()
    {
        if (!_active)
        {
            // Still perform cleanup just in case something left in half-state
            _ForceCleanup();
            Debug.Log("[PumpMinigame_Debug] Cancel() called but _active was false. Forced cleanup executed.");
            return;
        }

        Debug.Log("[PumpMinigame_Debug] Cancel()");
        _active = false;

        if (zoomBoom && _savedDistance >= 0f)
        {
            zoomBoom.SetTargetDistance(_savedDistance, snap: true);
            _savedDistance = -1f;
            Debug.Log("[PumpMinigame_Debug] zoomBoom restored saved distance.");
        }

        // Restore collider & UI
        if (interactCollider) {
            interactCollider.enabled = true;
            Debug.Log($"[PumpMinigame_Debug] interactCollider re-enabled: {interactCollider.name}");
        }

        if (uiRoot) {
            uiRoot.SetActive(false);
            Debug.Log($"[PumpMinigame_Debug] uiRoot set inactive: {uiRoot.name}");
            var cg = uiRoot.GetComponent<CanvasGroup>();
            if (cg != null) {
                cg.blocksRaycasts = false;
                Debug.Log($"[PumpMinigame_Debug] uiRoot CanvasGroup.blocksRaycasts set false.");
            }
        }

        // Clear any selected UI so EventSystem won't hold focus on invisible object
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[PumpMinigame_Debug] EventSystem.current selection cleared.");
        }
        else
        {
            Debug.LogWarning("[PumpMinigame_Debug] No EventSystem found when cancelling! This could block UI clicks.");
        }

        // Ensure cursor is unlocked/visible so UI can be clicked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log($"[PumpMinigame_Debug] Cursor unlocked/visible.");

        cameraDock.End();
        onCanceled?.Invoke();
    }

    void Update()
    {
        if (!_active) return;

        if (PressedEscape()) { Cancel(); return; }

        // optional decay
        if (decayPerSecond > 0f && _progress > 0f)
            SetProgress(Mathf.Max(0f, _progress - decayPerSecond * Time.deltaTime));

        // read half-strokes
        if (Pressed(keyDown))
        {
            // only accept DOWN if we're not already counting this part
            _targetY = handleDownY;
            _lastStrokeWasDown = true; // ready to complete on next UP
        }
        else if (Pressed(keyUp))
        {
            _targetY = handleUpY;

            // Count progress only when we just completed a full cycle (down -> up)
            if (_lastStrokeWasDown)
            {
                _lastStrokeWasDown = false;
                AddOnePumpProgress();
            }
        }

        // animate handle toward target
        var lp = handle.localPosition;
        lp.y = Mathf.MoveTowards(lp.y, _targetY, moveSpeed * Time.deltaTime);
        handle.localPosition = lp;
    }

    void AddOnePumpProgress()
    {
        SetProgress(Mathf.Clamp01(_progress + progressPerPump));
        Debug.Log($"[PumpMinigame_Debug] AddOnePumpProgress -> progress {_progress}");

        if (_progress >= 1f - 0.0001f)
        {
            _progress = 1f;
            if (!string.IsNullOrEmpty(dialogueVarName))
                DialogueLua.SetVariable(dialogueVarName, true);

            if (uiRoot) uiRoot.SetActive(false); // hide UI immediately on complete
            Debug.Log("[PumpMinigame_Debug] Minigame completed; uiRoot hidden and onCompleted invoked.");

            onCompleted?.Invoke();
            // small delay so final pose is visible, then exit
            StartCoroutine(FinishNextFrame());
        }
    }

    System.Collections.IEnumerator FinishNextFrame()
    {
        Debug.Log("[PumpMinigame_Debug] FinishNextFrame() waiting 0.15s before Cancel()");
        yield return new WaitForSeconds(0.15f);

        // Always do full cleanup when finishing
        Cancel();
        yield return null;
    }

    void SetProgress(float v)
    {
        _progress = v;
#if TMP_PRESENT || TEXTMESHPRO
        if (progressText) progressText.text = Mathf.RoundToInt(_progress * 100f) + "%";
#endif
#if UNITY_2019_1_OR_NEWER
        if (progressBar) progressBar.value = _progress;
#endif
        onProgress?.Invoke(_progress);
    }

    // -------- input helpers --------
    bool PressedEscape()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    bool Pressed(
#if ENABLE_INPUT_SYSTEM
        Key k
#else
        KeyCode k
#endif
    )
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb[k].wasPressedThisFrame;
#else
        return Input.GetKeyDown(k);
#endif
    }

    // Force cleanup if something weird happens or object disabled
    void _ForceCleanup()
    {
        if (interactCollider) interactCollider.enabled = true;
        if (uiRoot) {
            uiRoot.SetActive(false);
            var cg = uiRoot.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = false;
        }
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        // make sure we restore state if this script or object is destroyed/disabled for any reason
        _ForceCleanup();
        Debug.Log("[PumpMinigame_Debug] OnDisable cleanup executed.");
    }
}
