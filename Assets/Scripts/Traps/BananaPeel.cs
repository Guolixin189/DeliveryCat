using UnityEngine;

// Banana peel: pops in with a small bounce warning, then arms. Stepping on
// it slips the player: a short slide out plus a 1s hard stun. One-time use.
public class BananaPeel : MonoBehaviour
{
    public float stunDuration = 1f;
    public float knockbackDistance = 0.8f;
    public float warningTime = 0.4f;

    private float spawnTime;
    private bool armed;
    private bool used;
    private Vector3 baseScale;

    void Start()
    {
        spawnTime = Time.time;
        baseScale = transform.localScale;
        transform.localScale = baseScale * 0.2f;
    }

    void Update()
    {
        if (used)
            return;
        float t = (Time.time - spawnTime) / warningTime;
        if (t >= 1f)
        {
            armed = true;
            transform.localScale = baseScale;
            return;
        }
        // bounce in (ease-out-back): overshoots, then settles at full size
        float u = t - 1f;
        float f = 1f + 2.70158f * u * u * u + 1.70158f * u * u;
        transform.localScale = baseScale * (0.2f + 0.8f * f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed || used)
            return;
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat == null)
            return;
        used = true;
        Vector2 dir = (Vector2)cat.transform.position - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;
        dir.Normalize();
        cat.ApplyKnockback(dir, knockbackDistance, 0.25f);
        cat.ApplyStun(stunDuration);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        transform.localScale = baseScale * new Vector3(1.2f, 0.6f, 1f);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.5f);
        Destroy(gameObject, 0.3f);
    }
}
