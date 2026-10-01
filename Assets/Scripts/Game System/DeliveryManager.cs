using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    public PickupZone pickupZone;
    public DropoffZone[] dropOffZones;

    private int targetHouseIndex = -1;
    private GameManager gameManager;

    void Awake()
    {
        Instance = this;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        AssignNewTarget();
    }

    // 猫猫拿起包裹时调用：藏起取货泡泡
    public void OnPackagePickedUp()
    {
        if (pickupZone != null)
            pickupZone.SetIndicator(false);
    }

    // 猫猫送达时调用：判断对错、加减分、换下一个目标
    public void OnPackageDelivered(int houseIndex)
    {
        bool correct = (houseIndex == targetHouseIndex);
        if (gameManager != null)
            gameManager.AddScore(correct ? 100 : -100);
        Debug.Log($"[Delivery] delivered to house {houseIndex}, target was {targetHouseIndex} -> {(correct ? "+100" : "-100")}");

        AssignNewTarget();
        if (pickupZone != null)
            pickupZone.SetIndicator(true);
    }

    // 随机选一间房子作为目标，只显示它的泡泡
    // 注意：按 houseIndex 取值，不依赖 Inspector 中 dropOffZones 数组的顺序
    void AssignNewTarget()
    {
        if (dropOffZones == null || dropOffZones.Length == 0)
            return;
        DropoffZone target = dropOffZones[Random.Range(0, dropOffZones.Length)];
        if (target == null)
            return;
        targetHouseIndex = target.houseIndex;
        Debug.Log($"[Delivery] new target house: {targetHouseIndex}");
        for (int i = 0; i < dropOffZones.Length; i++)
        {
            if (dropOffZones[i] == null)
                continue;
            dropOffZones[i].SetIndicator(dropOffZones[i].houseIndex == targetHouseIndex);
        }
    }
}
