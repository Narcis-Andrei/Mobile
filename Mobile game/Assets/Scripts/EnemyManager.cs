using System.Collections.Generic;
using UnityEngine;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
using CandyCoded.HapticFeedback;
#endif

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    public Transform Player;
    public GameObject EnemyPrefab;
    public StatsTracker runStats;

    [Header("Combat")]
    public int EnemyMaxHP = 10;
    public float HitRadius = 0.45f;

    [Header("Enemy damage to player")]
    public float PlayerHitRadious = 0.7f;
    public int PlayerTouchDamage = 5;
    public float TouchDamageCooldown = 0.25f;
    private PlayerHealth _playerHealth;

    [Header("Pool")]
    [Min(0)] public int InitialEnemyPool = 0;

    [Header("Spawn Radius")]
    public float DefaultSpawnRadius = 20f;

    [Header("EnemyStats")]
    public float MoveSpeed = 4.5f;
    public float TurnDegPerSec = 360f;

    [Header("Avoid other Enemy")]
    public float SeparationRadius = 1.2f;
    public float SeparationStrength = 2.5f;
    public int MaxNeighbors = 6;
    public float PlayerPersonalSpace = 0.6f;

    [Header("Perf")]
    public float GridCellSize = 1.0f;
    public float CullingDistance = 60f;

    public int AliveCount => _transforms.Count;

    public int CurrentEnemyHPOnSpawn { get; set; }

    private readonly List<int> _maxHp = new();
    private readonly List<int> _hp = new();
    private readonly List<Transform> _hpFill = new();
    private readonly List<float> _nextTouchDamageTime = new();

    // Active enemy data is stored in parallel lists instead of individual scripts
    // This avoids having many Update() calls and improves mobile performance
    private readonly List<Transform> _transforms = new();
    private readonly List<Vector3> _positions = new();
    private readonly List<Quaternion> _rotations = new();

    // Object pool used to recycle enemy instances instead of instantiating at runtime.
    // This reduces CPU spikes.
    private readonly Stack<Transform> _pool = new();

    // Spatial partitioning grid used to limit neighbour checks
    // This prevents lag with large enemy counts.
    private readonly Dictionary<(int x, int z), List<int>> _grid = new(1024);

    void Awake()
    {
        if (!Player) Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _playerHealth = Player ? Player.GetComponent<PlayerHealth>() : null;
        if (InitialEnemyPool > 0) Prewarm(InitialEnemyPool);
        if (!runStats) runStats = FindFirstObjectByType<StatsTracker>();
        CurrentEnemyHPOnSpawn = EnemyMaxHP;
    }

    public void ApplyDamageAtIndex(int index, int amount)
    {
        if ((uint)index >= (uint)_hp.Count) return;
        if (_hp[index] <= 0) return;

        _hp[index] -= amount;

        if (_hp[index] <= 0)
        {
            runStats?.AddKill(1);
            DespawnAt(index);
            return;
        }

        var fill = _hpFill[index];
        if (fill)
        {
            float frac = Mathf.Clamp01(_hp[index] / (float)Mathf.Max(1, _maxHp[index]));
            var s = fill.localScale;
            fill.localScale = new Vector3(frac, s.y, s.z);

            var parent = fill.parent;
            if (parent && !parent.gameObject.activeSelf)
                parent.gameObject.SetActive(true);
        }
    }

    public bool TryFindEnemyWithinRadius(Vector3 from, float radius, out int hitIndex)
    {
        hitIndex = -1;
        if (_positions.Count == 0) return false;
        float r2 = radius * radius;

        for (int i = 0; i < _positions.Count; i++)
        {
            var p = _positions[i];
            float dx = p.x - from.x, dz = p.z - from.z;
            float d2 = dx * dx + dz * dz;
            if (d2 <= r2) { hitIndex = i; return true; }
        }
        return false;
    }

    // Pre create enemy objects at the start of the game to avoid instantiating during gameplay
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++) _pool.Push(CreatePooled());
    }

    // Spawns an enemy using pooled instances
    public Transform SpawnAt(Vector3 position)
    {
        Transform t = _pool.Count > 0 ? _pool.Pop() : CreatePooled();
        t.gameObject.SetActive(true);
        t.SetPositionAndRotation(position, Quaternion.identity);

        // Store enemy state in central lists
        _transforms.Add(t);
        _positions.Add(position);
        _rotations.Add(Quaternion.identity);

        int hpOnSpawn = Mathf.Max(1, CurrentEnemyHPOnSpawn > 0 ? CurrentEnemyHPOnSpawn : EnemyMaxHP);
        _hp.Add(hpOnSpawn);
        _maxHp.Add(hpOnSpawn);
        _nextTouchDamageTime.Add(0f);

        Transform fill = t.Find("HealthBar/EnemyHp");
        _hpFill.Add(fill);

        if (fill)
        {
            var s = fill.localScale; fill.localScale = new Vector3(1f, s.y, s.z);
            var parent = fill.parent;
            if (parent && parent.gameObject.activeSelf) parent.gameObject.SetActive(false);
        }

        return t;
    }

    public void SpawnCircleAroundPlayer(int count, float radius = -1f)
    {
        if (!Player) return;
        if (radius <= 0f) radius = DefaultSpawnRadius;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            var pos = Player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            SpawnAt(pos);
        }
    }

    // Removes an enemy and returns it to the pool
    public void DespawnAt(int index)
    {
        var t = _transforms[index];
        t.gameObject.SetActive(false);
        _pool.Push(t);

        // Swap-with-last removal to avoid shifting lists
        int last = _transforms.Count - 1;
        if (index != last)
        {
            _transforms[index] = _transforms[last];
            _positions[index] = _positions[last];
            _rotations[index] = _rotations[last];
            _hpFill[index] = _hpFill[last];
            _hp[index] = _hp[last];
            _maxHp[index] = _maxHp[last];
            _nextTouchDamageTime[index] = _nextTouchDamageTime[last];
        }

        _transforms.RemoveAt(last);
        _positions.RemoveAt(last);
        _rotations.RemoveAt(last);
        _hp.RemoveAt(last);
        _maxHp.RemoveAt(last);
        _hpFill.RemoveAt(last);
        _nextTouchDamageTime.RemoveAt(last);
    }

    public void DespawnAll()
    {
        for (int i = _transforms.Count - 1; i >= 0; i--) DespawnAt(i);
    }

    void Update()
    {
        if (!Player || _transforms.Count == 0) return;

        _grid.Clear();
        float invCellSize = 1f / Mathf.Max(0.1f, GridCellSize);
        for (int i = 0; i < _positions.Count; i++)
        {
            Vector3 pos = _positions[i];
            var key = (Mathf.FloorToInt(pos.x * invCellSize), Mathf.FloorToInt(pos.z * invCellSize));
            if (!_grid.TryGetValue(key, out var indices)) _grid[key] = indices = new List<int>(8);
            indices.Add(i);
        }

        float dt = Time.deltaTime;
        Vector3 playerPos = Player.position;
        float moveStep = MoveSpeed * dt;
        float turnStep = TurnDegPerSec * dt;
        float cullDistSqr = CullingDistance * CullingDistance;

        float sepRadius = Mathf.Max(0.01f, SeparationRadius);
        float sepRadiusSqr = sepRadius * sepRadius;

        float touchR2 = PlayerHitRadious * PlayerHitRadious;
        float now = Time.time;

        for (int i = 0; i < _positions.Count; i++)
        {
            Vector3 pos = _positions[i];

            Vector3 toPlayerXZ = playerPos - pos; toPlayerXZ.y = 0f;
            if (toPlayerXZ.sqrMagnitude > cullDistSqr) continue;

            Vector3 desireDir = Vector3.zero;
            float toPlayerSqr = toPlayerXZ.sqrMagnitude;
            if (toPlayerSqr > 1e-8f)
            {
                float toPlayerLen = Mathf.Sqrt(toPlayerSqr);
                if (toPlayerLen > PlayerPersonalSpace) desireDir = toPlayerXZ / toPlayerLen;
                else desireDir = -(toPlayerXZ / Mathf.Max(0.001f, toPlayerLen));
            }

            // Separation force pushes enemies away from nearby neighbours
            Vector3 separationDir = Vector3.zero;
            int neighborCount = 0;

            // Determine which grid cell the enemy is
            int cellX = Mathf.FloorToInt(pos.x * invCellSize);
            int cellZ = Mathf.FloorToInt(pos.z * invCellSize);

            // Check this cell and the 8 surrounding cells
            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    // Skip if the cell has no neighbour
                    if (!_grid.TryGetValue((cellX + dx, cellZ + dz), out var indices)) continue;
                    // Iterate through agents in the neighboring cell
                    for (int n = 0; n < indices.Count; n++)
                    {
                        int j = indices[n];
                        if (j == i) continue;

                        Vector3 neighborPos = _positions[j];
                        float offsetX = neighborPos.x - pos.x;
                        float offsetZ = neighborPos.z - pos.z;
                        float distSqr = offsetX * offsetX + offsetZ * offsetZ;

                        // Only apply separation if within radius and not overlapping
                        if (distSqr < sepRadiusSqr && distSqr > 1e-8f)
                        {
                            float invDist = 1f / Mathf.Sqrt(distSqr);
                            float dist = Mathf.Sqrt(distSqr);
                            float weight = 1f - (dist / sepRadius);
                            // Push away from neighbor
                            separationDir.x -= offsetX * invDist * weight;
                            separationDir.z -= offsetZ * invDist * weight;

                            // Stop early if max neighbors reached
                            if (++neighborCount >= MaxNeighbors) break;
                        }
                    }
                    if (neighborCount >= MaxNeighbors) break;
                }
                if (neighborCount >= MaxNeighbors) break;
            }

            Vector3 moveDir = desireDir + separationDir * SeparationStrength;
            moveDir.y = 0f;

            if (moveDir.sqrMagnitude > 1e-8f)
            {
                moveDir.Normalize();
                pos += moveDir * moveStep;

                var targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                _rotations[i] = (TurnDegPerSec <= 0f)
                    ? targetRot
                    : Quaternion.RotateTowards(_rotations[i], targetRot, turnStep);
            }

            if (_playerHealth && _playerHealth.CanTakeDamage)
            {
                float dx = playerPos.x - pos.x;
                float dz = playerPos.z - pos.z;
                float distSqr = dx * dx + dz * dz;

                // Applies contact damage with a cooldown
                if (distSqr <= touchR2 && now >= _nextTouchDamageTime[i])
                {
                    _nextTouchDamageTime[i] = now + TouchDamageCooldown;
                    _playerHealth.TakeDamage(PlayerTouchDamage);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                    HapticFeedback.LightFeedback();
#endif
                }
            }

            _positions[i] = pos;
        }

        for (int i = 0; i < _transforms.Count; i++)
            _transforms[i].SetPositionAndRotation(_positions[i], _rotations[i]);
    }

    // Creates a pooled enemy instance in a disabled state
    Transform CreatePooled()
    {
        var t = Instantiate(EnemyPrefab).transform;
        t.gameObject.SetActive(false);
        return t;
    }

    public bool TryGetNearestEnemy(Vector3 from, float maxRange, out Vector3 enemyPos)
    {
        enemyPos = Vector3.zero;
        if (_positions.Count == 0) return false;

        float maxRangeSqr = maxRange * maxRange;
        float bestDistSqr = maxRangeSqr;
        int bestIndex = -1;

        for (int i = 0; i < _positions.Count; i++)
        {
            Vector3 p = _positions[i];
            float dx = p.x - from.x;
            float dz = p.z - from.z;
            float d2 = dx * dx + dz * dz;

            if (d2 < bestDistSqr)
            {
                bestDistSqr = d2;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
        {
            enemyPos = _positions[bestIndex];
            return true;
        }

        return false;
    }

    public bool TryGetNearestEnemyWithinRange(
        Vector3 from,
        float maxRange,
        HashSet<int> exclude,
        out Vector3 enemyPos,
        out int enemyIndex
    )
    {
        enemyPos = Vector3.zero;
        enemyIndex = -1;

        if (_positions.Count == 0)
            return false;

        float maxRangeSqr = maxRange * maxRange;
        float bestDistSqr = maxRangeSqr;

        for (int i = 0; i < _positions.Count; i++)
        {
            if (exclude != null && exclude.Contains(i))
                continue;

            Vector3 p = _positions[i];
            float dx = p.x - from.x;
            float dz = p.z - from.z;
            float d2 = dx * dx + dz * dz;

            if (d2 < bestDistSqr)
            {
                bestDistSqr = d2;
                enemyIndex = i;
                enemyPos = p;
            }
        }

        return enemyIndex >= 0;
    }

    public bool TryGetRandomEnemyWithinRange(
        Vector3 from,
        float maxRange,
        HashSet<int> exclude,
        out Vector3 enemyPos,
        out int enemyIndex
    )
    {
        enemyPos = Vector3.zero;
        enemyIndex = -1;

        if (_positions.Count == 0)
            return false;

        float maxRangeSqr = maxRange * maxRange;

        int picked = -1;
        int seen = 0;

        for (int i = 0; i < _positions.Count; i++)
        {
            if (exclude != null && exclude.Contains(i))
                continue;

            Vector3 p = _positions[i];
            float dx = p.x - from.x;
            float dz = p.z - from.z;
            float d2 = dx * dx + dz * dz;

            if (d2 > maxRangeSqr)
                continue;

            seen++;
            if (Random.Range(0, seen) == 0)
                picked = i;
        }

        if (picked < 0)
            return false;

        enemyIndex = picked;
        enemyPos = _positions[picked];
        return true;
    }
}