using UnityEngine;
using UnityEngine.UI;

public class DirectionPointer : MonoBehaviour
{
    [Header("References (leave empty to auto-find)")]
    public RectTransform arrow;            // arrow RectTransform
    public Transform player;               // cat
    public DeliveryManager deliveryManager;

    [Header("Color")]
    public Color toPickupColor = new Color(1f, 0.72f, 0.30f, 1f);  // to pick up
    public Color toDropoffColor = new Color(0.40f, 0.80f, 1f, 1f);   // to drop off

    Image arrowImage;
    player.CatController cat;

    void Start()
    {
        if (deliveryManager == null)
            deliveryManager = DeliveryManager.Instance;

        if (player == null)
        {
            cat = FindFirstObjectByType<player.CatController>();
            if (cat != null)
                player = cat.transform;
        }
        else
        {
            cat = player.GetComponent<player.CatController>();
        }

        if (arrow != null)
            arrowImage = arrow.GetComponent<Image>();
    }

    void Update()
    {
        if (arrow == null || player == null || deliveryManager == null)
            return;

        Transform target;
        bool toPickup;
        if (cat != null && cat.hasPackage)
        {
            target = FindTargetDropoff();
            toPickup = false;
        }
        else
        {
            target = deliveryManager.pickupZone != null ? deliveryManager.pickupZone.transform : null;
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

        Vector2 dir = (Vector2)(target.position - player.position);
        if (dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.rotation = Quaternion.Euler(0f, 0f, ang - 90f);
        }

        if (arrowImage != null)
            arrowImage.color = toPickup ? toPickupColor : toDropoffColor;
    }

    Transform FindTargetDropoff()
    {
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
