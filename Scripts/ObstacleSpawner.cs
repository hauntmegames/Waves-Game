// ObstacleSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs (random pick)")]
    public Obstacle[] obstaclePrefabs;

    [Header("Lanes")]
    public int lanes = 5;
    public float laneWidth = 2f;
    public float laneY = 0.5f;        // height of the water surface
    public float spawnZ = 60f;        // how far in front to spawn
    public float killZ = -20f;        // recycle when past this

    [Header("Timing")]
    public float spawnInterval = 0.8f;
    public Vector2 gapZRange = new Vector2(6f, 12f); // extra spacing by skipping spawns

    [Header("Pooling")]
    public int poolPerPrefab = 8;

    float _timer;
    Transform _t;

    readonly Dictionary<Obstacle, Queue<Obstacle>> _pools = new();

    void Awake()
    {
        _t = transform;
        foreach (var prefab in obstaclePrefabs)
        {
            var q = new Queue<Obstacle>();
            for (int i = 0; i < poolPerPrefab; i++)
            {
                var o = Instantiate(prefab, _t);
                o.gameObject.SetActive(false);
                o.OnDespawn = ReturnToPool;
                q.Enqueue(o);
            }
            _pools[prefab] = q;
        }
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnOne();
            _timer = spawnInterval;
        }
    }

    void SpawnOne()
    {
        if (obstaclePrefabs.Length == 0) return;

        int half = (lanes - 1) / 2;
        int laneIndex = Random.Range(-half, half + 1);
        float x = laneIndex * laneWidth;

        // Occasionally skip a spawn to create bigger gaps
        if (Random.value < 0.25f) return;

        var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        var o = GetFromPool(prefab);

        Vector3 pos = new Vector3(x, laneY, spawnZ);
        o.transform.position = pos;
        o.killZ = killZ;
        o.gameObject.SetActive(true);
    }

    Obstacle GetFromPool(Obstacle prefab)
    {
        var q = _pools[prefab];
        if (q.Count > 0)
        {
            var o = q.Dequeue();
            return o;
        }
        else
        {
            var o = Instantiate(prefab, _t);
            o.gameObject.SetActive(false);
            o.OnDespawn = ReturnToPool;
            return o;
        }
    }

    void ReturnToPool(Obstacle o)
    {
        o.gameObject.SetActive(false);
        foreach (var kv in _pools)
        {
            // match by prefab type (compare component names)
            if (kv.Key.GetType() == o.GetType())
            {
                kv.Value.Enqueue(o);
                return;
            }
        }
        // if not found, just push into first pool
        _pools[obstaclePrefabs[0]].Enqueue(o);
    }
}
