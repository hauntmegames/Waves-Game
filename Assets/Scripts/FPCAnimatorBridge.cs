// FPCAnimatorBridge.cs  (on the CHILD with the Animator)
using UnityEngine;
using StarterAssets;

public class FPCAnimatorBridge : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;                     // child Animator
    public CharacterController controller;        // root CharacterController
    public FirstPersonController fpc;             // root FPC (for Grounded)
    public StarterAssetsInputs inputs;            // root StarterAssetsInputs

    [Header("Params")]
    public string speedParam = "Speed";           // float
    public string groundedParam = "Grounded";     // bool
    public string jumpTrigger = "Jump";           // trigger
    public float dampTime = 0.1f;

    bool prevJump;

    void Reset() {
        animator  = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
        fpc        = GetComponentInParent<FirstPersonController>();
        inputs     = GetComponentInParent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (!animator || !controller) return;

        // Speed (horizontal m/s)
        var v = controller.velocity; v.y = 0f;
        animator.SetFloat(speedParam, v.magnitude, dampTime, Time.deltaTime);

        // Grounded flag (optional but useful for landing)
        if (fpc && !string.IsNullOrEmpty(groundedParam))
            animator.SetBool(groundedParam, fpc.Grounded);

        // Jump trigger on *press* while grounded
        if (inputs && fpc && fpc.Grounded)
        {
            bool pressedThisFrame = inputs.jump && !prevJump;
            if (pressedThisFrame && !string.IsNullOrEmpty(jumpTrigger))
                animator.SetTrigger(jumpTrigger);
            prevJump = inputs.jump;
        }
        else if (inputs)
        {
            prevJump = inputs.jump;
        }
    }
}
