using UnityEngine;
using UnityEngine.UI;

// 左下角方向指针：
//   没拿包裹 -> 指向取货点（橙色）
//   拿了包裹   -> 指向当前目标房子（蓝色）
// 要求：相机不旋转（俯视 2D），此时世界方向 = 屏幕方向。
// 箭头素材默认朝上；脚本按目标角度旋转箭头。
public class DirectionPointer : MonoBehaviour
{
    [Header("引用（留空会自动查找）")]
    public RectTransform arrow;            // 箭头图片的 RectTransform
    public Transform player;               // 猫
    public DeliveryManager deliveryManager;

    [Header("颜色")]
    public Color toPickupColor = new Color(1f, 0.72f, 0.30f, 1f);  // 去取货
    public Color toDropoffColor = new Color(0.40f, 0.80f, 1f, 1f);   // 去送货

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

    // 当前目标房子 = 指示器亮着的那一栋（DeliveryManager 每轮随机指定）
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
