using UnityEngine;

public class RotateInPlace : MonoBehaviour
{
    public float rotationSpeed = 50f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
