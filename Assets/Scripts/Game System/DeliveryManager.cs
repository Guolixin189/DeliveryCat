
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    public PickupZone pickupZone;
    public DropoffZone[] dropOffZones;

    public Company company = Company.Freelancer;

    // True from the moment a package is picked up until it is delivered.
    // The minimap uses this to hide the target house before pickup.
    public bool HasPackage { get; private set; }

    // Combo: consecutive correct deliveries. A wrong delivery resets it.
    public int Combo { get; private set; }
    public int BestCombo { get; private set; }

    // Score multiplier from the combo: 1x for the first correct
    // delivery, 1.5x for the second in a row, 2x from the third on.
    public float ComboMultiplier
    {
        get
        {
            if (Combo <= 1) return 1f;
            if (Combo == 2) return 1.5f;
            return 2f;
        }
    }

    [Header("Tip orders")]
    public float tipChance = 0.3f;      // chance an order is a tip order
    public int tipAmount = 50;          // extra points for an on-time tip delivery
    public float tipTimeLimit = 20f;    // seconds from pickup to earn the tip
    public bool IsTipOrder { get; private set; }

    [Header("Perfect delivery")]
    public int perfectBonus = 50;       // extra points for a delivery with no hits
    public int PerfectDeliveries { get; private set; }

    // True if the player hit a pedestrian or a trap at any point in
    // this level (used for the "Perfect Delivery" results title).
    public bool EverHitObstacle { get; private set; }

    // True if the player hit a pedestrian or a trap during the current
    // order (from pickup to delivery). Reset on every pickup.
    private bool hitObstacleThisOrder;
    private SpriteRenderer tipIcon;
    private Vector3 tipIconBasePos;

    private int targetHouseIndex = -1;
    private DropoffZone targetZone;
    private GameManager gameManager;
    private float orderStartTime = 0f;

    void Awake()
    {
        Instance = this;
        company = GameSettings.SelectedCompany;
        gameManager = FindFirstObjectByType<GameManager>();
        BuildTipIcon();
    }

    void Update()
    {
        // Bob the tip coin above the target house.
        if (tipIcon != null && tipIcon.gameObject.activeSelf)
            tipIcon.transform.position = tipIconBasePos + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;
    }

    // Called by the player when hitting a pedestrian or a trap.
    public void MarkObstacleHit()
    {
        hitObstacleThisOrder = true;
        EverHitObstacle = true;
    }

    void BuildTipIcon()
    {
        Sprite coin = Resources.Load<Sprite>("coin_tip");
        if (coin == null)
        {
            Debug.LogWarning("[Delivery] coin_tip sprite not found in Resources.");
            return;
        }
        GameObject go = new GameObject("TipIcon");
        tipIcon = go.AddComponent<SpriteRenderer>();
        tipIcon.sprite = coin;
        tipIcon.sortingOrder = 10;
        go.SetActive(false);
    }

    void ShowTipIcon(DropoffZone target)
    {
        if (tipIcon == null || target == null)
            return;
        tipIconBasePos = target.transform.position + new Vector3(0f, 1.6f, 0f);
        tipIcon.transform.position = tipIconBasePos;
        tipIcon.gameObject.SetActive(true);
    }

    void HideTipIcon()
    {
        if (tipIcon != null)
            tipIcon.gameObject.SetActive(false);
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
        orderStartTime = Time.time;
        HasPackage = true;
        // A fresh order starts clean: no hits yet.
        hitObstacleThisOrder = false;
        if (pickupZone != null)
            pickupZone.SetIndicator(false);
        // Now that the player has the package, reveal the target house.
        for (int i = 0; i < dropOffZones.Length; i++)
        {
            if (dropOffZones[i] == null)
                continue;
            dropOffZones[i].SetIndicator(dropOffZones[i].houseIndex == targetHouseIndex);
        }
        // Reveal the tip coin together with the target bubble, so the
        // destination is not given away before pickup.
        if (IsTipOrder && targetZone != null)
            ShowTipIcon(targetZone);
    }

    public bool IsTargetHouse(int houseIndex)
    {
        return houseIndex == targetHouseIndex;
    }

    public float OrderStartTime => orderStartTime;

  
    public void OnPackageDelivered(int houseIndex)
    {
        bool correct = (houseIndex == targetHouseIndex);
        int points;
        int tip = 0;
        int perfect = 0;
        if (correct)
        {
            if (company == Company.FedEx && Time.time - orderStartTime <= CompanyRules.OrderTimeLimit(company))
                points = CompanyRules.FedExOnTimePoints;
            else
                points = CompanyRules.CorrectPoints(company);
            Combo++;
            BestCombo = Mathf.Max(BestCombo, Combo);
            points = Mathf.RoundToInt(points * ComboMultiplier);
            // Tip order: on time means delivered within the tip limit.
            // Late means the tip is gone, only the base points remain.
            if (IsTipOrder && Time.time - orderStartTime <= tipTimeLimit)
                tip = tipAmount;
            // Perfect delivery: no pedestrian hit and no trap hit
            // during this order. Drive carefully for the bonus.
            if (!hitObstacleThisOrder)
            {
                perfect = perfectBonus;
                PerfectDeliveries++;
            }
            points += tip + perfect;
            if (gameManager != null)
                gameManager.PackageDelivered();
        }

        else
        {
            points = CompanyRules.WrongPoints(company);
            Combo = 0;
        }
        if (gameManager != null)
            gameManager.AddScore(points);
        Debug.Log($"[Delivery] delivered to house {houseIndex}, target was {targetHouseIndex} -> {(points >= 0 ? "+" : "")}{points} [{company}]" + (tip > 0 ? $" (+{tip} tip)" : "") + (perfect > 0 ? $" (+{perfect} perfect)" : ""));

        HasPackage = false;
        HideTipIcon();
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
        targetZone = target;
        // Roll a tip order. The coin floats above the target house,
        // but is only revealed with the target bubble after pickup.
        IsTipOrder = Random.value < tipChance;
        HideTipIcon();
        Debug.Log($"[Delivery] new target house: {targetHouseIndex}" + (IsTipOrder ? " (tip order)" : ""));
        // The target bubble stays hidden until the package is picked up,
        // so the destination is not given away early.
        for (int i = 0; i < dropOffZones.Length; i++)
        {
            if (dropOffZones[i] == null)
                continue;
            dropOffZones[i].SetIndicator(false);
        }
    }

}
