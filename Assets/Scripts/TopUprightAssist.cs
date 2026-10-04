using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopUprightAssist : MonoBehaviour
{
    public float uprightSpin = 20f;
    public float fullStrengthSpin = 35f;
    public float strength = 30f;
    public float tiltDamping = 4f;

    [Header("Debug")]
    public bool logSpin = true;
    public float logInterval = 0.5f; // seconds between log messages

    Rigidbody rb;
    float nextLogTime;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        float spin = Mathf.Abs(Vector3.Dot(rb.angularVelocity, transform.up));

        if (logSpin && Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + logInterval;
            Debug.Log($"{name} spin: {spin:F1} rad/s", this);
        }

        if (spin < uprightSpin) return;

        float t = Mathf.InverseLerp(uprightSpin, fullStrengthSpin, spin);

        Vector3 axis = Vector3.Cross(transform.up, Vector3.up);
        Vector3 spring = axis * strength;

        Vector3 spinPart = transform.up * Vector3.Dot(rb.angularVelocity, transform.up);
        Vector3 tiltVel = rb.angularVelocity - spinPart;
        Vector3 damper = -tiltVel * tiltDamping;

        rb.AddTorque((spring + damper) * t, ForceMode.Acceleration);
    }
}