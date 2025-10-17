using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    public Transform Player;
    public GameObject EnemyPrefab;

    [Header("Combat")]
    public int EnemyMaxHP =10;
    public float HitRadius = 0.45f;

    [Header("Enemy damade to player")]
    public float PlayerHitRadious = 0.7f;
    public int PlayerTouchDamage = 5;
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

    private readonly List<int> _hp = new();
    private readonly List<Transform> _hpFill = new();

    public void ApplyDamageAtIndex(int index, int amount)
    {
        if ((uint)index >= (uint)_hp.Count) return;

        _hp[index] -= amount;
        if (_hp[index] <= 0)
        {
            DespawnAt(index);
            return;
        }

        var fill = _hpFill[index];
        if (fill)
        {
            float frac = Mathf.Clamp01(_hp[index] / (float)EnemyMaxHP);
            var s = fill.localScale; fill.localScale = new Vector3(frac, s.y, s.z);

            var parent = fill.parent;
            if (parent && !parent.gameObject.activeSelf) parent.gameObject.SetActive(true);
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

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++) _pool.Push(CreatePooled());
    }

    public Transform SpawnAt(Vector3 position)
    {
        Transform t = _pool.Count > 0 ? _pool.Pop() : CreatePooled();
        t.gameObject.SetActive(true);
        t.SetPositionAndRotation(position, Quaternion.identity);

        _transforms.Add(t);
        _positions.Add(position);
        _rotations.Add(Quaternion.identity);
        _hp.Add(EnemyMaxHP);

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

    public void DespawnAt(int index)
    {
        var t = _transforms[index];
        t.gameObject.SetActive(false);
        _pool.Push(t);

        int last = _transforms.Count - 1;
        if (index != last)
        {
            _transforms[index] = _transforms[last];
            _positions[index] = _positions[last];
            _rotations[index] = _rotations[last];
            _hpFill[index] = _hpFill[last];
            _hp[index] = _hp[last];
        }
        _transforms.RemoveAt(last);
        _positions.RemoveAt(last);
        _rotations.RemoveAt(last);
        _hp.RemoveAt(last);
        _hpFill.RemoveAt(last);
    }

    public void DespawnAll()
    {
        for (int i = _transforms.Count - 1; i >= 0; i--) DespawnAt(i);
    }

    private readonly List<Transform> _transforms = new();
    private readonly List<Vector3> _positions = new();
    private readonly List<Quaternion> _rotations = new();
    private readonly Stack<Transform> _pool = new();

    private readonly Dictionary<(int x, int z), List<int>> _grid = new(1024);

    void Awake()
    {
        if (!Player) Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _playerHealth = Player ? Player.GetComponent<PlayerHealth>() : null;
        if (InitialEnemyPool > 0) Prewarm(InitialEnemyPool);
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

            Vector3 separationDir = Vector3.zero;
            int neighborCount = 0;

            int cellX = Mathf.FloorToInt(pos.x * invCellSize);
            int cellZ = Mathf.FloorToInt(pos.z * invCellSize);

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (!_grid.TryGetValue((cellX + dx, cellZ + dz), out var indices)) continue;
                    for (int n = 0; n < indices.Count; n++)
                    {
                        int j = indices[n];
                        if (j == i) continue;

                        Vector3 neighborPos = _positions[j];
                        float offsetX = neighborPos.x - pos.x;
                        float offsetZ = neighborPos.z - pos.z;
                        float distSqr = offsetX * offsetX + offsetZ * offsetZ;

                        if (distSqr < sepRadiusSqr && distSqr > 1e-8f)
                        {
                            float invDist = 1f / Mathf.Sqrt(distSqr);
                            float weight = 1f - (Mathf.Sqrt(distSqr) / sepRadius);
                            separationDir.x -= offsetX * invDist * weight;
                            separationDir.z -= offsetZ * invDist * weight;

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
                float dx = playerPos.x- pos.x;
                float dz = playerPos.z- pos.z;
                float distSqr = dx * dx + dz * dz;

                if (distSqr <= PlayerHitRadious * PlayerHitRadious)
                {
                    _playerHealth.TakeDamage(PlayerTouchDamage);
                }
            }
            _positions[i] = pos;
        }

        for (int i = 0; i < _transforms.Count; i++)
            _transforms[i].SetPositionAndRotation(_positions[i], _rotations[i]);
    }

    Transform CreatePooled()
    {
        var t = Instantiate(EnemyPrefab).transform;
        t.gameObject.SetActive(false);
        return t;
    }

    public bool TryGetNearestEnemy(Vector3 from, float maxRane, out Vector3 enemyPos)
    {
        enemyPos = Vector3.zero;
        if (_positions.Count == 0) return false;

        float maxRangeSqr = maxRane * maxRane;
        float bestDistSqr = maxRangeSqr;
        int bestIndex = -1;
        for (int i = 0; i<_positions.Count; i++)
        {
            Vector3 p = _positions[i];
        float distSqr = (p - from).sqrMagnitude;
            if (distSqr<bestDistSqr)
            {
                bestDistSqr = distSqr;
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
}