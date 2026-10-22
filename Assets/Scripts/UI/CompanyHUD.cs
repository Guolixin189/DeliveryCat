using UnityEngine;
using TMPro;
using player;

public class CompanyHUD : MonoBehaviour
{
    public TMP_Text companyText;
    public TMP_Text timerText;

    private CatController cat;
    private DeliveryManager deliveryManager;

    void Awake()
    {
        cat = FindFirstObjectByType<CatController>();
        deliveryManager = FindFirstObjectByType<DeliveryManager>();
    }

    void Update()
    {
        Company c = GameSettings.SelectedCompany;
        if (companyText != null)
            companyText.text = CompanyRules.ShortLabel(c);
        bool showTimer = c == Company.FedEx && cat != null && cat.hasPackage && deliveryManager != null;
        if (timerText != null)
        {
            if (timerText.gameObject.activeSelf != showTimer)
                timerText.gameObject.SetActive(showTimer);
            if (showTimer)
            {
                float left = Mathf.Max(0f, CompanyRules.OrderTimeLimit(c) - (Time.time - deliveryManager.OrderStartTime));
                timerText.text = left.ToString("F1") + "s";
                timerText.color = left < 3f ? Color.red : Color.white;
            }
        }
    }
}
