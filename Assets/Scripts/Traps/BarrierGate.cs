using UnityEngine;

// Retractable barrier: green light = arm raised, road open;
// red light = arm lowered, road closed. The cycle is driven by Time.time,
// so multiple gates with the same timings stay in sync.
public class BarrierGate : MonoBehaviour
{
    public Transform arm;              // arm pivot, rotates around the hinge
    public SpriteRenderer light;       // signal light, tinted green/red
    public Collider2D armCollider;     // blocks the road while the arm is down
    public float greenDuration = 4f;
    public float redDuration = 4f;
    public float moveDuration = 0.5f;  // arm raise/lower time
    public float phaseOffset = 0f;     // shift the cycle
    public float closedAngle = 0f;     // arm angle while closed (0 = pointing east)
    public float openAngle = 80f;      // arm angle while open

    private float cycle => greenDuration + redDuration;

    void Update()
    {
        float t = (Time.time + phaseOffset) % cycle;
        bool green = t < greenDuration;
        float target = green ? openAngle : closedAngle;
        if (arm != null)
        {
            float current = arm.localEulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, target,
                (Mathf.Abs(openAngle - closedAngle) / moveDuration) * Time.deltaTime);
            arm.localRotation = Quaternion.Euler(0f, 0f, next);
            if (armCollider != null)
                armCollider.enabled = Mathf.Abs(Mathf.DeltaAngle(next, closedAngle)) < 5f;
        }
        if (light != null)
            light.color = green ? new Color(0.48f, 0.86f, 0.48f) : new Color(0.85f, 0.42f, 0.35f);
    }
}
