using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private InputManager inputs;
    private Rigidbody rb;
    private DashController dash;

    [Header("Settings for movement")]
    public float Speed = 5f;
    public float RotationSpeed = 10f;

    [Header("Caps")]
    public float MaxMoveSpeedMultiplier = 2f;

    private Vector2 moveDirection;
    private bool isMoving;

    void Awake()
    {
        inputs = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        dash = GetComponent<DashController>();

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
        if (dash != null && dash.IsDashing) return;

        Vector3 move = new Vector3(moveDirection.x, 0f, moveDirection.y);
        if (move.sqrMagnitude < 1e-6f) return;

        float speedMultiplier = 1f;
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            speedMultiplier = Mathf.Min(stats.moveSpeedMultiplier, MaxMoveSpeedMultiplier);

        rb.MovePosition(
            rb.position + move * Speed * speedMultiplier * Time.fixedDeltaTime
        );

        Rotate(move);
    }

    void Rotate(Vector3 move)
    {
        if (!isMoving || move.sqrMagnitude < 1e-6f) return;

        Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime));
    }
}
