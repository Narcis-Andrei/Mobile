using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public EnemyManager Manager;
    public Transform Player;

    [Header("Spawn Settings")]
    public int StartCount = 20;
    public int MaxCount = 100;
    public float EnemiesPerSecond = 2f;
    public float SpawnRadius = 25f;
    public bool SpawnInCircle = true;

    [Header("Bursts")]
    public int BurstAmount = 0;
    public float BurstInterval = 0f;

    [Header("Performance")]
    public int MaxSpawnPerFrame = 32;

    float timePassed;
    float nextBurstTime;
    int targetCount;

    void Start()
    {
        if (!Manager) Manager = FindObjectOfType<EnemyManager>();
        if (!Player) Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!Manager || !Player)
        {
            Debug.LogError("EnemySpawner: Missing references");
            enabled = false;
            return;
        }

        timePassed = 0f;
        nextBurstTime = BurstInterval > 0f ? BurstInterval : Mathf.Infinity;

        Manager.Prewarm(MaxCount);
        Manager.SpawnCircleAroundPlayer(StartCount, SpawnRadius);
        targetCount = StartCount;
    }

    void Update()
    {
        if (!Manager || !Player) return;

        timePassed += Time.deltaTime;
        int desiredCount = Mathf.Min(StartCount + Mathf.RoundToInt(EnemiesPerSecond * timePassed), MaxCount);

        if (BurstAmount > 0 && BurstInterval > 0f && timePassed >= nextBurstTime)
        {
            desiredCount = Mathf.Min(desiredCount + BurstAmount, MaxCount);
            nextBurstTime += BurstInterval;
        }

        int toSpawn = Mathf.Clamp(desiredCount - Manager.AliveCount, 0, MaxSpawnPerFrame);
        if (toSpawn > 0)
        {
            if (SpawnInCircle) Manager.SpawnCircleAroundPlayer(toSpawn, SpawnRadius);
            else SpawnRandom(toSpawn);
        }

        targetCount = desiredCount;
    }

    void SpawnRandom(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * SpawnRadius;
            Vector3 spawnPos = new Vector3(Player.position.x + offset.x, Player.position.y, Player.position.z + offset.y);
            Manager.SpawnAt(spawnPos);
        }
    }
}
