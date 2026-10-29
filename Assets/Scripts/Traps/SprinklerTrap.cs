using UnityEngine;

// Rotating sprinkler: a water jet sweeps back and forth around a center
// direction. Players caught in the jet are slowed, like a moving,
// predictable puddle.
public class SprinklerTrap : MonoBehaviour
{
    public Transform jet;              // jet pivot, rotates around the sprinkler head
    public float jetRange = 2.2f;      // how far the water reaches
    public float jetHalfAngle = 25f;   // half-width of the jet sector in degrees
    public float sweepAngle = 60f;     // jet swings +/- sweepAngle around the center
    public float sweepCenterAngle = 0f; // center direction in degrees (0 = east)
    public float sweepSpeed = 1f;      // sweep oscillation speed
    public float slowFactor = 0.5f;

    private player.CatController cat;
    private float findTimer = 0f;

    void Start()
    {
        cat = FindFirstObjectByType<player.CatController>();
    }

    void Update()
    {
        if (cat == null)
        {
            // The player may spawn after this trap; retry occasionally
            findTimer -= Time.deltaTime;
            if (findTimer <= 0f)
            {
                cat = FindFirstObjectByType<player.CatController>();
                findTimer = 0.5f;
            }
            if (cat == null)
                return;
        }
        float angle = sweepCenterAngle + Mathf.Sin(Time.time * sweepSpeed) * sweepAngle;
        if (jet != null)
            jet.localRotation = Quaternion.Euler(0f, 0f, angle);
        Vector2 toPlayer = (Vector2)cat.transform.position - (Vector2)transform.position;
        if (toPlayer.magnitude > jetRange)
            return;
        float playerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        float diff = Mathf.DeltaAngle(angle, playerAngle);
        if (Mathf.Abs(diff) < jetHalfAngle)
            cat.ApplyTrapSlow(slowFactor);
    }
}
