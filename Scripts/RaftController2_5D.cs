// RaftController2_5D.cs (only the animation part changed)
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RaftController2_5D : MonoBehaviour
{
    [Header("Movement")]
    public bool useLanes = true;
    public int lanes = 5;
    public float laneWidth = 2f;
    public float slideSpeed = 8f;
    public float freeMoveSpeed = 6f;
    public float maxX = 6f;

    [Header("Animation (optional)")]
    public Animator[] animators;      // <— multiple animators
    public string steerParam = "Steer";

    float _targetX;
    int _laneIndex;
    Transform _t;

    void Awake()
    {
        _t = transform;
        if (useLanes) { _laneIndex = 0; _targetX = 0f; }
        else _targetX = _t.position.x;
    }

    void Update()
    {
        float steer = ReadHorizontal(); // -1..1

        if (useLanes)
        {
            if (steer > 0.5f) _laneIndex++;
            else if (steer < -0.5f) _laneIndex--;

            int half = (lanes - 1) / 2;
            _laneIndex = Mathf.Clamp(_laneIndex, -half, half);
            _targetX = _laneIndex * laneWidth;

            var p = _t.position;
            p.x = Mathf.MoveTowards(p.x, _targetX, slideSpeed * Time.deltaTime);
            _t.position = p;
        }
        else
        {
            float target = _targetX + steer * freeMoveSpeed * Time.deltaTime;
            _targetX = Mathf.Clamp(target, -maxX, maxX);
            var p = _t.position;
            p.x = Mathf.MoveTowards(p.x, _targetX, freeMoveSpeed * Time.deltaTime);
            _t.position = p;
        }

        // drive ALL animators' steer param
        if (animators != null && !string.IsNullOrEmpty(steerParam))
        {
            for (int i = 0; i < animators.Length; i++)
                if (animators[i]) animators[i].SetFloat(steerParam, steer);
        }
    }

    float ReadHorizontal()
    {
        float x = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (useLanes)
            {
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) x = -1f;
                if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) x =  1f;
            }
            else
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            }
        }
        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.ReadValue().x;
            x = useLanes ? (stick > 0.5f ? 1f : (stick < -0.5f ? -1f : 0f)) : stick;
        }
#else
        x = Input.GetAxisRaw("Horizontal");
#endif
        return Mathf.Clamp(x, -1f, 1f);
    }
}
