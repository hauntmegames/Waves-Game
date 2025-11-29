// SwimStateRelay.cs
using UnityEngine;

public class SwimStateRelay : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;                  // child Animator with your controller
    public Transform probe;                    // where to sample (head or chest)
    
    [Header("Water Detection")]
    public LayerMask waterLayer;               // set to your Water layer
    public float probeRadius = 0.4f;           // size of the overlap sphere
    public QueryTriggerInteraction triggers = QueryTriggerInteraction.Collide;

    [Header("Animator Param")]
    public string swimmingParam = "Swimming";

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        probe = transform; // default at root; you can assign camera/head later
    }

    void Update()
    {
        if (!animator) return;

        Vector3 p = probe ? probe.position : transform.position;
        bool inWater = Physics.CheckSphere(p, probeRadius, waterLayer, triggers);

        animator.SetBool(swimmingParam, inWater);
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        Gizmos.color = Color.cyan;
        Vector3 p = probe ? probe.position : transform.position;
        Gizmos.DrawWireSphere(p, probeRadius);
    }
}
