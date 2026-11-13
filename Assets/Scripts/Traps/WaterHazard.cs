using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Tilemap))]
public class WaterHazard : MonoBehaviour
{
    [Tooltip("Layers the cat can't stand on: walls, houses, trees, benches, etc. " +
             "Do NOT include the water, mud, or player layers.")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField] private float fallDuration = 0.5f;

    [Tooltip("Size of the cat at the bottom of the fall, relative to its normal size.")]
    [Range(0f, 1f)]
    [SerializeField] private float fallEndScale = 0.75f;

    [Header("Fall distance by facing direction (× the cat's height)")]
    [Tooltip("Falling in while facing down (toward the camera).")]
    [SerializeField] private float fallDistanceDown = 1f;
    [Tooltip("Falling in while facing up (away from the camera).")]
    [SerializeField] private float fallDistanceUp = 0.5f;
    [Tooltip("Falling in while facing left or right.")]
    [SerializeField] private float fallDistanceSide = 0.75f;

    [Tooltip("Seconds the cat stays hidden under the water before reappearing on the bank.")]
    [SerializeField] private float respawnDelay = 1f;

    [SerializeField] private int maxSearchRadius = 40;

    private Tilemap water;
    private bool busy;

    private static readonly Vector3Int[] Directions =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    // Tilemaps layers that are part of the water body, not ground
    private static readonly string[] NotBankNames =
        { "water (non trigger)", "water (trigger zone)" };

    private Tilemap[] notBankTilemaps;

    private void Awake()
    {
        water = GetComponent<Tilemap>();

        var list = new List<Tilemap>();
        foreach (Tilemap tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (System.Array.IndexOf(NotBankNames, tm.gameObject.name) >= 0)
                list.Add(tm);
        notBankTilemaps = list.ToArray();
    }

    private bool IsNotBankTile(Vector3 worldPos)
    {
        foreach (Tilemap tm in notBankTilemaps)
            if (tm != null && tm.HasTile(tm.WorldToCell(worldPos)))
                return true;
        return false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (busy) return;

        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat == null) return;

        // Only count it as "fallen in" once the middle of the cat's collider is over
        // a water tile
        Vector3Int cell = water.WorldToCell(other.bounds.center);
        if (!water.HasTile(cell)) return;

        StartCoroutine(FallIn(cat, other, cell));
    }

    private IEnumerator FallIn(player.CatController cat, Collider2D col, Vector3Int fellAt)
    {
        busy = true;

        Transform t = cat.transform;
        Rigidbody2D rb = cat.GetComponent<Rigidbody2D>();
        SpriteRenderer[] renderers = cat.GetComponentsInChildren<SpriteRenderer>();
        Vector3 pivotOffset = t.position - col.bounds.center; // pivot vs. collider centre
        Vector2 catSize = col.bounds.size;

        // Measure the cat's visible sprite to know how far to fall.
        Bounds visual = renderers[0].bounds;
        foreach (SpriteRenderer r in renderers) visual.Encapsulate(r.bounds);
        float height = visual.size.y;

        // Freeze the cat and turn off its physics so the fall doesn't bump into anything.
        cat.enabled = false;
        if (rb)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.simulated = false;
        }

        // Fall straight down by a distance that depends on which way the cat is facing,
        // and shrink a little as if moving away from the camera.
        Vector3 start = t.position;
        Vector3 end = start + Vector3.down * height * GetFallDistanceMultiplier(cat);
        Vector3 startScale = t.localScale;
        Vector3 endScale = startScale * fallEndScale;
        for (float e = 0f; e < fallDuration; e += Time.deltaTime)
        {
            float p = e / fallDuration;
            t.position = Vector3.Lerp(start, end, p * p);
            t.localScale = Vector3.Lerp(startScale, endScale, p);
            yield return null;
        }
        t.position = end;
        t.localScale = endScale;

        // Hide the cat while it's under the water. 
        bool[] wasEnabled = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            wasEnabled[i] = renderers[i].enabled;
            renderers[i].enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        t.localScale = startScale;

        // Move cat to nearest bank
        if (TryFindNearestBank(fellAt, catSize, out Vector3 bankCentre))
        {
            t.position = bankCentre + pivotOffset;
        }
        else
        {
            Debug.LogWarning("WaterHazard: no dry tile found within search radius.");
            t.position = start;
        }

        // Make the cat visible again after the penalty
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = wasEnabled[i];

        if (rb)
        {
            rb.position = t.position;
            rb.simulated = true;
        }
        cat.enabled = true;
        busy = false;
    }

    // Works out which way the cat is facing from the Animator's last MoveX/MoveY
    // and returns how far it should fall, as a multiple of its height.
    private float GetFallDistanceMultiplier(player.CatController cat)
    {
        Animator anim = cat.GetComponent<Animator>();
        if (anim == null) return fallDistanceDown;

        float x = anim.GetFloat("MoveX");
        float y = anim.GetFloat("MoveY");

        // Never moved yet: fall back to the default.
        if (Mathf.Abs(x) < 0.01f && Mathf.Abs(y) < 0.01f) return fallDistanceDown;

        // When moving diagonally, the larger axis decides the facing.
        if (Mathf.Abs(x) > Mathf.Abs(y))
            return fallDistanceSide;
        return y > 0f ? fallDistanceUp : fallDistanceDown;
    }

    // Breadth-first search outward from where the cat fell in. The first tile that's not on a water layer and isn't blocked by an obstacle is the closest bank.
    private bool TryFindNearestBank(Vector3Int start, Vector2 catSize, out Vector3 worldPos)
    {
        var visited = new HashSet<Vector3Int> { start };
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3Int cell = queue.Dequeue();

            if (!water.HasTile(cell))
            {
                Vector3 centre = water.GetCellCenterWorld(cell);
                bool blocked = IsNotBankTile(centre)
                            || Physics2D.OverlapBox(centre, catSize * 0.9f, 0f, obstacleLayers);
                if (!blocked)
                {
                    worldPos = centre;
                    return true;
                }
            }

            foreach (Vector3Int dir in Directions)
            {
                Vector3Int next = cell + dir;
                if (Mathf.Abs(next.x - start.x) > maxSearchRadius ||
                    Mathf.Abs(next.y - start.y) > maxSearchRadius) continue;
                if (visited.Add(next)) queue.Enqueue(next);
            }
        }

        worldPos = default;
        return false;
    }
}