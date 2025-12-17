using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("References")]
    public EnemyManager EnameManager;
    public ProjectileManager ProjectileManager;
    public PlayerStats stats;

    [Header("Weapon")]
    [Min(0.1f)] public float FireRate = 2f;
    public float Range = 10f;
    public float ProjectileSpeed = 14f;
    public float ProjectileLifetime = 2.5f;
    public int ProjectileDamage = 5;

    float cooldown;

    private void Awake()
    {
        if (!stats) stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!EnameManager || !ProjectileManager) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        Vector3 origin = transform.position;

        if (EnameManager.TryGetNearestEnemy(origin, Range, out var targetPos))
        {
            Vector3 dir = targetPos - origin;
            dir.y = 0f;

            if (dir.sqrMagnitude > 1e-6f)
            {
                dir.Normalize();

                ProjectileManager.Fire(origin, dir, ProjectileSpeed, ProjectileLifetime, ProjectileDamage);

                float mult = stats ? stats.fireRateMultiplier : 1f;
                float effectiveFireRate = Mathf.Max(0.1f, FireRate * mult);
                cooldown = 1f / effectiveFireRate;
            }
        }
    }
}