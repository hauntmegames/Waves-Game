using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraZoomBoom : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform target;

    [Header("Offsets")]
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);
    public Vector3 shoulderOffset = new Vector3(0.3f, 0f, 0f);

    [Header("Distances")]
    public float minDistance = 0.0f;      // 0 = true FP
    public float maxDistance = 3.0f;
    public float startDistance = 2.0f;
    public float zoomSpeed = 10f;
    public float scrollSensitivity = 1.0f;

    [Header("Collision Avoidance")]
    public LayerMask collisionMask = ~0;
    public float sphereRadius = 0.2f;
    public float wallBuffer = 0.05f;

    [Header("First-Person Helpers")]
    public float fpHideThreshold = 0.05f;
    public Renderer[] hideInFirstPerson;

    float _targetDistance;
    float _currentDistance;

    // --- Public API so other scripts can drive zoom ---
    public float CurrentDistance => _currentDistance;
    public void SetTargetDistance(float d, bool snap = false)
    {
        _targetDistance = Mathf.Clamp(d, minDistance, maxDistance);
        if (snap)
        {
            _currentDistance = _targetDistance;
        }
    }

    void Awake()
    {
        _targetDistance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        _currentDistance = _targetDistance;
        if (!target && transform.parent) target = transform.parent;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Wheel input
        float scroll = 0f;
    #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
    #else
        scroll = Input.GetAxis("Mouse ScrollWheel");
    #endif
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            _targetDistance -= scroll * scrollSensitivity;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }

        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, 1f - Mathf.Exp(-zoomSpeed * Time.deltaTime));

        // Place camera
        Vector3 pivotWorld = target.TransformPoint(pivotOffset);
        Vector3 desiredPos = pivotWorld + transform.right * shoulderOffset.x + transform.up * shoulderOffset.y - transform.forward * _currentDistance;

        // Collision
        Vector3 toCam = desiredPos - pivotWorld;
        float dist = toCam.magnitude;
        Vector3 dir = dist > 0.0001f ? toCam / dist : -transform.forward;

        if (dist > 0f && Physics.SphereCast(pivotWorld, sphereRadius, dir, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredPos = pivotWorld + dir * Mathf.Max(0f, hit.distance - wallBuffer);
            _currentDistance = (desiredPos - pivotWorld).magnitude;
        }

        transform.position = desiredPos;

        // Hide/show renderers in FP
        bool firstPerson = _currentDistance <= fpHideThreshold + 0.0001f;
        if (hideInFirstPerson != null)
        {
            for (int i = 0; i < hideInFirstPerson.Length; i++)
            {
                var r = hideInFirstPerson[i];
                if (!r) continue;
                r.enabled = !firstPerson;
            }
        }
    }
}
