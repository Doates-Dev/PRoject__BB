using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionVFX : MonoBehaviour
{
    [Header("Top vs top hit effect")]
    public GameObject[] vfxPrefabs;          // one is picked at random on each hit
    public bool avoidRepeat = true;          // don't pick the same effect twice in a row
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

    [Header("Ground effect direction")]
    public bool faceOppositeToMovement = true; // effect streams out behind the top
    [Range(0f, 80f)] public float upwardAngle = 25f; // tilts the trail up from the ground (0 = flat)
    public float minDirectionSpeed = 0.5f;     // below this speed, keep the last direction
    public float turnSpeed = 15f;              // higher = snappier, lower = smoother
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f); // corrects for the effect's own emission axis

    Rigidbody rb;
    float lastTime = -10f;
    float lastGroundTime = -10f;
    Vector3 groundPoint;
    GameObject groundFx;
    bool groundFxActive;
    int lastVfxIndex = -1;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start()
    {
        if (groundVfxPrefab == null) return;

        // Not parented to the top, so it doesn't spin with it
        groundFx = Instantiate(groundVfxPrefab, transform.position, Quaternion.Euler(rotationOffset));
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

    GameObject PickVfx()
    {
        if (vfxPrefabs == null || vfxPrefabs.Length == 0) return null;
        if (vfxPrefabs.Length == 1) return vfxPrefabs[0];

        int index = Random.Range(0, vfxPrefabs.Length);

        // Re-roll once by shifting to the next slot if it matches the last one
        if (avoidRepeat && index == lastVfxIndex)
            index = (index + 1) % vfxPrefabs.Length;

        lastVfxIndex = index;
        return vfxPrefabs[index];
    }

    void HandleTopHit(Collision c)
    {
        if (vfxPrefabs == null || vfxPrefabs.Length == 0) return;
        if (!c.gameObject.CompareTag(topTag)) return;
        if (c.relativeVelocity.magnitude < minImpactSpeed) return;
        if (Time.time - lastTime < cooldown) return;

        // Only let one top play the effect if both tops have this script
        if (c.gameObject.TryGetComponent<CollisionVFX>(out _) &&
            gameObject.GetHashCode() > c.gameObject.GetHashCode()) return;

        GameObject prefab = PickVfx();
        if (prefab == null) return; // empty slot in the array

        lastTime = Time.time;

        ContactPoint contact = c.GetContact(0);
        Quaternion rot = alignToNormal
            ? Quaternion.LookRotation(contact.normal)
            : Quaternion.identity;

        GameObject fx = Instantiate(prefab, contact.point, rot);
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

            if (faceOppositeToMovement)
            {
                // Horizontal velocity only, so hops don't tilt the effect
                Vector3 v = rb.linearVelocity; // rb.velocity on older Unity
                v.y = 0f;

                if (v.magnitude >= minDirectionSpeed)
                {
                    // Flat direction behind the top, then lifted upward by the chosen angle
                    Vector3 back = -v.normalized;
                    float rad = upwardAngle * Mathf.Deg2Rad;
                    Vector3 dir = back * Mathf.Cos(rad) + Vector3.up * Mathf.Sin(rad);

                    Quaternion target = Quaternion.LookRotation(dir, Vector3.up)
                                        * Quaternion.Euler(rotationOffset);
                    groundFx.transform.rotation = Quaternion.Slerp(
                        groundFx.transform.rotation, target, turnSpeed * Time.deltaTime);
                }
            }

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