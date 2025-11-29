// UnderwaterFreeSwim.cs
// Add to the player root (same object as CharacterController).
// Enables full 3D swim (no gravity), using camera forward/right for movement.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class UnderwaterFreeSwim : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;              // drag your gameplay camera

    [Header("Speed / Feel")]
    public float swimSpeed = 3.0f;                 // base horizontal speed
    public float sprintMultiplier = 1.6f;          // hold sprint to go faster
    public float verticalSpeed = 3.0f;             // up/down speed
    public float acceleration = 6.0f;              // how fast we reach target velocity
    public float waterDrag = 2.0f;                 // how quickly velocity bleeds off

    [Header("Input (optional)")]
    public bool useNewInputSystem = true;          // auto if ENABLE_INPUT_SYSTEM
    public bool invertVertical = false;

    [Header("Water Current (optional)")]
    public Vector3 constantCurrent = Vector3.zero; // e.g., (0.3,0,0)

    CharacterController cc;
    Vector3 velocity;                              // smoothed world velocity

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (!cameraTransform)
        {
            Camera mainCam = Camera.main;
            if (mainCam) cameraTransform = mainCam.transform;
        }

        // We enable this script only when in swim mode (via SwimModeSwitcher).
        enabled = false;
    }

    void Update()
    {
        if (!cameraTransform) return;

        // --- Read horizontal input (WASD / left stick) ---
        Vector2 move = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        if (useNewInputSystem)
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;

            float x = 0f, y = 0f;
            if (kb != null)
            {
                if (kb.aKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed) x += 1f;
                if (kb.sKey.isPressed) y -= 1f;
                if (kb.wKey.isPressed) y += 1f;
            }
            if (gp != null)
            {
                Vector2 ls = gp.leftStick.ReadValue();
                x += ls.x; y += ls.y;
            }
            move = new Vector2(x, y);
        }
        else
#endif
        {
            move = new Vector2(
                (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)
            );
        }
        move = Vector2.ClampMagnitude(move, 1f);

        // --- Up/Down (Space / LeftCtrl) ---
        float upDown = 0f;
#if ENABLE_INPUT_SYSTEM
        if (useNewInputSystem)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.isPressed) upDown += 1f;
                if (kb.leftCtrlKey.isPressed || kb.cKey.isPressed) upDown -= 1f;
            }
            var gp = Gamepad.current;
            if (gp != null)
                upDown += Mathf.Round(gp.rightTrigger.ReadValue()) - Mathf.Round(gp.leftTrigger.ReadValue());
        }
        else
#endif
        {
            if (Input.GetKey(KeyCode.Space))       upDown += 1f;
            if (Input.GetKey(KeyCode.LeftControl)) upDown -= 1f;
        }
        if (invertVertical) upDown = -upDown;

        // Sprint
        bool sprinting =
#if ENABLE_INPUT_SYSTEM
            (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
#else
            Input.GetKey(KeyCode.LeftShift);
#endif

        float speed = swimSpeed * (sprinting ? sprintMultiplier : 1f);

        // Camera-aligned axes (horizontal ignores camera pitch)
        Vector3 fwd = cameraTransform.forward; fwd.y = 0f;
        fwd = fwd.sqrMagnitude > 0.001f ? fwd.normalized : transform.forward;
        Vector3 right = cameraTransform.right; right.y = 0f; right.Normalize();

        // Target world velocity
        Vector3 targetWorldVel =
            (fwd * move.y + right * move.x) * speed +
            Vector3.up * (upDown * verticalSpeed) +
            constantCurrent;

        // Smooth toward target & apply drag
        velocity = Vector3.MoveTowards(velocity, targetWorldVel, acceleration * Time.deltaTime);
        velocity = Vector3.Lerp(velocity, Vector3.zero, 1f - Mathf.Exp(-waterDrag * Time.deltaTime));

        // Move CharacterController (no gravity in swim)
        cc.Move(velocity * Time.deltaTime);
    }
}
