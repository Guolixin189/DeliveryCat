using UnityEngine;

// Banana peel: drops in from above with a bounce, then arms. Stepping on
// it slips the player: a short slide out plus a 1s hard stun. One-time use.
// The spawn animation is deliberately loud (a falling drop plus a squash
// bounce) so the player sees the peel coming instead of it just appearing.
public class BananaPeel : MonoBehaviour
{
    public float stunDuration = 1f;
    public float knockbackDistance = 0.8f;

    [Header("Spawn animation")]
    public float dropHeight = 1.5f;
    public float fallTime = 0.35f;
    public float bounceTime = 0.25f;
    public float bounceHeight = 0.3f;

    [Header("Sound effects")]
    public AudioClip spawnSFX;
    public AudioClip slipSFX;
    private AudioSource audioSource;

    private float spawnTime;
    private bool armed;
    private bool used;
    private Vector3 baseScale;
    private Vector3 basePosition;

    void Start()
    {
        spawnTime = Time.time;
        baseScale = transform.localScale;
        basePosition = transform.position;
        // Start above the spot, small, so the drop is visible.
        transform.position = basePosition + Vector3.up * dropHeight;
        transform.localScale = baseScale * 0.5f;
        // Sound: use the assigned clips, or the generated ones in
        // Resources/SFX if the prefab has none wired up.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (spawnSFX == null)
            spawnSFX = Resources.Load<AudioClip>("SFX/banana_spawn");
        if (slipSFX == null)
            slipSFX = Resources.Load<AudioClip>("SFX/banana_slip");
        if (audioSource != null && spawnSFX != null)
            audioSource.PlayOneShot(spawnSFX);
    }

    void Update()
    {
        if (used)
            return;
        float t = Time.time - spawnTime;
        float total = fallTime + bounceTime;
        if (t >= total)
        {
            armed = true;
            transform.position = basePosition;
            transform.localScale = baseScale;
            return;
        }
        if (t < fallTime)
        {
            // Falling: accelerates downward (ease-in), grows to full size.
            float u = t / fallTime;
            float yOffset = dropHeight * (1f - u * u);
            transform.position = basePosition + Vector3.up * yOffset;
            float s = 0.5f + 0.5f * u;
            transform.localScale = baseScale * s;
        }
        else
        {
            // Landing bounce: a small hop that decays, with a squash
            // on landing that eases back to full size.
            float u = (t - fallTime) / bounceTime;
            float yOffset = bounceHeight * Mathf.Sin(Mathf.PI * u) * (1f - u);
            transform.position = basePosition + Vector3.up * yOffset;
            float squash = Mathf.Sin(Mathf.PI * u);
            float sx = 1f + 0.25f * squash;
            float sy = 1f - 0.3f * squash;
            transform.localScale = Vector3.Scale(baseScale, new Vector3(sx, sy, 1f));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed || used)
            return;
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat == null)
            return;
        used = true;
        if (audioSource != null && slipSFX != null)
            audioSource.PlayOneShot(slipSFX);
        Vector2 dir = (Vector2)cat.transform.position - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;
        dir.Normalize();
        cat.ApplyKnockback(dir, knockbackDistance, 0.25f);
        cat.ApplyStun(stunDuration);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        transform.localScale = Vector3.Scale(baseScale, new Vector3(1.2f, 0.6f, 1f));
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.5f);
        Destroy(gameObject, 0.3f);
    }
}
