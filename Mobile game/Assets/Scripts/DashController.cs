using UnityEngine;
using CandyCoded.HapticFeedback;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.1f;
    public float dashDistance = 8f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public bool IsDashing => _isDashing;
    public int CurrentCharges => _charges;

    [Header("Visuals")]
    public ParticleSystem dashEffect;
    public AudioSource dashSound;

    private Rigidbody _rb;
    private bool _isDashing = false;
    private float _cooldownTimer = 0f;
    private PlayerStats _stats;
    int _charges;
    float _rechargeTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _stats = GetComponent<PlayerStats>();
        int maxCharges = _stats ? Mathf.Max(1, _stats.maxDashCharges) : 1;
        _charges = Mathf.Clamp(1, 1, maxCharges);
        _rechargeTimer = 0f;
    }

    void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (_stats == null) return;

        int maxCharges = Mathf.Max(1, _stats.maxDashCharges);
        if (_charges >= maxCharges) return;

        _rechargeTimer -= Time.deltaTime;
        if (_rechargeTimer <= 0f)
        {
            _charges++;
            _charges = Mathf.Min(_charges, maxCharges);

            if (_charges < maxCharges)
                _rechargeTimer = Mathf.Max(0.1f, _stats.dashRechargeTime);
        }
    }

    public void DoDash(Vector3 worldDirection)
    {
        if (_isDashing || _cooldownTimer > 0f || Time.timeScale == 0f)
            return;

        if (_stats != null && _charges <= 0)
            return;

        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.01f) return;

        const float minPower = 0.5f;
        const float maxPower = 2.0f;

        float power = Mathf.Clamp(worldDirection.magnitude, minPower, maxPower);
        Vector3 dir = worldDirection.normalized;

        if (_stats != null)
        {
            int maxCharges = Mathf.Max(1, _stats.maxDashCharges);

            _charges = Mathf.Max(0, _charges - 1);

            if (_charges < maxCharges && _rechargeTimer <= 0f)
                _rechargeTimer = Mathf.Max(0.1f, _stats.dashRechargeTime);
        }

        StartCoroutine(DashRoutine(dir, power));

        float cdMult = _stats ? _stats.dashCooldownMultiplier : 1f;
        _cooldownTimer = Mathf.Max(0.01f, dashCooldown * cdMult);

        HapticFeedback.MediumFeedback();
        if (dashEffect) dashEffect.Play();
        if (dashSound) dashSound.Play();
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 dir, float power)
    {
        _isDashing = true;
        float elapsed = 0f;

        bool hadGravity = _rb.useGravity;
        _rb.useGravity = false;

        var wait = new WaitForFixedUpdate();

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);
            float curve = speedCurve != null ? speedCurve.Evaluate(t) : 1f;

            Vector3 step = dir * (dashSpeed * power * curve * Time.fixedDeltaTime);
            _rb.MovePosition(_rb.position + step);
            yield return wait;
        }

        _rb.useGravity = hadGravity;
        _isDashing = false;
    }
}