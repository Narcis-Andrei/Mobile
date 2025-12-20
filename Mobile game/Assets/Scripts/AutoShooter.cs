using UnityEngine;
using System.Collections.Generic;

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

    void Awake()
    {
        if (!stats)
            stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!EnameManager || !ProjectileManager)
            return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f)
            return;

        Vector3 origin = transform.position;

        int shots = stats ? Mathf.Max(1, stats.projectileCount) : 1;

        HashSet<int> used = new HashSet<int>();

        int fired = 0;

        for (int i = 0; i < shots; i++)
        {
            if (!EnameManager.TryGetNearestEnemyWithinRange(
                    origin,
                    Range,
                    used,
                    out Vector3 targetPos,
                    out int idx))
                break;

            used.Add(idx);

            Vector3 dir = targetPos - origin;
            dir.y = 0f;

            if (dir.sqrMagnitude <= 1e-6f)
                continue;

            dir.Normalize();

            ProjectileManager.Fire(origin, dir, ProjectileSpeed, ProjectileLifetime, ProjectileDamage);
            fired++;
        }

        if (fired > 0)
        {
            float mult = stats ? stats.fireRateMultiplier : 1f;
            float effectiveFireRate = Mathf.Max(0.1f, FireRate * mult);
            cooldown = 1f / effectiveFireRate;
        }
    }
}