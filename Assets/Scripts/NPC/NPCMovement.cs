using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    public bool isGameOver = false;

    private int direction = -1; 
    private SpriteRenderer spriteRenderer;
    private Tilemap[] blockers;

    // Painted static layers: a tile on any of these counts as solid for
    // NPCs, even where no collider was painted (roofs, trees, lamps...).
    private static readonly string[] BlockerNames =
        { "Building", "Building collider", "Building decor", "roof", "nature", "static obstacles", "Building behind", "static obstacle (behind)" };

    void Start(){
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
        if (isGameOver)
            return;

        Vector3 step = Vector3.right * direction * speed * Time.fixedDeltaTime;
        Vector3 next = transform.position + step;
        Vector3 probe = next + Vector3.right * direction * 0.35f;
        if (!Blocked(transform.position) && (Blocked(next) || Blocked(probe)))
        {
            direction *= -1;
            if (spriteRenderer != null)
                spriteRenderer.flipX = !spriteRenderer.flipX;
            return;
        }
        transform.position = next;
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        // 不再直接结束游戏：让玩家减速，行人被弹开
        player.CatController cat = collision.gameObject.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplySlow();
    }

    direction *= -1;
    spriteRenderer.flipX = !spriteRenderer.flipX;
}
}