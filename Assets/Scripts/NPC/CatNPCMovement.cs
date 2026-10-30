using UnityEngine;
using UnityEngine.Tilemaps;

// Wandering cat NPC. Patrols faster than the villagers. When it bumps into
// a person (a villager, another cat, or the player) it gets startled and
// dashes away in the opposite direction for a short burst; walls just turn
// it around. Bumping into the player still slows the player down.
public class CatNPCMovement : MonoBehaviour
{
    public float patrolSpeed = 3f;
    public float fleeSpeed = 8f;
    public float fleeDuration = 0.7f;

    private int direction = -1;
    private float fleeTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Tilemap[] blockers;

    // Painted static layers: a tile on any of these counts as solid for
    // NPCs, even where no collider was painted (roofs, trees, lamps...).
    private static readonly string[] BlockerNames =
        { "Building", "Building collider", "Building decor", "roof", "nature", "static obstacles", "Building behind", "static obstacle (behind)", "obstacles behind (1)", "obstacles middleground", "obstacle part in front of player" };

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var all = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        var list = new System.Collections.Generic.List<Tilemap>();
        foreach (var tm in all)
            if (System.Array.IndexOf(BlockerNames, tm.gameObject.name) >= 0)
                list.Add(tm);
        blockers = list.ToArray();
    }

    bool Blocked(Vector3 pos)
    {
        if (blockers == null) return false;
        foreach (var tm in blockers)
            if (tm != null && tm.HasTile(tm.WorldToCell(pos)))
                return true;
        return false;
    }

    void FixedUpdate()
    {
        float speed = patrolSpeed;
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.fixedDeltaTime;
            speed = fleeSpeed;
        }
        Vector3 step = Vector3.right * direction * speed * Time.fixedDeltaTime;
        Vector3 next = transform.position + step;
        Vector3 probe = next + Vector3.right * direction * 0.3f;
        if (!Blocked(transform.position) && (Blocked(next) || Blocked(probe)))
        {
            direction *= -1;
            if (spriteRenderer != null)
                spriteRenderer.flipX = direction > 0;
            return;
        }
        transform.position = next;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isPlayer = collision.gameObject.CompareTag("Player");
        bool isPerson = isPlayer
            || collision.gameObject.GetComponent<NPCMovement>() != null
            || collision.gameObject.GetComponent<CatNPCMovement>() != null;

        if (isPlayer)
        {
            player.CatController cat = collision.gameObject.GetComponent<player.CatController>();
            if (cat != null)
                cat.ApplySlow();
        }

        if (isPerson)
        {
            // dash away from whoever startled it
            float away = transform.position.x - collision.transform.position.x;
            direction = away >= 0f ? 1 : -1;
            fleeTimer = fleeDuration;
        }
        else
        {
            direction *= -1;
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction > 0;
    }
}
