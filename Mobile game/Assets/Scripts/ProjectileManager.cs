using System.Collections.Generic;
using UnityEngine;

// Pooled projectile system which stores projectile state in lists and updates them centrally for performance.
public class ProjectileManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public int initialPoolSize = 250;

    [Header("References")]
    public EnemyManager enemyManager;
    public PlayerStats playerStats;

    // Parallel lists store projectile state by index
    private readonly List<Transform> _projectiles = new();
    private readonly List<Vector3> _positions = new();
    private readonly List<Vector3> _directions = new();
    private readonly List<float> _lifetimes = new();
    private readonly Stack<Transform> _pool = new();
    private readonly List<int> _damage = new();

    void Awake()
    {
        if (!playerStats) playerStats = FindFirstObjectByType<PlayerStats>();

        // Prewarm projectile pool
        for (int i = 0; i < initialPoolSize; i++)
            _pool.Push(CreateProjectile());
    }

    Transform CreateProjectile()
    {
        var t = Instantiate(projectilePrefab).transform;
        t.gameObject.SetActive(false);
        t.SetParent(transform,false);
        return t;
    }

    // Fires a projectile using pooled instances
    public void Fire(Vector3 origin, Vector3 drection, float speed, float lifetime, int damage)
    {
        Transform t = _pool.Count > 0 ? _pool.Pop() : CreateProjectile();
        t.position = origin;
        t.rotation = Quaternion.LookRotation(drection);
        t.gameObject.SetActive(true);

        // Store projectile state in parallel lists
        _projectiles.Add(t);
        _positions.Add(origin);
        _directions.Add(drection * speed);
        _lifetimes.Add(lifetime);
        _damage.Add(damage);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Iterate backwards so despawning doesnt break indexes
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            // Lifetime countdown
            _lifetimes[i] -= dt;
            if (_lifetimes[i] <= 0f)
            {
                Despawn(i);
                continue;
            }

            // Move projectile using stored direction * speed
            _positions[i] += _directions[i] * dt;
            _projectiles[i].position = _positions[i];

            // Hit detection against enemies
            if (enemyManager && enemyManager.TryFindEnemyWithinRadius(_positions[i], enemyManager.HitRadius, out int hitIdx))
            {
                int dmg = _damage[i];

                // Critical hit logic uses player stats
                if (playerStats && playerStats.critChance > 0f && Random.value < playerStats.critChance)
                {
                    dmg = Mathf.RoundToInt(dmg * Mathf.Max(1f, playerStats.critDamage));
                }

                enemyManager.ApplyDamageAtIndex(hitIdx, dmg);
                Despawn(i);
            }
        }
    }

    // Returns projectile to pool and removes it from active lists
    void Despawn(int i)
    {
        Transform t = _projectiles[i];
        t.gameObject.SetActive(false);
        _pool.Push(t);

        // Swap with the last removal and avoids shifting elements in lists
        int last = _projectiles.Count - 1;
        if (i != last)
        {
            _projectiles[i] = _projectiles[last];
            _positions[i] = _positions[last];
            _directions[i] = _directions[last];
            _lifetimes[i] = _lifetimes[last];
            _damage[i] = _damage[last];
        }
        _projectiles.RemoveAt(last);
        _positions.RemoveAt(last);
        _directions.RemoveAt(last);
        _lifetimes.RemoveAt(last);
        _damage.RemoveAt(last);
    }
}
