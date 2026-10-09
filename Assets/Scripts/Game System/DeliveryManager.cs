using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    public PickupZone pickupZone;
    public DropoffZone[] dropOffZones;

    private int targetHouseIndex = -1;
    private GameManager gameManager;
    public AudioClip deliverySuccessSound;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        gameManager = FindFirstObjectByType<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
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
        
        if (correct && deliverySuccessSound != null && audioSource != null){
        audioSource.PlayOneShot(deliverySuccessSound);
         }
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
