using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private InputManager inputs;
    private Rigidbody rb;

    [Header("Movement Settings")]
    public float Speed = 5f;
    public float RotationSpeed = 10f;

    private Vector2 moveDirection;
    private bool isMoving;

    void Awake()
    {
        inputs = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        moveDirection = inputs.MoveInput;
        if (moveDirection.sqrMagnitude < 0.04f)
            moveDirection = Vector2.zero;
        else moveDirection.Normalize();

        isMoving = moveDirection.sqrMagnitude > 0.001f;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 move = new Vector3(moveDirection.x, 0f, moveDirection.y);

        if (move.sqrMagnitude < 1e-6f) return;

        rb.MovePosition(rb.position + move * Speed * Time.fixedDeltaTime);

        Rotate(move);
    }

    void Rotate(Vector3 move)
    {
        if (!isMoving || move.sqrMagnitude < 1e-6f) return;

        Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime));
    }
}
