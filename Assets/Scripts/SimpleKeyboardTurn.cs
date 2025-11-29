using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleKeyboardTurn : MonoBehaviour
{
    public float turnSpeed = 180f;

    private void Update()
    {
        float turn = 0f;

        // ONLY A and D / arrows – we never touch E at all
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            turn -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            turn += 1f;

        if (turn != 0f)
            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
    }
}