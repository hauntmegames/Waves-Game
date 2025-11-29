using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleTurn : MonoBehaviour
{
    public float turnSpeed = 150f;

    private float turnInput;

    // This is called automatically by the new Input System
    public void OnTempTurn(InputValue value)
    {
        turnInput = value.Get<float>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
    }
}