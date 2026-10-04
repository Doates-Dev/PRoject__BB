using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Colideboost : MonoBehaviour
{
    [Header("Spin boost")]
    public float spinBoost = 10f;        // spin added per hit (rad/s)
    public float maxSpin = 80f;          // cap so it can't spin forever

    [Header("Jump")]
    public float jumpImpulse = 4f;       // upward kick on hit

    [Header("Filtering")]
    public string topTag = "Top";        // only react to other tops
    public float minImpactSpeed = 1f;    // ignore light touches
    public float cooldown = 0.25f;       // prevents repeat triggers while touching

    Rigidbody rb;
    float lastHitTime = -10f;

    void Awake() => rb = GetComponent<Rigidbody>();

    void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag(topTag)) return;
        if (c.relativeVelocity.magnitude < minImpactSpeed) return;
        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // Increase spin around the top's own up axis, keeping its spin direction
        float spin = Vector3.Dot(rb.angularVelocity, transform.up);
        float dir = spin >= 0f ? 1f : -1f;
        float newSpin = Mathf.Min(Mathf.Abs(spin) + spinBoost, maxSpin);
        rb.angularVelocity += transform.up * (dir * newSpin - spin);

        // Jump: clear downward velocity first so the hop is consistent
        Vector3 v = rb.linearVelocity; // rb.velocity on older Unity
        if (v.y < 0f) { v.y = 0f; rb.linearVelocity = v; }
        rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }
}