using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollUV : MonoBehaviour
{
    public enum Axis { U, V, Both }
    public Axis axis = Axis.V;            // pick U or V
    public float speed = -0.08f;          // negative = move "toward camera" look
    public bool multiplyByTrackSpeed = true;

    Material mat;
    Vector2 offset;

    void Awake() { mat = GetComponent<Renderer>().material; }

    void Update()
    {
        float s = speed * Time.deltaTime;
        if (multiplyByTrackSpeed)
        {
            // If you’re not using TrackMover, replace with a constant.
            s *= (typeof(TrackMover).GetProperty("Speed") != null ? TrackMover.Speed * 0.1f : 1f);
        }

        switch (axis)
        {
            case Axis.U:     offset.x += s; break;
            case Axis.V:     offset.y += s; break;
            case Axis.Both:  offset += new Vector2(s, s); break;
        }

        mat.mainTextureOffset = offset;
    }
}
