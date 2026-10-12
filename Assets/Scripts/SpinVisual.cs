using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinVisual : MonoBehaviour
{
    [Header("Visual model")]
    public Transform visual;                   // the mesh object (a child of the top)

    [Header("Anti-aliasing")]
    public float maxDegreesPerFrame = 25f;     // keep below 180 / (number of repeating features)

    [Header("Blur disc (optional)")]
    public Renderer blurRenderer;              // flat translucent disc, child of the visual
    public float blurStartSpin = 60f;          // blur begins fading in at this spin
    public float blurFullSpin = 120f;          // full blur at this spin
    [Range(0f, 1f)] public float blurMaxAlpha = 0.6f;

    Rigidbody rb;
    Vector3 localPos;
    Quaternion baseRot;
    float angle;
    Material blurMat;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start()
    {
        if (visual == null) return;

        // Remember where the model sits relative to the top, then detach it so it
        // no longer inherits the physics spin
        localPos = visual.localPosition;
        baseRot = visual.localRotation;
        visual.SetParent(null, true);

        if (blurRenderer != null) blurMat = blurRenderer.material;
    }

    void LateUpdate()
    {
        if (visual == null) return;

        float spin = Vector3.Dot(rb.angularVelocity, transform.up);

        // Visible rotation per frame is capped, so it can't alias
        float step = Mathf.Min(Mathf.Abs(spin) * Mathf.Rad2Deg * Time.deltaTime, maxDegreesPerFrame);
        angle += Mathf.Sign(spin) * step;

        // Follow the top's position and tilt, but not its physics spin
        Quaternion tilt = Quaternion.FromToRotation(Vector3.up, transform.up);
        visual.SetPositionAndRotation(
            transform.TransformPoint(localPos),
            tilt * Quaternion.Euler(0f, angle, 0f) * baseRot);

        // Fade the blur disc in as spin increases
        if (blurMat != null)
        {
            float a = Mathf.InverseLerp(blurStartSpin, blurFullSpin, Mathf.Abs(spin)) * blurMaxAlpha;
            Color c = blurMat.color;
            c.a = a;
            blurMat.color = c;
            blurRenderer.enabled = a > 0.01f;
        }
    }

    void OnDestroy()
    {
        if (visual != null) Destroy(visual.gameObject);
        if (blurMat != null) Destroy(blurMat);
    }
}