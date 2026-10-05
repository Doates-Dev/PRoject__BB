using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Colideboost : MonoBehaviour
{
    [Header("Spin boost")]
    public float spinBoost = 10f;       // instant, fixed boost per hit (rad/s)
    public float gradualBoost = 20f;    // total extra spin added over the duration
    public float gradualDuration = 10f; // seconds the extra boost is spread over
    public float maxSpin = 80f;         // cap so it can't spin forever

    [Header("Jump")]
    public float jumpImpulse = 4f;

    [Header("Filtering")]
    public string topTag = "Top";
    public float minImpactSpeed = 1f;
    public float cooldown = 0.25f;

    struct GradualEntry
    {
        public float rate;     // spin added per second
        public float endTime;
    }

    Rigidbody rb;
    float lastHitTime = -10f;
    readonly List<GradualEntry> active = new List<GradualEntry>();

    void Awake() => rb = GetComponent<Rigidbody>();

    void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag(topTag)) return;
        if (c.relativeVelocity.magnitude < minImpactSpeed) return;
        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // 1) Instant fixed boost
        AddSpin(spinBoost);

        // 2) Start a gradual boost over the next gradualDuration seconds
        if (gradualBoost > 0f && gradualDuration > 0f)
        {
            active.Add(new GradualEntry
            {
                rate = gradualBoost / gradualDuration,
                endTime = Time.time + gradualDuration
            });
        }

        // Jump: clear downward velocity first so the hop is consistent
        Vector3 v = rb.linearVelocity; // rb.velocity on older Unity
        if (v.y < 0f) { v.y = 0f; rb.linearVelocity = v; }
        rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        if (active.Count == 0) return;

        // Add up the rates of every boost still running, and drop finished ones
        float rate = 0f;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (Time.time >= active[i].endTime) { active.RemoveAt(i); continue; }
            rate += active[i].rate;
        }

        if (rate > 0f) AddSpin(rate * Time.fixedDeltaTime);
    }

    // Adds spin around the top's up axis, keeping its spin direction, never exceeding maxSpin
    void AddSpin(float amount)
    {
        float spin = Vector3.Dot(rb.angularVelocity, transform.up);
        float dir = spin >= 0f ? 1f : -1f;
        float current = Mathf.Abs(spin);

        // Never reduces spin that is already above the cap
        float newSpin = Mathf.Max(current, Mathf.Min(current + amount, maxSpin));
        rb.angularVelocity += transform.up * (dir * newSpin - spin);
    }
}