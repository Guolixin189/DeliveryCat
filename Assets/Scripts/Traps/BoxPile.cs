using UnityEngine;

// Pushable box pile. The box is kinematic (a solid wall the player cannot
// clip through) and slides away at pushSpeed while the player keeps pushing
// into it, stopping when something solid is ahead (Rigidbody2D.Cast).
public class BoxPile : MonoBehaviour
{
    public float pushSpeed = 1.5f;

    private Rigidbody2D rb;
    private player.CatController pushingCat;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        player.CatController cat = collision.gameObject.GetComponent<player.CatController>();
        if (cat != null)
            pushingCat = cat;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<player.CatController>() != null)
            pushingCat = null;
    }

    void FixedUpdate()
    {
        if (pushingCat == null || rb == null)
            return;
        Vector2 dir = (Vector2)transform.position - (Vector2)pushingCat.transform.position;
        // Push along the dominant axis so the box slides cleanly, not diagonally
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = new Vector2(Mathf.Sign(dir.x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(dir.y));
        float step = pushSpeed * Time.fixedDeltaTime;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        RaycastHit2D[] hits = new RaycastHit2D[4];
        int n = rb.Cast(dir, filter, hits, step + 0.02f);
        if (n == 0)
            rb.MovePosition(rb.position + dir * step);
    }
}
