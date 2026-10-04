using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopController : MonoBehaviour
{
    public float moveForce = 15f;
    public float maxSpeed = 8f;
    public Transform cameraTransform; // optional: moves relative to the camera

    Rigidbody rb;
    Vector3 input;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        // Read input in Update, apply forces in FixedUpdate
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);
    }

    void FixedUpdate()
    {
        Vector3 dir = input;

        if (cameraTransform != null)
        {
            Vector3 fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            dir = fwd * input.z + right * input.x;
        }

        // Only push if we're under the speed cap
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude < maxSpeed)
            rb.AddForce(dir * moveForce, ForceMode.Acceleration);
    }
}