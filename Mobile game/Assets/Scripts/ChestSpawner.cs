using UnityEngine;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject chestPrefab;

    [Header("spawn")]
    public float intervalSeconds = 10f;
    public float spawnRadius = 12f;
    public int maxSpawnAtTheSameTime = 1;

    private float _timer;
    private readonly List<GameObject> _active = new List<GameObject>();

    private void Awake()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _timer = intervalSeconds;
    }

    private void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (_active[i] == null || !_active[i].activeInHierarchy) _active.RemoveAt(i);
        }

        if (!player || !chestPrefab || _active.Count >= maxSpawnAtTheSameTime) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Spawn();
            _timer += intervalSeconds;
        }
    }
    private void Spawn()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = player.position + new Vector3(dir.x, 0f, dir.y) * spawnRadius;

        var go = Instantiate(chestPrefab, pos, Quaternion.identity);
        _active.Add(go);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
#endif
}
