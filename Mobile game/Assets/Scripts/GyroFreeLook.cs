using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class GyroFreeLook : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineCamera cmCam;
    public CinemachineOrbitalFollow orbital;

    [Header("Orientation")]
    public bool forceLandscape = true;
    public bool landscapeRight = true;

    [Header("Yaw Mapping")]
    public float pitchToYaw = 1f;
    public float rollToYaw = -1f;
    public bool invertPitch = false;
    public bool invertRoll = false;

    [Header("Speed & Feel")]
    public float speedPerDegree = 2.80f;
    public float deadZoneDegrees = 2.0f;
    public float smoothing = 12f;
    public float maxSpeed = 150f;

    [Header("Gestures")]
    public bool allowRecenterTap = true;
    public float gestureCooldown = 0.35f;

    bool _ready;
    bool _calibrating;

    Quaternion _neutral;
    float _smoothedSpeed;

    float _debugTimer;
    float _gestureCooldownTimer;
    bool _twoFingerArmed;
    int _prevTouchCount;

    void Awake()
    {
        if (!cmCam) cmCam = GetComponent<CinemachineCamera>();
        if (cmCam && orbital == null)
            orbital = cmCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void OnEnable()
    {
        if (!SystemInfo.supportsGyroscope)
        {
            _ready = false;
            return;
        }

        Input.gyro.enabled = true;
        _ready = true;

        ResetGestureState();
        StartCoroutine(Calibrate());
    }

    void OnDisable()
    {
        ResetGestureState();
    }

    void ResetGestureState()
    {
        _twoFingerArmed = false;
        _gestureCooldownTimer = 0f;
        _prevTouchCount = 0;
    }

    void Update()
    {
        if (!_ready || orbital == null) return;

        UpdateTouchGestures();
        if (_calibrating) return;

        Quaternion current = GetMappedDeviceRotation();
        Quaternion rel = current * Quaternion.Inverse(_neutral);

        Vector3 euler = rel.eulerAngles;
        float pitch = NormaliseSignedAngle(euler.x);
        float roll = NormaliseSignedAngle(euler.z);

        if (invertPitch) pitch = -pitch;
        if (invertRoll) roll = -roll;

        float pitchEff = Mathf.Abs(pitch) < deadZoneDegrees ? 0f : pitch;
        float rollEff = Mathf.Abs(roll) < deadZoneDegrees ? 0f : roll;

        float combined = (pitchEff * pitchToYaw) + (rollEff * rollToYaw);
        float targetSpeed = Mathf.Clamp(combined * speedPerDegree, -maxSpeed, maxSpeed);

        _smoothedSpeed = Mathf.Lerp(
            _smoothedSpeed,
            targetSpeed,
            1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime)
        );

        orbital.HorizontalAxis.Value += _smoothedSpeed * Time.unscaledDeltaTime;
    }

    void UpdateTouchGestures()
    {
        if (_gestureCooldownTimer > 0f)
            _gestureCooldownTimer -= Time.unscaledDeltaTime;

        int tc = Input.touchCount;

        if (tc < 2) _twoFingerArmed = true;

        if (allowRecenterTap && tc == 2 && _twoFingerArmed && _gestureCooldownTimer <= 0f)
        {
            if (AnyTouchBegan() || JustReachedTouchCount(2))
            {
                _twoFingerArmed = false;
                _gestureCooldownTimer = gestureCooldown;
                Recenter();
            }
        }

        _prevTouchCount = tc;
    }

    bool JustReachedTouchCount(int target) => _prevTouchCount != target && Input.touchCount == target;

    bool AnyTouchBegan()
    {
        for (int i = 0; i < Input.touchCount; i++)
            if (Input.GetTouch(i).phase == TouchPhase.Began)
                return true;
        return false;
    }

    public void Recenter()
    {
        if (!_ready) return;
        StopAllCoroutines();
        StartCoroutine(Calibrate());
    }

    IEnumerator Calibrate()
    {
        _calibrating = true;
        yield return new WaitForSecondsRealtime(0.15f);

        Quaternion avg = GetMappedDeviceRotation();
        const int samples = 20;

        for (int i = 1; i < samples; i++)
        {
            yield return null;
            Quaternion q = GetMappedDeviceRotation();
            float t = 1f / (i + 1f);
            avg = Quaternion.Slerp(avg, q, t);
        }

        _neutral = avg;
        _smoothedSpeed = 0f;
        _calibrating = false;
    }

    Quaternion GetMappedDeviceRotation()
    {
        Quaternion q = Input.gyro.attitude;
        Quaternion unityQ = new Quaternion(q.x, q.y, -q.z, -q.w);
        Quaternion rot = Quaternion.Euler(90f, 0f, 0f) * unityQ;

        if (forceLandscape)
        {
            rot = (landscapeRight ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.Euler(0f, 0f, 90f)) * rot;
        }

        return rot;
    }

    static float NormaliseSignedAngle(float degrees)
    {
        degrees %= 360f;
        if (degrees > 180f) degrees -= 360f;
        if (degrees < -180f) degrees += 360f;
        return degrees;
    }
}
