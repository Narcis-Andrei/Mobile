using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject chestPrefab;

    [Header("Respawn")]
    public float respawnSeconds = 3f;

    [Header("Spawn Radius")]
    public float spawnRadiusMin = 8f;
    public float spawnRadiusMax = 18f;
    public float minutesToMaxRadius = 8f;

    private float _timer;
    private GameObject _active;
    private float _timePassed;

    private void Awake()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _timer = 0f;
        _timePassed = 0f;
    }

    private void Update()
    {
        if (!player || !chestPrefab) return;

        _timePassed += Time.deltaTime;

        if (_active != null && _active.activeInHierarchy)
            return;

        if (_active != null && !_active.activeInHierarchy)
            _active = null;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Spawn();
            _timer = respawnSeconds;
        }
    }

    private void Spawn()
    {
        float minutes = _timePassed / 60f;
        float t = minutesToMaxRadius <= 0f ? 1f : Mathf.Clamp01(minutes / minutesToMaxRadius);
        float radius = Mathf.Lerp(spawnRadiusMin, spawnRadiusMax, t);

        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = player.position + new Vector3(dir.x, 0f, dir.y) * radius;

        _active = Instantiate(chestPrefab, pos, Quaternion.identity, transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!player) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadiusMin);

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(player.position, spawnRadiusMax);
    }
#endif
}