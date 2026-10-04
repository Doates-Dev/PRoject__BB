using UnityEngine;

public class TopHitSFX : MonoBehaviour
{
    public string topTag = "Top";
    public float minImpactSpeed = 1f;    // ignore light touches
    public float maxImpactSpeed = 15f;   // impact speed that gives full volume
    public float cooldown = 0.1f;

    float lastTime = -10f;
   

    void OnCollisionEnter(Collision c)
    {
        Debug.Log("Manager object", SFXManager.Instance);
        Debug.Log($"{name} collided with {c.gameObject.name}, tag: {c.gameObject.tag}, speed: {c.relativeVelocity.magnitude:F1}");
        Debug.Log($"SFX: manager {(SFXManager.Instance == null ? "NULL" : "ok")}");
        if (SFXManager.Instance == null) return;
        if (!c.gameObject.CompareTag(topTag)) return;

        float speed = c.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;
        if (Time.time - lastTime < cooldown) return;

        // Both tops get this callback, so only one plays the sound
        // Only defer to the other top if it also has this script
        if (c.gameObject.TryGetComponent<TopHitSFX>(out _) &&
            gameObject.GetHashCode() > c.gameObject.GetHashCode()) return;

        lastTime = Time.time;

        float intensity = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, speed);
        SFXManager.Instance.PlayTopHit(c.GetContact(0).point, Mathf.Lerp(0.4f, 1f, intensity));
    }
}