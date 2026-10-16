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
        // DropoffZone.Start() hides its own bubble, and Start order between
        // objects is not guaranteed. If the target zone's Start ran after
        // this one, it would switch the freshly lit target bubble back off
        // (and the direction pointer, which looks for the lit bubble, would
        // hide itself). Wait one frame so every Start has finished first.
        StartCoroutine(AssignTargetAfterStarts());
    }

    System.Collections.IEnumerator AssignTargetAfterStarts()
    {
        yield return null;
        AssignNewTarget();
    }


    public void OnPackagePickedUp()
    {
        if (pickupZone != null)
            pickupZone.SetIndicator(false);
    }

  
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
