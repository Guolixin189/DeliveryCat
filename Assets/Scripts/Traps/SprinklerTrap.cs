using UnityEngine;

// Rotating sprinkler: the water jet spins a full 360 degrees around the
// sprinkler head, over and over. Players caught in the jet are slowed,
// like a moving, predictable puddle.
public class SprinklerTrap : MonoBehaviour
{
    public Transform jet;              // jet pivot, rotates around the sprinkler head
    public float jetRange = 2.2f;      // how far the water reaches
    public float jetHalfAngle = 25f;   // half-width of the jet sector in degrees
    public float rotationSpeed = 90f;  // jet spin speed in degrees per second
    public float slowFactor = 0.5f;

    private player.CatController cat;
    private float findTimer = 0f;
    private float angle = 0f;

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
        // spin the jet a full circle, continuously
        angle += rotationSpeed * Time.deltaTime;
        if (angle >= 360f)
            angle -= 360f;
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
