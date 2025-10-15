using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public void OnMove(InputValue input)
    {
        MoveInput = input.Get<Vector2>();
    }
}
