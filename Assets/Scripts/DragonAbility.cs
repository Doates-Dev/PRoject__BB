using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragonAbility : MonoBehaviour
{
    [Header("Ability")]
    public KeyCode key = KeyCode.Q;
    public float abilityMaxSpin = 500f; // spin cap and spin speed during the ability
    public float duration = 3f;         // seconds the ability lasts
    public float cooldown = 8f;         // seconds after it ends before it can be used again
    public bool holdSpin = true;        // keep spin at the max for the whole duration

    Rigidbody rb;
    Spin spinScript;

    bool active;
    float endTime;
    float nextUseTime;
    float spinBefore;   // signed spin around the up axis before the ability
    float capBefore;    // rb.maxAngularVelocity before the ability
    float spinDir = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spinScript = GetComponent<Spin>();
    }

    void Update()
    {
        if (!active && Time.time >= nextUseTime && Input.GetKeyDown(key))
            Activate();
    }

    void Activate()
    {
        active = true;
        endTime = Time.time + duration;
        nextUseTime = endTime + cooldown;

        // Remember the state to return to
        spinBefore = Vector3.Dot(rb.angularVelocity, transform.up);
        capBefore = rb.maxAngularVelocity;
        spinDir = spinBefore >= 0f ? 1f : -1f;

        // Raise the cap, stop the normal spin decay, and jump to the new spin
        rb.maxAngularVelocity = abilityMaxSpin;
        if (spinScript != null) spinScript.enabled = false;

        SetSpin(spinDir * abilityMaxSpin);
    }

    void FixedUpdate()
    {
        if (!active) return;

        if (Time.time >= endTime)
        {
            End();
            return;
        }

        // Collisions can knock spin down, so push it back up each step
        if (holdSpin) SetSpin(spinDir * abilityMaxSpin);
    }

    void End()
    {
        // Back to the spin it had before, with the old cap and normal decay
        SetSpin(spinBefore);
        Restore();
    }

    void Restore()
    {
        rb.maxAngularVelocity = capBefore;
        if (spinScript != null) spinScript.enabled = true;
        active = false;
    }

    void OnDisable()
    {
        // If the top is disabled mid-ability, don't leave it in a changed state
        if (active) Restore();
    }

    // Sets the spin around the top's up axis without touching its tilt or wobble
    void SetSpin(float target)
    {
        float current = Vector3.Dot(rb.angularVelocity, transform.up);
        rb.angularVelocity += transform.up * (target - current);
    }
}