using UnityEngine;
using UnityEngine.Events;

public enum FlickDir { None, Left, Right, Up, Down }

public class AccelormeterFlick : MonoBehaviour
{
    [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }
    [System.Serializable] public class FlickEvent : UnityEvent<FlickDir> { }

    [Header("Output")]
    public Vector3Event onDash;
    public FlickEvent onFlick;

    [Header("Reference")]
    public Camera referenceCamera;

    [Header("Accelerometer Flick")]
    public bool enablePhoneFlick = true;

    public float accelerationTreshhold = 1.3f;
    public float acceleratioCooldown = 0.7f;
    [Range(0f, 1f)] public float accelerationSmoothing = 0.1f;
    [Range(0f, 1f)] public float accelerationDirectionalBias = 0.25f;
    public bool detectWhenPause = true;

    public bool enableSwipe = true;
    public float minSwipePix = 80f;
    public float maxSwipeTime = 0.35f;

    // acceleration
    bool _gyro;
    Vector3 _lowPass;
    float _sinceAccel;

    // swipe
    bool _swiping;
    Vector2 _startPos;
    float _startTime;

    void Awake()
    {
        if (!referenceCamera) referenceCamera = Camera.main;
        _gyro = SystemInfo.supportsGyroscope;
        if (_gyro) Input.gyro.enabled = true;
        _lowPass = Input.acceleration;
    }

    void Update()
    {
        if (enablePhoneFlick) UpdateAccelerometer();
        if (enableSwipe) UpdateSwipe();
    }

    void UpdateAccelerometer()
    {
        float dt = detectWhenPause ? Mathf.Max(Time.unscaledDeltaTime, 1e-4f)
                                    : Mathf.Max(Time.deltaTime, 1e-4f);
        _sinceAccel += dt;

        Vector3 a = GetWorldLinearAcceleration(dt);
        if (accelerationSmoothing > 0f) a = Vector3.Lerp(a, Vector3.zero, accelerationSmoothing);

        Transform cam = referenceCamera ? referenceCamera.transform : null;
        Vector3 right = cam ? cam.right : Vector3.right;
        Vector3 fwd = cam ? cam.forward : Vector3.forward;
        right.y = 0f; fwd.y = 0f; right.Normalize(); fwd.Normalize();

        float x = Vector3.Dot(a, right);
        float z = Vector3.Dot(a, fwd);
        float ax = Mathf.Abs(x), az = Mathf.Abs(z);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.D)) x = accelerationTreshhold + 0.2f;
        if (Input.GetKeyDown(KeyCode.A)) x = -accelerationTreshhold - 0.2f;
        if (Input.GetKeyDown(KeyCode.W)) z = accelerationTreshhold + 0.2f;
        if (Input.GetKeyDown(KeyCode.S)) z = -accelerationTreshhold - 0.2f;
        ax = Mathf.Abs(x); az = Mathf.Abs(z);
#endif

        if (_sinceAccel < acceleratioCooldown) return;
        if (ax < accelerationTreshhold && az < accelerationTreshhold) return;

        if (ax > az * (1f + accelerationDirectionalBias))
        {
            Emit(x > 0f ? FlickDir.Right : FlickDir.Left, x > 0f ? right : -right);
        }
        else if (az > ax * (1f + accelerationDirectionalBias))
        {
            Emit(z > 0f ? FlickDir.Up : FlickDir.Down, z > 0f ? fwd : -fwd);
        }
        // else too diagonal -> ignore
    }

    Vector3 GetWorldLinearAcceleration(float dt)
    {
        if (_gyro)
        {
            Vector3 dev = Input.gyro.userAcceleration;

            Quaternion q = Input.gyro.attitude;
            Quaternion deviceToUnity = new Quaternion(q.x, q.y, -q.z, -q.w);
            return deviceToUnity * dev;
        }

        Vector3 raw = Input.acceleration;
        float hp = Mathf.Exp(-dt * 4.5f);
        _lowPass = Vector3.Lerp(raw, _lowPass, hp);
        return raw - _lowPass;
    }

    void UpdateSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                _swiping = true;
                _startPos = t.position;
                _startTime = Time.unscaledTime;
            }
            else if (_swiping && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
            {
                _swiping = false;
                EvaluateSwipe(_startPos, t.position, Time.unscaledTime - _startTime);
            }
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            _swiping = true; _startPos = Input.mousePosition; _startTime = Time.unscaledTime;
        }
        else if (_swiping && Input.GetMouseButtonUp(0))
        {
            _swiping = false;
            EvaluateSwipe(_startPos, (Vector2)Input.mousePosition, Time.unscaledTime - _startTime);
        }
#endif
    }

    void EvaluateSwipe(Vector2 start, Vector2 end, float dt)
    {
        if (dt > maxSwipeTime) return;
        Vector2 delta = end - start;
        if (delta.sqrMagnitude < minSwipePix * minSwipePix) return;

        Vector2 nd = delta.normalized;
        FlickDir dir;
        Vector3 world;

        Transform cam = referenceCamera ? referenceCamera.transform : null;
        Vector3 right = cam ? cam.right : Vector3.right;
        Vector3 fwd = cam ? cam.forward : Vector3.forward;
        right.y = 0f; fwd.y = 0f; right.Normalize(); fwd.Normalize();

        if (Mathf.Abs(nd.x) > Mathf.Abs(nd.y))
        {
            dir = nd.x > 0f ? FlickDir.Right : FlickDir.Left;
            world = nd.x > 0f ? right : -right;
        }
        else
        {
            dir = nd.y > 0f ? FlickDir.Up : FlickDir.Down;
            world = nd.y > 0f ? fwd : -fwd;
        }

        Emit(dir, world);
    }

    void Emit(FlickDir dir, Vector3 worldDash)
    {
        _sinceAccel = 0f;
        onFlick?.Invoke(dir);
        if (worldDash.sqrMagnitude > 1e-6f)
        {
            worldDash.y = 0f;
            onDash?.Invoke(worldDash.normalized);
        }
    }
} 