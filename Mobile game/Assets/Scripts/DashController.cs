using UnityEngine;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public bool IsDashing => _isDashing;

    [Header("Visuals")]
    public ParticleSystem dashEffect;
    public AudioSource dashSound;

    private Rigidbody _rb;
    private bool _isDashing = false;
    private float _cooldownTimer = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public void DoDash(Vector3 worldDirection)
    {
        if (_isDashing || _cooldownTimer > 0f)
            return;

        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.01f) return;
        worldDirection.Normalize();

        StartCoroutine(DashRoutine(worldDirection));

        _cooldownTimer = dashCooldown;

        if (dashEffect) dashEffect.Play();
        if (dashSound) dashSound.Play();
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 dir)
    {
        _isDashing = true;
        float elapsed = 0f;

        bool hadGravity = _rb.useGravity;
        _rb.useGravity = false;

        var wait = new WaitForFixedUpdate();

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);
            float curve = speedCurve != null ? speedCurve.Evaluate(t) : 1f;

            Vector3 step = dir * (dashSpeed * curve * Time.fixedDeltaTime);
            _rb.MovePosition(_rb.position + step);
            yield return null;
        }

        _rb.useGravity = hadGravity;
        _isDashing = false;
    }
}