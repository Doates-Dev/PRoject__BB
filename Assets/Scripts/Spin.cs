using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spin : MonoBehaviour
{
    
    public float spinLossPerSecond = 3f; // how much spin is lost each second
    public float minSpin = 0.5f;         // below this, stop applying decay

    Rigidbody rb;

    public float startSpin = 200f;
    public float maxSpin = 300f; // must be above startSpin and any collision boost

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxSpin;
    }

    void Start()
    {
        rb.angularVelocity = transform.up * startSpin;
    }

    void FixedUpdate()
    {
        // Spin speed around the top's own up axis
        float spin = Vector3.Dot(rb.angularVelocity, transform.up);

        if (Mathf.Abs(spin) > minSpin)
        {
            float newSpin = Mathf.MoveTowards(spin, 0f, spinLossPerSecond * Time.fixedDeltaTime);
            rb.angularVelocity += transform.up * (newSpin - spin);
        }
    }
}