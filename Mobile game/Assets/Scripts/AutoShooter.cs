using UnityEngine;
using System.Collections.Generic;

// Automatically targets enemies and fires projectiles.
// Uses a cooldown timer so shooting frequency is controlled by FireRate.
public class AutoShooter : MonoBehaviour
{
    [Header("References")]
    public EnemyManager EnameManager;
    public ProjectileManager ProjectileManager;
    public PlayerStats stats;

    [Header("Weapon")]
    // Shots per second before multipliers
    [Min(0.1f)] public float FireRate = 2f;
    // Targeting radius
    public float Range = 10f;
    public float ProjectileSpeed = 14f;
    public float ProjectileLifetime = 2.5f;
    public int ProjectileDamage = 5;

    // Time until next shot is allowed
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

        // Cooldown controls automatic fire rate
        cooldown -= Time.deltaTime;
        if (cooldown > 0f)
            return;

        Vector3 origin = transform.position;

        // Number of shots scales with upgrades
        int shots = stats ? Mathf.Max(1, stats.projectileCount) : 1;

        // Prevents selecting the same enemy multiple times in one firing cycle
        HashSet<int> used = new HashSet<int>();

        int fired = 0;

        for (int i = 0; i < shots; i++)
        {
            // Finds nearest enemy within range that hasnt been used already
            if (!EnameManager.TryGetNearestEnemyWithinRange(
                    origin,
                    Range,
                    used,
                    out Vector3 targetPos,
                    out int idx))
                break;

            used.Add(idx);

            // Fire direction is flattened to XZ so bullets dont tilt up or down
            Vector3 dir = targetPos - origin;
            dir.y = 0f;

            if (dir.sqrMagnitude <= 1e-6f)
                continue;

            dir.Normalize();

            // Delegates projectile spawning to the pooled ProjectileManager
            ProjectileManager.Fire(origin, dir, ProjectileSpeed, ProjectileLifetime, ProjectileDamage);
            fired++;
        }

        if (fired > 0)
        {
            // Fire rate is scaled by upgrades
            float mult = stats ? stats.fireRateMultiplier : 1f;
            float effectiveFireRate = Mathf.Max(0.1f, FireRate * mult);
            // Convert shots a second into a cooldown delay
            cooldown = 1f / effectiveFireRate;
        }
    }
}