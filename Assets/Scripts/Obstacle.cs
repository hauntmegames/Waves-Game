using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Obstacle : MonoBehaviour
{
    [Header("Scroll & Lifetime")]
    public float speedMultiplier = 1f;   // uses TrackMover.Speed * this
    public float killZ = -20f;

    [Header("Prefab Y offset (optional)")]
    public float spawnYOffset = 0f;

    [Header("Debug")]
    public bool logHits = false;

    public Action<Obstacle> OnDespawn;

    Transform _t;
    Collider _col;
    Rigidbody _rb;

    void Awake()
    {
        _t = transform;
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void Update()
    {
        float dz = TrackMover.Speed * speedMultiplier * Time.deltaTime;
        _t.position += Vector3.back * dz;

        if (_t.position.z < killZ) Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (logHits) Debug.Log($"Obstacle hit by {other.name}", this);

        var hit = other.GetComponent<RaftHitResponder>() ?? other.GetComponentInParent<RaftHitResponder>();
        if (hit) hit.OnHit(_t.position);

        Despawn();
    }

    void Despawn()
    {
        if (OnDespawn != null) OnDespawn(this);
        else gameObject.SetActive(false);
    }
}
