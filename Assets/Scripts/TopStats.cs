using UnityEngine;

[DefaultExecutionOrder(-100)] // runs before the other top scripts' Awake
public class TopStats : MonoBehaviour
{
    public TopData data;
    public float baseDamage = 2f;        // damage per unit of impact speed
    public float minImpactSpeed = 2f;    // ignore light touches
    public string topTag = "Top";

    public float Health { get; private set; }
    public bool IsDead => Health <= 0f;
    public event System.Action<TopStats> Died;

    void Awake()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name} has no TopData assigned", this);
            return;
        }

        Health = data.maxHealth;
        ApplyData();
    }

    // Pushes the data into the top's other scripts
    void ApplyData()
    {
        if (TryGetComponent<Spin>(out var spin))
        {
            spin.startSpin = data.startSpin;
            spin.maxSpin = data.maxSpin;
            spin.spinLossPerSecond = data.spinLossPerSecond;
        }

        if (TryGetComponent<Colideboost>(out var boost))
        {
            boost.maxSpin = data.maxSpin;
        }

        if (TryGetComponent<TopController>(out var ctrl))
        {
            ctrl.moveForce = data.moveForce;
            ctrl.maxSpeed = data.maxSpeed;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (IsDead || data == null) return;
        if (!c.gameObject.CompareTag(topTag)) return;
        if (!c.gameObject.TryGetComponent<TopStats>(out var other) || other.data == null) return;

        float speed = c.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        // Damage taken depends on the OTHER top's attack and this top's defense
        float damage = speed * baseDamage * other.data.attack / data.defense;
        TakeDamage(damage);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        Health = Mathf.Max(0f, Health - amount);
        if (Health <= 0f) Died?.Invoke(this);
    }

    public void Heal(float amount)
    {
        if (IsDead || data == null) return;
        Health = Mathf.Min(data.maxHealth, Health + amount);
    }
}