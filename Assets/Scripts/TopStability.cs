using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopStability : MonoBehaviour
{
    public Vector3 centerOfMass = new Vector3(0f, -0.3f, 0f); // local space
    public bool showGizmo = true;

    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMass), 0.03f);
    }
}