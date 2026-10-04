using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionVFX : MonoBehaviour
{
    [Header("Top vs top hit effect")]
    public GameObject vfxPrefab;
    public string topTag = "Top";
    public float minImpactSpeed = 2f;
    public float cooldown = 0.2f;
    public float destroyAfter = 3f;
    public bool alignToNormal = true;

    [Header("Ground contact effect")]
    public GameObject groundVfxPrefab;       // effect that plays while touching the floor
    public LayerMask groundLayers;           // set to your floor/arena layer
    public Vector3 groundVfxOffset = Vector3.zero; // nudge it, e.g. (0, 0.02, 0)
    public float groundGraceTime = 0.1f;     // how long after the last contact it counts as grounded

    float lastTime = -10f;
    float lastGroundTime = -10f;
    Vector3 groundPoint;
    GameObject groundFx;
    bool groundFxActive;

    void Start()
    {
        if (groundVfxPrefab == null) return;

        // Not parented to the top, so it doesn't spin with it
        groundFx = Instantiate(groundVfxPrefab, transform.position, Quaternion.identity);
        groundFx.SetActive(false);
    }

    void OnCollisionEnter(Collision c)
    {
        HandleTopHit(c);
        HandleGround(c);
    }

    void OnCollisionStay(Collision c)
    {
        HandleGround(c);
    }

    void HandleTopHit(Collision c)
    {
        if (vfxPrefab == null) return;
        if (!c.gameObject.CompareTag(topTag)) return;
        if (c.relativeVelocity.magnitude < minImpactSpeed) return;
        if (Time.time - lastTime < cooldown) return;

        // Both tops get this callback, so only one of them spawns the effect
        if (gameObject.GetHashCode() > c.gameObject.GetHashCode()) return;

        lastTime = Time.time;

        ContactPoint contact = c.GetContact(0);
        Quaternion rot = alignToNormal
            ? Quaternion.LookRotation(contact.normal)
            : Quaternion.identity;

        GameObject fx = Instantiate(vfxPrefab, contact.point, rot);
        Destroy(fx, destroyAfter);
    }

    void HandleGround(Collision c)
    {
        if (groundFx == null) return;
        if (((1 << c.gameObject.layer) & groundLayers) == 0) return;

        lastGroundTime = Time.time;
        groundPoint = c.GetContact(0).point;
    }

    void LateUpdate()
    {
        if (groundFx == null) return;

        bool grounded = Time.time - lastGroundTime < groundGraceTime;

        if (grounded)
        {
            groundFx.transform.position = groundPoint + groundVfxOffset;

            if (!groundFxActive)
            {
                groundFx.SetActive(true);
                groundFxActive = true;
            }
        }
        else if (groundFxActive)
        {
            groundFx.SetActive(false);
            groundFxActive = false;
        }
    }

    void OnDestroy()
    {
        if (groundFx != null) Destroy(groundFx);
    }
}