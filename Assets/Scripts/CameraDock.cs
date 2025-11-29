// CameraDock.cs  — simple "go to anchor" camera lock (patched to avoid stealing UI focus)
using UnityEngine;
using UnityEngine.EventSystems;
#if PIXELCRUSHERS
using PixelCrushers.DialogueSystem;
#endif

public class CameraDock : MonoBehaviour
{
    [Header("Control to disable during dock")]
    public MonoBehaviour movementController;   // your FirstPersonController (or mover)
    public MonoBehaviour lookController;       // your look script (if separate)

    [Header("Blend")]
    public float moveSpeed = 10f;
    public float rotateSpeed = 12f;

    Transform _anchor;
    bool _active;
    Vector3 _origPos; Quaternion _origRot; Transform _origParent;

    void Awake(){ _origParent = transform.parent; _origPos = transform.localPosition; _origRot = transform.localRotation; }

    public void Begin(Transform anchor)
    {
        _anchor = anchor; _active = true;

        // If the player is currently interacting with UI or a dialogue is active, do not steal cursor/input.
        bool uiActive = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        #if PIXELCRUSHERS
        bool convoActive = DialogueManager.isConversationActive;
        #else
        bool convoActive = false;
        #endif

        if (!uiActive && !convoActive)
        {
            if (movementController) movementController.enabled = false;
            if (lookController)     lookController.enabled     = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // else: leave existing controllers/cursor state alone while UI/dialogue active
    }

    public void End()
    {
        _active = false; _anchor = null;

        bool uiActive = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        #if PIXELCRUSHERS
        bool convoActive = DialogueManager.isConversationActive;
        #else
        bool convoActive = false;
        #endif

        // Only restore gameplay control/cursor when no UI/dialogue is active.
        if (!uiActive && !convoActive)
        {
            if (movementController) movementController.enabled = true;
            if (lookController)     lookController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // else: keep controllers/cursor as they are so we don't steal focus

        // snap back to original local pose over a couple frames
        transform.SetParent(_origParent, true);
        transform.localPosition = _origPos;
        transform.localRotation = _origRot;
    }

    void LateUpdate()
    {
        if (!_active || !_anchor) return;
        transform.position = Vector3.Lerp(transform.position, _anchor.position, 1f - Mathf.Exp(-moveSpeed * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(transform.rotation, _anchor.rotation, 1f - Mathf.Exp(-rotateSpeed * Time.deltaTime));
    }
}
