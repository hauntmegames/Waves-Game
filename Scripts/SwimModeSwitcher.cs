// SwimModeSwitcher.cs
// Add to the player root. It toggles between FirstPersonController (land)
// and UnderwaterFreeSwim (water) by watching the Animator "Swimming" bool.

using UnityEngine;
using StarterAssets; // for FirstPersonController

public class SwimModeSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public Animator playerAnimator;                // your child Animator
    public string swimmingParam = "Swimming";
    public FirstPersonController landController;   // your existing mover
    public UnderwaterFreeSwim swimController;      // the new swim script
    public MonoBehaviour lookController;           // your mouse/third-person look script (usually leave ON)

    bool lastSwimming;

    void Reset()
    {
        playerAnimator = GetComponentInChildren<Animator>();
        landController  = GetComponent<FirstPersonController>();
        swimController  = GetComponent<UnderwaterFreeSwim>();
    }

    void Awake()
    {
        if (swimController) swimController.enabled = false; // start on land
    }

    void Update()
    {
        bool swimming = playerAnimator && !string.IsNullOrEmpty(swimmingParam) && playerAnimator.GetBool(swimmingParam);
        if (swimming == lastSwimming) return;

        if (swimming)
        {
            if (landController) landController.enabled = false;
            if (swimController) swimController.enabled = true;
            // keep lookController enabled (you still aim with the same camera)
        }
        else
        {
            if (swimController) swimController.enabled = false;
            if (landController) landController.enabled = true;
        }

        lastSwimming = swimming;
    }
}
