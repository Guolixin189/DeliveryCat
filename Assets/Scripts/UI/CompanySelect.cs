using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CompanySelect : MonoBehaviour
{
    public GameObject companyPanel;
    public TMP_Text companyInfoText;
    private string defaultInfoText = "Hover a company to preview its rules. Click to deliver for them!";

    void Start()
    {
        if (GameSettings.OpenCompanySelectOnMenu)
        {
            GameSettings.OpenCompanySelectOnMenu = false;
            ShowCompanyPanel();
        }
    }

    public void ShowCompanyPanel()
    {
        if (companyPanel != null)
            companyPanel.SetActive(true);
    }

    public void HideCompanyPanel()
    {
        if (companyPanel != null)
            companyPanel.SetActive(false);
    }

    // 0=Freelancer, 1=UPS, 2=USPS, 3=FedEx
    public void ChooseCompany(int index)
    {
        GameSettings.SelectedCompany = (Company)Mathf.Clamp(index, 0, 3);
        SceneManager.LoadScene("level_2");
    }

    public void ShowCompanyInfo(int index)
    {
        if (companyInfoText != null)
            companyInfoText.text = InfoText(index);
    }

    public void HideCompanyInfo()
    {
        if (companyInfoText != null)
            companyInfoText.text = defaultInfoText;
    }

    private string InfoText(int index)
    {
        switch ((Company)Mathf.Clamp(index, 0, 3))
        {
            case Company.UPS:
                return "UPS: Wait 3 seconds at the door for a signature. Patience pays: +200 pts.";
            case Company.USPS:
                return "USPS: Oversized drop-off zone. Toss the package and keep moving.";
            case Company.FedEx:
                return "FedEx: Beat the 15-second clock on every order for double points.";
            default:
                return "Freelancer: No company, no rules. Classic deliveries: +100 right, -100 wrong.";
        }
    }
}
