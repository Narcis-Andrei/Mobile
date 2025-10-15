using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public InputManager inputManager;
    public Vector3 baseCamCord = new Vector3(0, 5f, -4f);
    public float followSpeed = 5f;

    public Vector3 camRotation = new Vector3(45f,0f,0f);

    public float moveInfluence = 2f;
    public float offsetLearpSpeed = 5f;

    private Vector3 currentOffset;

    private void Start()
    {
        currentOffset = baseCamCord;    
    }

    void LateUpdate()
    {
        Vector2 moveInput = inputManager ? inputManager.MoveInput : Vector2.zero;

        Vector3 DynamicOffset = baseCamCord + new Vector3(moveInput.x, 0f, -Mathf.Abs(moveInput.y)) * moveInfluence;

        currentOffset = Vector3.Lerp(currentOffset, DynamicOffset, offsetLearpSpeed * Time.deltaTime);

        Vector3 desiredPos = target.position + currentOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed* Time.deltaTime);
        transform.rotation = Quaternion.Euler(camRotation);
    }
}
