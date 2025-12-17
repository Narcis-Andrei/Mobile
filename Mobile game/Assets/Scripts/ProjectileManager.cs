using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public int initialPoolSize = 100;

    [Header("References")]
    public EnemyManager enemyManager;
    public PlayerStats playerStats;

    private readonly List<Transform> _projectiles = new();
    private readonly List<Vector3> _positions = new();
    private readonly List<Vector3> _directions = new();
    private readonly List<float> _lifetimes = new();
    private readonly Stack<Transform> _pool = new();
    private readonly List<int> _damage = new();

    void Awake()
    {
        if (!playerStats) playerStats = FindFirstObjectByType<PlayerStats>();

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

    public void Fire(Vector3 origin, Vector3 drection, float speed, float lifetime, int damage)
    {
        Transform t = _pool.Count > 0 ? _pool.Pop() : CreateProjectile();
        t.position = origin;
        t.rotation = Quaternion.LookRotation(drection);
        t.gameObject.SetActive(true);

        _projectiles.Add(t);
        _positions.Add(origin);
        _directions.Add(drection * speed);
        _lifetimes.Add(lifetime);
        _damage.Add(damage);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            _lifetimes[i] -= dt;
            if (_lifetimes[i] <= 0f)
            {
                Despawn(i);
                continue;
            }

            _positions[i] += _directions[i] * dt;
            _projectiles[i].position = _positions[i];

            if (enemyManager && enemyManager.TryFindEnemyWithinRadius(_positions[i], enemyManager.HitRadius, out int hitIdx))
            {
                int dmg = _damage[i];

                if (playerStats && playerStats.critChance > 0f && Random.value < playerStats.critChance)
                {
                    dmg = Mathf.RoundToInt(dmg * Mathf.Max(1f, playerStats.critDamage));
                }

                enemyManager.ApplyDamageAtIndex(hitIdx, dmg);
                Despawn(i);
            }
        }
    }

    void Despawn(int i)
    {
        Transform t = _projectiles[i];
        t.gameObject.SetActive(false);
        _pool.Push(t);

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
