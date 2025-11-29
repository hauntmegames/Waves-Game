// NPCLookAtPlayer.cs
using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    public Transform target;            // player camera or head
    public float maxAngle = 80f;
    public float weight = 1f;

    Animator anim;
    void Awake(){ anim = GetComponent<Animator>(); }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim || !target) return;
        Vector3 to = (target.position - transform.position).normalized;
        float a = Vector3.Angle(transform.forward, to);
        float w = Mathf.Clamp01(1f - a / maxAngle);
        anim.SetLookAtWeight(weight * w, 0.2f, 0.9f, 1f, 0.5f);
        anim.SetLookAtPosition(target.position);
    }
}
