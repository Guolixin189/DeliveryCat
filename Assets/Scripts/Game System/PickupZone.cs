using UnityEngine;

public class PickupZone : MonoBehaviour
{
    public GameObject indicator;   // 取货对话框泡泡
    public float bobSpeed = 2f;    // 浮动快慢
    public float bobHeight = 0.15f; // 浮动幅度

    private Vector3 basePos;

    void Start()
    {
        if (indicator != null)
        {
            basePos = indicator.transform.position;
            indicator.SetActive(true);
        }
    }

    void Update()
    {
        if (indicator == null || !indicator.activeSelf)
            return;
        // 上下浮动（只改位置，不碰 scale）
        indicator.transform.position =
            basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    public void SetIndicator(bool on)
    {
        if (indicator != null)
            indicator.SetActive(on);
    }
}
