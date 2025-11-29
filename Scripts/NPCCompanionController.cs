// NPCCompanionController.cs (stable idle + safe jump echo)
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class NPCCompanionController : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform playerRoot;
    public Animator  playerAnimator;

    [Header("Distances (XZ plane)")]
    public float followDistance      = 3.0f;   // desired gap to hold
    public float faceToFaceDistance  = 1.6f;   // only for convo-facing when player idle
    public float rejoinDistance      = 6.0f;   // run if farther than this
    public float slowRadius          = 3.0f;   // ease into stop point
    public float minSeparation       = 1.0f;   // never cross this
    public float stopDeadZone        = 0.20f;  // snap to idle when within this of stop point

    [Header("Movement (Land)")]
    public float moveSpeed   = 3.2f;
    public float runSpeed    = 5.0f;
    public float accel       = 12f;
    public float rotSpeed    = 10f;
    public float gravity     = -15f;

    [Header("Movement (Swim)")]
    public float swimSpeed     = 2.2f;
    public float swimTurnSpeed = 6f;

    [Header("Swimming Height Matching")]
    public float swimYOffset      = -0.20f;
    public float swimYFollowSpeed = 3.0f;

    [Header("Behaviour")]
    public bool holdWhenPlayerAirborne = true;

    [Header("Animator Params")]
    public string speedParam    = "Speed";     // float (m/s)
    public string groundedParam = "Grounded";  // bool
    public string swimmingParam = "Swimming";  // bool
    public string jumpTrigger   = "Jump";      // trigger (optional)

    [Header("Jump Echo Guard")]
    public float jumpEchoCooldown = 0.35f;     // min seconds between echo jumps
    public float jumpEchoMaxRange = 5.0f;      // only echo if within this XZ distance

    CharacterController cc;
    Animator anim;
    float currentSpeed = 0f;
    float vVel = 0f;

    bool  prevPlayerGrounded = true;
    float jumpEchoTimer = 0f;

    void Awake()
    {
        cc   = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (!playerRoot)     Debug.LogWarning("[NPCCompanionController] Assign playerRoot.");
        if (!playerAnimator) Debug.LogWarning("[NPCCompanionController] Assign playerAnimator (child).");
    }

    void Update()
    {
        jumpEchoTimer -= Time.deltaTime;

        if (!playerRoot)
        {
            DriveAnimator(0f, true, false);
            return;
        }

        // --- Player state sample ---
        bool playerSwimming = playerAnimator ? SafeGetBool(playerAnimator, swimmingParam) : false;
        bool playerGrounded = playerAnimator ? SafeGetBool(playerAnimator, groundedParam) : true;
        float playerSpeed   = playerAnimator ? playerAnimator.GetFloat(speedParam) : 0f;

        // --- XZ geometry ---
        Vector3 npcPos     = transform.position;
        Vector3 playerPos  = playerRoot.position;
        Vector3 toPlayerXZ = playerPos - npcPos; toPlayerXZ.y = 0f;
        float   distXZ     = toPlayerXZ.magnitude;
        Vector3 dirToPlayer = distXZ > 0.001f ? toPlayerXZ / distXZ : transform.forward;

        // Stop point: keep standoff distance
        Vector3 stopPointXZ = playerPos - dirToPlayer * followDistance; stopPointXZ.y = npcPos.y;
        Vector3 toStop      = stopPointXZ - npcPos; toStop.y = 0f;
        float   distToStop  = toStop.magnitude;
        Vector3 dirToStop   = distToStop > 0.001f ? toStop / distToStop : Vector3.zero;

        // Convo face-to-face (only when very close, idle, grounded, not swimming)
        bool shouldFaceToFace = distXZ <= faceToFaceDistance
                                && playerSpeed < 0.05f
                                && playerGrounded
                                && !playerSwimming;

        // --- Desired horizontal motion ---
        Vector3 desiredMove = Vector3.zero;
        float baseSpeed = playerSwimming ? swimSpeed : moveSpeed;
        float maxSpeed  = playerSwimming ? swimSpeed : runSpeed;

        if (shouldFaceToFace)
        {
            desiredMove = Vector3.zero;

            if (toPlayerXZ.sqrMagnitude > 0.0001f)
            {
                Quaternion face = Quaternion.LookRotation(dirToPlayer, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, face, rotSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Hard minimum separation
            if (distXZ < minSeparation)
            {
                desiredMove = -dirToPlayer * baseSpeed;
            }
            else
            {
                // Arrival behavior toward stop point
                if (distToStop <= stopDeadZone)
                {
                    desiredMove = Vector3.zero; // snap to idle in the pocket
                }
                else
                {
                    float arriveT   = Mathf.Clamp01(distToStop / Mathf.Max(0.001f, slowRadius));
                    float targetSpd = Mathf.Lerp(0f, baseSpeed, arriveT);
                    if (distXZ > rejoinDistance) targetSpd = maxSpeed;
                    desiredMove = dirToStop * targetSpd;

                    // Don't rush under the airborne player if we're already close
                    if (holdWhenPlayerAirborne && !playerGrounded && distXZ <= followDistance + 0.25f)
                        desiredMove = Vector3.zero;
                }
            }

            // Face direction of travel
            if (desiredMove.sqrMagnitude > 0.01f)
            {
                Quaternion look = Quaternion.LookRotation(desiredMove.normalized, Vector3.up);
                float turn = (playerSwimming ? swimTurnSpeed : rotSpeed) * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turn);
            }
        }

        // Smooth horizontal speed & apply
        float desiredMag = desiredMove.magnitude;
        // Stronger decel when slowing down so we actually hit 0
        float accelThisFrame = desiredMag < currentSpeed ? accel * 1.5f : accel;
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredMag, accelThisFrame * Time.deltaTime);

        Vector3 horiz = (desiredMag > 0.001f) ? desiredMove.normalized * currentSpeed : Vector3.zero;

        // --- Vertical motion ---
        bool grounded = cc.isGrounded;

        if (playerSwimming)
        {
            float targetY = playerRoot.position.y + swimYOffset;
            float newY    = Mathf.MoveTowards(transform.position.y, targetY, swimYFollowSpeed * Time.deltaTime);
            vVel = (newY - transform.position.y) / Mathf.Max(Time.deltaTime, 0.0001f);
        }
        else
        {
            if (grounded && vVel < 0f) vVel = -2f;
            vVel += gravity * Time.deltaTime;
        }

        // --- Move ---
        Vector3 motion = horiz * Time.deltaTime + Vector3.up * vVel * Time.deltaTime;
        cc.Move(motion);

        // --- Jump echo (safe) ---
        if (!string.IsNullOrEmpty(jumpTrigger)
            && prevPlayerGrounded && !playerGrounded            // player just left ground
            && grounded                                         // NPC is on ground (so jumping makes sense)
            && !playerSwimming                                  // not in water
            && distXZ <= jumpEchoMaxRange                       // close enough to care
            && jumpEchoTimer <= 0f)
        {
            anim.SetTrigger(jumpTrigger);
            jumpEchoTimer = jumpEchoCooldown;
        }
        prevPlayerGrounded = playerGrounded;

        // --- Animator drive (with tiny-speed clamp) ---
        float animSpeed = horiz.magnitude; // m/s
        if (animSpeed < 0.05f) animSpeed = 0f; // clamp tiny values to full idle
        DriveAnimator(animSpeed, grounded, playerSwimming);
    }

    void DriveAnimator(float speedMS, bool grounded, bool swimming)
    {
        if (!anim) return;
        if (!string.IsNullOrEmpty(speedParam))
            anim.SetFloat(speedParam, speedMS, 0.1f, Time.deltaTime);
        if (!string.IsNullOrEmpty(groundedParam))
            anim.SetBool(groundedParam, grounded && !swimming);
        if (!string.IsNullOrEmpty(swimmingParam))
            anim.SetBool(swimmingParam, swimming);
    }

    bool SafeGetBool(Animator a, string param)
    {
        if (a == null || string.IsNullOrEmpty(param)) return false;
        return a.GetBool(param);
    }
}
