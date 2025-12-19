using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class PinchZoomRadialAxis : MonoBehaviour
{
    [Header("Axis Limits")]
    public float minRadial = 0.4f;
    public float maxRadial = 2.0f;

    [Header("Sensitivity")]
    public float pinchSensitivity = 0.0025f;

    CinemachineOrbitalFollow orbital;

    void Awake()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (Input.touchCount != 2 || orbital == null)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(t0Prev, t1Prev);
        float currDist = Vector2.Distance(t0.position, t1.position);

        float delta = currDist - prevDist;

        orbital.RadialAxis.Value -= delta * pinchSensitivity;
        orbital.RadialAxis.Value = Mathf.Clamp(orbital.RadialAxis.Value, minRadial, maxRadial);
    }
}
