using UnityEngine;
using UnityEngine.Tilemaps;

// Pushable box pile. The box is kinematic (a solid wall the player cannot
// clip through) and slides away at pushSpeed while the player keeps pushing
// into it, stopping when something solid is ahead (Rigidbody2D.Cast) or
// when the next step would put it into the river.
public class BoxPile : MonoBehaviour
{
    public float pushSpeed = 1.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Tilemap[] water;
    private player.CatController pushingCat;

    // Painted water layers: the box refuses to slide onto any of these tiles.
    private static readonly string[] WaterNames =
        { "water (non trigger)", "water (trigger zone)" };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        var list = new System.Collections.Generic.List<Tilemap>();
        foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (System.Array.IndexOf(WaterNames, tm.gameObject.name) >= 0)
                list.Add(tm);
        water = list.ToArray();
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

    // Checks the middle and both corners of the box's leading edge, one step ahead
    bool WaterAhead(Vector2 dir, float step)
    {
        if (col == null || water == null)
            return false;
        Bounds b = col.bounds;
        Vector2 front = (Vector2)b.center + Vector2.Scale(dir, b.extents) + dir * (step + 0.02f);
        // Perpendicular half-width, pulled in slightly so we don't sample the neighbouring lane
        Vector2 side = Vector2.Scale(new Vector2(dir.y, dir.x), b.extents) * 0.9f;
        Vector2[] probes = { front, front + side, front - side };
        foreach (var p in probes)
            foreach (var tm in water)
                if (tm != null && tm.HasTile(tm.WorldToCell(p)))
                    return true;
        return false;
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
        if (n == 0 && !WaterAhead(dir, step))
            rb.MovePosition(rb.position + dir * step);
    }
}
