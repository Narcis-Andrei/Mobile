using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public EnemyManager Manager;
    public Transform Player;

    [Header("Initial Spawn")]
    public int StartCount = 20;
    public float StartSpawnRadius = 14f;

    [Header("Enemy Stat Scaling")]
    public int ExtraHPPerMinute = 2;
    public int ExtraHPQuadratic = 1;
    public int MaxHPOnSpawn = 120;
    public float MoveSpeedPerMinute = 0.15f;
    public int TouchDamagePerMinute = 1;

    [Header("Spawning")]
    public float SpawnRadius = 25f;
    public bool SpawnInCircle = true;

    [Header("Difficulty")]
    public int BaseMaxCount = 90;
    public int MaxCountPerMinute = 45;

    public float BaseEnemiesPerSecond = 1.2f;
    public float EnemiesPerSecondPerMinute = 0.9f;

    [Header("Performance")]
    public int MaxSpawnPerFrame = 24;

    float _timePassed;
    float _spawnBudget;
    int _lastMinute = -1;

    void Start()
    {
        if (!Manager) Manager = FindFirstObjectByType<EnemyManager>();
        if (!Player) Player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!Manager || !Player)
        {
            Debug.LogError("EnemySpawner: Missing references");
            enabled = false;
            return;
        }

        _timePassed = 0f;
        _spawnBudget = 0f;

        int prewarm = Mathf.Max(BaseMaxCount, StartCount);
        Manager.Prewarm(prewarm);

        if (StartCount > 0)
            SpawnNow(StartCount, StartSpawnRadius);
    }

    void Update()
    {
        if (!Manager || !Player) return;

        float dt = Time.deltaTime;
        _timePassed += dt;

        int minute = Mathf.FloorToInt(_timePassed / 60f);

        if (minute != _lastMinute)
        {
            _lastMinute = minute;

            int hp = HPOnSpawn(minute);
            Manager.CurrentEnemyHPOnSpawn = hp;

            Manager.MoveSpeed =
                Manager.MoveSpeed + minute * MoveSpeedPerMinute;

            Manager.PlayerTouchDamage =
                Manager.PlayerTouchDamage + minute * TouchDamagePerMinute;
        }

        int maxCount = BaseMaxCount + minute * MaxCountPerMinute;
        float eps = BaseEnemiesPerSecond + minute * EnemiesPerSecondPerMinute;

        if (Manager.AliveCount >= maxCount)
            return;

        _spawnBudget += eps * dt;

        int canSpawn = Mathf.Min(Mathf.FloorToInt(_spawnBudget), MaxSpawnPerFrame);
        int spaceLeft = maxCount - Manager.AliveCount;
        int toSpawn = Mathf.Clamp(canSpawn, 0, spaceLeft);

        if (toSpawn <= 0)
            return;

        SpawnNow(toSpawn, SpawnRadius);
        _spawnBudget -= toSpawn;
    }

    int HPOnSpawn(int minute)
    {
        int baseHP = Mathf.Max(1, Manager ? Manager.EnemyMaxHP : 1);

        int linear = minute * Mathf.Max(0, ExtraHPPerMinute);
        int quad = (minute * minute) * Mathf.Max(0, ExtraHPQuadratic);

        int hp = baseHP + linear + quad;

        if (MaxHPOnSpawn > 0)
            hp = Mathf.Min(hp, MaxHPOnSpawn);

        return Mathf.Max(1, hp);
    }

    void SpawnNow(int count, float radius)
    {
        if (SpawnInCircle)
            Manager.SpawnCircleAroundPlayer(count, radius);
        else
            SpawnRandom(count, radius);
    }

    void SpawnRandom(int count, float radius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 spawnPos = new Vector3(
                Player.position.x + offset.x,
                Player.position.y,
                Player.position.z + offset.y
            );

            Manager.SpawnAt(spawnPos);
        }
    }
}