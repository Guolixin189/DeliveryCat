using UnityEngine;

public class DropoffZone : MonoBehaviour
{
    public int houseIndex;          // 这间房子是几号
    public GameObject indicator;    // 思考泡泡主气泡
    public GameObject[] puffs;      // 小尾巴泡泡，从下往上：puff2, puff1, puff0
    public float bobSpeed = 2f;     // 浮动快慢
    public float bobHeight = 0.15f;  // 浮动幅度
    public float puffInterval = 0.35f; // 小泡泡冒出的间隔（秒）

    private Vector3 basePos;
    private float timer;

    void Start()
    {
        if (indicator != null)
        {
            basePos = indicator.transform.position;
            indicator.SetActive(false);
        }
    }

    void Update()
    {
        if (indicator == null || !indicator.activeSelf)
            return;
        // 主气泡上下浮动（只改位置，不碰 scale）
        indicator.transform.position =
            basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        // 小泡泡从下往上依次冒出，到齐后重新开始
        if (puffs == null || puffs.Length == 0)
            return;
        timer += Time.deltaTime;
        int n = Mathf.FloorToInt(timer / puffInterval) % (puffs.Length + 1);
        for (int i = 0; i < puffs.Length; i++)
            if (puffs[i] != null)
                puffs[i].SetActive(i < n);
    }

    public void SetIndicator(bool on)
    {
        if (indicator == null)
            return;
        indicator.SetActive(on);
        if (on)
        {
            timer = 0f;
            if (puffs != null)
                foreach (var p in puffs)
                    if (p != null)
                        p.SetActive(false);
        }
    }
}
