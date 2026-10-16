using UnityEngine;

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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        float speed = patrolSpeed;
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.fixedDeltaTime;
            speed = fleeSpeed;
        }
        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;
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
