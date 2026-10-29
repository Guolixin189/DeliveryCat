using UnityEngine;

// Small directional arrow growing out of the player's feet, pointing toward
// the current objective: the pickup zone when not carrying, the target dropoff
// zone when carrying. Same target logic and colors as the corner compass
// (DirectionPointer). The arrow is created at runtime as a child of the
// player, so no scene setup is needed. It sits fixed below the feet
// (feetOffset) and rotates in place to point toward the target. It stays hidden
// when there is no active target (for example in the tutorial, which has no
// DeliveryManager).
public class FootDirectionPointer : MonoBehaviour
{
    [Header("Arrow")]
    public Vector3 feetOffset = new Vector3(0f, -0.55f, 0f);
    public int sortingOrder = 7;
    public float arrowScale = 0.45f;

    [Header("Colors")]
    public Color toPickupColor = new Color(1f, 0.72f, 0.30f, 1f);  // to pick up
    public Color toDropoffColor = new Color(0.40f, 0.80f, 1f, 1f); // to drop off

    Transform arrow;
    SpriteRenderer arrowRenderer;
    DeliveryManager deliveryManager;
    player.CatController cat;

    void Start()
    {
        cat = GetComponent<player.CatController>();
        CreateArrow();
    }

    void CreateArrow()
    {
        Sprite sprite = Resources.Load<Sprite>("FootArrow");
        if (sprite == null)
        {
            Debug.LogWarning("[FootPointer] FootArrow sprite not found under a Resources folder.");
            return;
        }
        GameObject go = new GameObject("FootArrow");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = feetOffset;
        go.transform.localScale = Vector3.one * arrowScale;
        arrowRenderer = go.AddComponent<SpriteRenderer>();
        arrowRenderer.sprite = sprite;
        arrowRenderer.sortingOrder = sortingOrder;
        arrow = go.transform;
    }

    void Update()
    {
        if (deliveryManager == null)
            deliveryManager = DeliveryManager.Instance;
        if (arrow == null)
        {
            CreateArrow();
            if (arrow == null)
                return;
        }

        Transform target;
        bool toPickup;
        if (cat != null && cat.hasPackage)
        {
            target = FindTargetDropoff();
            toPickup = false;
        }
        else if (deliveryManager != null)
        {
            target = deliveryManager.pickupZone != null ? deliveryManager.pickupZone.transform : null;
            toPickup = true;
        }
        else
        {
            target = null;
            toPickup = true;
        }

        if (target == null)
        {
            if (arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(false);
            return;
        }
        if (!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);

        Vector2 dir = (Vector2)(target.position - transform.position);
        if (dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.localRotation = Quaternion.Euler(0f, 0f, ang - 90f);
        }

        if (arrowRenderer != null)
            arrowRenderer.color = toPickup ? toPickupColor : toDropoffColor;
    }

    Transform FindTargetDropoff()
    {
        if (deliveryManager == null)
            return null;
        var zones = deliveryManager.dropOffZones;
        if (zones == null)
            return null;
        foreach (var z in zones)
        {
            if (z == null || z.indicator == null)
                continue;
            if (z.indicator.activeSelf)
                return z.transform;
        }
        return null;
    }
}
